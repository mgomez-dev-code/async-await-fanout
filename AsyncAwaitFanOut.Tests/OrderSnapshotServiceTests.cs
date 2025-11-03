using AsyncAwaitFanOut.Core.DTOs;
using AsyncAwaitFanOut.Core.Interfaces;
using AsyncAwaitFanOut.Core.Services;
using Moq;

namespace AsyncAwaitFanOut.Tests
{
    public class OrderSnapshotServiceTests
    {
        [Fact]
        public async Task GetOrderSnapshotsAsync_ShouldReturnSnapshots_WhenAllServicesSucceed()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            var mockOrder = new Mock<IOrderService>();
            var mockPayment = new Mock<IPaymentService>();
            var mockShip = new Mock<IShippingService>();

            mockOrder.Setup(s => s.GetOrderAsync(orderId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new OrderDto(orderId, "Test", DateTime.UtcNow, 100));

            mockPayment.Setup(s => s.GetPaymentAsync(orderId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new PaymentDto(orderId, "Approved", DateTime.UtcNow));

            mockShip.Setup(s => s.GetShipmentAsync(orderId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ShipmentDto(orderId, "UPS", "TRACK-1234"));

            var sut = new OrderSnapshotService(mockOrder.Object, mockPayment.Object, mockShip.Object);

            // Act
            var result = await sut.GetOrderSnapshotsAsync(
                new[] { orderId }, maxConcurrency: 2, perCallTimeout: TimeSpan.FromSeconds(1), CancellationToken.None);

            // Assert
            var snapshot = Assert.Single(result);
            Assert.NotNull(snapshot.Order);
            Assert.NotNull(snapshot.Payment);
            Assert.NotNull(snapshot.Shipment);
            Assert.Empty(snapshot.Errors);
        }

        [Fact]
        public async Task GetOrderSnapshotsAsync_ShouldHandleTimeouts_AndReturnPartialResults()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var mockOrder = new Mock<IOrderService>();
            var mockPayment = new Mock<IPaymentService>();
            var mockShip = new Mock<IShippingService>();

            // Simula timeout en PaymentService
            mockOrder.Setup(s => s.GetOrderAsync(orderId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new OrderDto(orderId, "Customer", DateTime.UtcNow, 200));

            mockPayment.Setup(s => s.GetPaymentAsync(orderId, It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new TaskCanceledException()); // Timeout

            mockShip.Setup(s => s.GetShipmentAsync(orderId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ShipmentDto(orderId, "DHL", "TRACK-9999"));

            var sut = new OrderSnapshotService(mockOrder.Object, mockPayment.Object, mockShip.Object);

            // Act
            var result = await sut.GetOrderSnapshotsAsync(
                new[] { orderId }, maxConcurrency: 1, perCallTimeout: TimeSpan.FromMilliseconds(100), CancellationToken.None);

            // Assert
            var snapshot = Assert.Single(result);
            Assert.NotNull(snapshot.Order);
            Assert.NotNull(snapshot.Shipment);
            Assert.Single(snapshot.Errors);
            Assert.Contains("PaymentService", snapshot.Errors[0]);
        }

        [Fact]
        public async Task GetOrderSnapshotsAsync_ShouldReturnEmpty_WhenInputListIsEmpty()
        {
            // Arrange
            var sut = new OrderSnapshotService(
                Mock.Of<IOrderService>(),
                Mock.Of<IPaymentService>(),
                Mock.Of<IShippingService>());

            // Act
            var result = await sut.GetOrderSnapshotsAsync(
                Array.Empty<Guid>(), maxConcurrency: 2, perCallTimeout: TimeSpan.FromMilliseconds(1000), CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetOrderSnapshotsAsync_ShouldRespectMaxConcurrency()
        {
            // Arrange
            var counter = 0;
            var lockObj = new object();
            var peakConcurrency = 0;

            var mockOrder = new Mock<IOrderService>();
            mockOrder.Setup(s => s.GetOrderAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                     .Returns<Guid, CancellationToken>(async (_, ct) =>
                     {
                         lock (lockObj)
                         {
                             counter++;
                             peakConcurrency = Math.Max(peakConcurrency, counter);
                         }

                         await Task.Delay(200, ct);

                         lock (lockObj)
                         {
                             counter--;
                         }

                         return new OrderDto(Guid.NewGuid(), "X", DateTime.UtcNow, 100);
                     });

            var sut = new OrderSnapshotService(
                mockOrder.Object,
                Mock.Of<IPaymentService>(),
                Mock.Of<IShippingService>());

            var orderIds = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToList();

            // Act
            await sut.GetOrderSnapshotsAsync(
                orderIds, maxConcurrency: 3, perCallTimeout: TimeSpan.FromSeconds(1), CancellationToken.None);

            // Assert
            Assert.True(peakConcurrency <= 3, $"Peak concurrency exceeded limit: {peakConcurrency}");
        }
    }
}
