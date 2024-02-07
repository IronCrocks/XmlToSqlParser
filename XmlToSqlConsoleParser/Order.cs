namespace XmlToSqlConsoleParser
{
    internal class Order
    {
        public int Id { get; set; }
        public string? Number { get; set; }
        public DateTime Date { get; set; }
        public float Cost { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
    }
}
