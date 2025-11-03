namespace AsyncAwaitFanOut.Core.DTOs
{
    public class ShipmentDto
    {
        public Guid OrderId { get; set; }
        public string Carrier { get; set; } = string.Empty;
        public string Tracking { get; set; } = string.Empty;

        public ShipmentDto() { }

        public ShipmentDto(Guid orderId, string carrier, string tracking)
        {
            OrderId = orderId;
            Carrier = carrier;
            Tracking = tracking;
        }
    }
}
