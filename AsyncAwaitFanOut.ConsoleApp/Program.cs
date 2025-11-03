using AsyncAwaitFanOut.Core.Services;

namespace AsyncAwaitFanOut.ConsoleApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Demo wiring (no DI container needed for a console sample)
            var orderService = new OrderService();
            var paymentService = new PaymentService();
            var shippingService = new ShippingService();

            var orderIds = Enumerable.Range(1, 8).Select(_ => Guid.NewGuid()).ToList();
            using var cts = new CancellationTokenSource();

            var snapshotService = new OrderSnapshotService(
                orderService, paymentService, shippingService);

            var snapshots = await snapshotService.GetOrderSnapshotsAsync(
                orderIds,
                maxConcurrency: 3,
                perCallTimeout: TimeSpan.FromMilliseconds(1500),
                cts.Token);

            // Pretty print a tiny report
            Console.WriteLine("=== Order Snapshots ===");
            foreach (var s in snapshots)
            {
                var errs = (s.Errors?.Count ?? 0) == 0 ? "OK" : string.Join(" | ", s.Errors!);
                Console.WriteLine($"- {s.OrderId} :: " +
                                  $"Order? {(s.Order is null ? "no" : "yes")}, " +
                                  $"Payment? {(s.Payment is null ? "no" : "yes")}, " +
                                  $"Shipment? {(s.Shipment is null ? "no" : "yes")} :: " +
                                  $"{errs}");
            }

            Console.WriteLine("\nDone.");
        }
    }
}
