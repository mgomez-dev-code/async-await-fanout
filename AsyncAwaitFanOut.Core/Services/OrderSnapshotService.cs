using AsyncAwaitFanOut.Core.DTOs;
using AsyncAwaitFanOut.Core.Interfaces;

namespace AsyncAwaitFanOut.Core.Services
{
    /// <summary>
    /// Orchestrates a bounded fan-out to fetch partial snapshots per OrderId.
    /// - Bounded concurrency via SemaphoreSlim
    /// - Per-call timeout (linked with the outer CancellationToken)
    /// - Partial success: errors are aggregated, not thrown
    /// - No blocking (.Result/.Wait()) — async/await only
    /// </summary>
    public sealed class OrderSnapshotService
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IShippingService _shippingService;

        public OrderSnapshotService(
            IOrderService orderService,
            IPaymentService paymentService,
            IShippingService shippingService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
            _shippingService = shippingService;
        }

        public async Task<IReadOnlyList<OrderSnapShot>> GetOrderSnapshotsAsync(
            IReadOnlyList<Guid> orderIds,
            int maxConcurrency,
            TimeSpan perCallTimeout,
            CancellationToken ct)
        {
            if (orderIds is null || orderIds.Count == 0)
                return Array.Empty<OrderSnapShot>();

            if (maxConcurrency <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxConcurrency), "Must be > 0.");

            using var sem = new SemaphoreSlim(maxConcurrency);

            // Build tasks lazily with LINQ and run them in parallel with Task.WhenAll
            var tasks = orderIds.Select(id => ProcessOneAsync(id, sem, perCallTimeout, ct)).ToArray();
            var snapshots = await Task.WhenAll(tasks).ConfigureAwait(false);

            // Stable ordering by OrderId
            return snapshots.OrderBy(s => s.OrderId).ToList();
        }

        private async Task<OrderSnapShot> ProcessOneAsync(
            Guid orderId,
            SemaphoreSlim sem,
            TimeSpan perCallTimeout,
            CancellationToken ct)
        {
            await sem.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                return await FetchOneAsync(orderId, perCallTimeout, ct).ConfigureAwait(false);
            }
            finally
            {
                sem.Release();
            }
        }

        private async Task<OrderSnapShot> FetchOneAsync(
            Guid orderId,
            TimeSpan perCallTimeout,
            CancellationToken ct)
        {
            // Fire the three calls concurrently — each one with its own timeout linked to the global CT
            var orderTask = SafeCall(
                t => _orderService.GetOrderAsync(orderId, t),
                perCallTimeout,
                ct,
                serviceName: "OrderService");

            var paymentTask = SafeCall(
                t => _paymentService.GetPaymentAsync(orderId, t),
                perCallTimeout,
                ct,
                serviceName: "PaymentService");

            var shippingTask = SafeCall(
                t => _shippingService.GetShipmentAsync(orderId, t),
                perCallTimeout,
                ct,
                serviceName: "ShippingService");

            // Await all three
            var (order, e1) = await orderTask.ConfigureAwait(false);
            var (payment, e2) = await paymentTask.ConfigureAwait(false);
            var (shipment, e3) = await shippingTask.ConfigureAwait(false);

            // Aggregate non-null errors (partial success policy)
            var errors = new List<string>(3);
            if (e1 is not null) errors.Add(e1);
            if (e2 is not null) errors.Add(e2);
            if (e3 is not null) errors.Add(e3);

            return new OrderSnapShot(orderId, order, payment, shipment, errors);
        }

        /// <summary>
        /// Wraps a single async call to enforce per-call timeout + outer cancellation.
        /// Returns either the value or a string error (timeout/exception type).
        /// </summary>
        private static async Task<(T? Value, string? Error)> SafeCall<T>(
            Func<CancellationToken, Task<T>> action,
            TimeSpan timeout,
            CancellationToken outerCt,
            string serviceName)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(outerCt);
            linked.CancelAfter(timeout);

            try
            {
                var val = await action(linked.Token).ConfigureAwait(false);
                return (val, null);
            }
            // Timed out (but caller didn't cancel globally)
            catch (OperationCanceledException) when (!outerCt.IsCancellationRequested)
            {
                return (default, $"{serviceName}: timeout");
            }
            catch (Exception ex)
            {
                // Return exception type name, do not throw — keeps partial success semantics
                return (default, $"{serviceName}: {ex.GetType().Name}");
            }
        }
    }
}
