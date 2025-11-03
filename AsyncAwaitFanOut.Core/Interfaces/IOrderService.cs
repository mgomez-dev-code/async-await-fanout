using AsyncAwaitFanOut.Core.DTOs;

namespace AsyncAwaitFanOut.Core.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> GetOrderAsync(Guid orderId, CancellationToken ct);
    }
}
