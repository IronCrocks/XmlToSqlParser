using System.Collections.Generic;

namespace XmlToSqlConsoleParser
{
    internal class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public List<Order> Orders { get; set; } = [];
    }
}
