using AsyncAwaitFanOut.Core.DTOs;

namespace AsyncAwaitFanOut.Core.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDto> GetPaymentAsync(Guid orderId, CancellationToken ct);
    }
}
