namespace AsyncAwaitFanOut.Core.DTOs
{
    public class OrderSnapShot
    {
        public Guid OrderId { get; set; }
        public OrderDto? Order { get; set; }
        public PaymentDto? Payment { get; set; }
        public ShipmentDto? Shipment { get; set; }
        public List<string> Errors { get; set; } = new();

        public OrderSnapShot() { }

        public OrderSnapShot(
            Guid orderId,
            OrderDto? order,
            PaymentDto? payment,
            ShipmentDto? shipment,
            List<string>? errors)
        {
            OrderId = orderId;
            Order = order;
            Payment = payment;
            Shipment = shipment;
            Errors = errors ?? new List<string>();
        }
    }
}
