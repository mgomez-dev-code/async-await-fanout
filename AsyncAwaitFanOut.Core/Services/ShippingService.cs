using AsyncAwaitFanOut.Core.DTOs;
using AsyncAwaitFanOut.Core.Interfaces;

namespace AsyncAwaitFanOut.Core.Services
{
    public sealed class ShippingService : IShippingService
    {
        public async Task<ShipmentDto> GetShipmentAsync(Guid orderId, CancellationToken ct)
        {
            var delay = Random.Shared.Next(800, 2_500);
            await Task.Delay(delay, ct).ConfigureAwait(false);

            return new ShipmentDto(
                orderId,
                "DHL",
                $"TRACK-{Random.Shared.Next(10_000, 99_999)}");
        }
    }
}
