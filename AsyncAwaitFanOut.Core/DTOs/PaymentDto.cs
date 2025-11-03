namespace AsyncAwaitFanOut.Core.DTOs
{
    public class PaymentDto
    {
        public Guid OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime AuthorizedAt { get; set; } = DateTime.UtcNow;

        public PaymentDto() { }

        public PaymentDto(Guid orderId, string status, DateTime authorizedAt)
        {
            OrderId = orderId;
            Status = status;
            AuthorizedAt = authorizedAt;
        }
    }
}
