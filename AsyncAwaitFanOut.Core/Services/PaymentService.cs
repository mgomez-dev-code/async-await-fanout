using AsyncAwaitFanOut.Core.DTOs;
using AsyncAwaitFanOut.Core.Interfaces;

namespace AsyncAwaitFanOut.Core.Services
{
    public sealed class PaymentService : IPaymentService
    {
        public async Task<PaymentDto> GetPaymentAsync(Guid orderId, CancellationToken ct)
        {
            var delay = Random.Shared.Next(500, 2_000);
            await Task.Delay(delay, ct).ConfigureAwait(false);

            return new PaymentDto(
                orderId,
                "Approved",
                DateTime.UtcNow.AddMinutes(-Random.Shared.Next(1, 60)));
        }
    }
}
