namespace AsyncAwaitFanOut.Core.DTOs
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }

        public OrderDto() { }

        public OrderDto(Guid orderId, string customerName, DateTime date, decimal amount)
        {
            OrderId = orderId;
            CustomerName = customerName;
            Date = date;
            Amount = amount;
        }
    }
}
