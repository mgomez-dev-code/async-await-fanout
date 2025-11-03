using AsyncAwaitFanOut.Core.DTOs;
using AsyncAwaitFanOut.Core.Interfaces;

namespace AsyncAwaitFanOut.Core.Services
{
    public sealed class OrderService : IOrderService
    {
        public async Task<OrderDto> GetOrderAsync(Guid orderId, CancellationToken ct)
        {
            // Simulate 1–3s latency
            var delay = Random.Shared.Next(1_000, 3_000);
            await Task.Delay(delay, ct).ConfigureAwait(false);

            return new OrderDto(
                orderId,
                $"Customer_{orderId.ToString()[..4]}",
                DateTime.UtcNow,
                Random.Shared.Next(100, 5_000));
        }
    }
}
