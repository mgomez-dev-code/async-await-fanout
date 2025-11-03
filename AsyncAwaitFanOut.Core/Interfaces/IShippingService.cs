using AsyncAwaitFanOut.Core.DTOs;

namespace AsyncAwaitFanOut.Core.Interfaces
{
    public interface IShippingService
    {
        Task<ShipmentDto> GetShipmentAsync(Guid orderId, CancellationToken ct);
    }
}
