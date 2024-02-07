using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Xml;

namespace XmlToSqlConsoleParser
{
    internal class Program
    {
        private static readonly string _fileName = "SqlData.xml";
        private static readonly string _format = "yyyy.MM.dd";

        static void Main(string[] args)
        {
            string path = GetFilePath();

            if (!File.Exists(path))
            {
                return;
            }

            var ordersXmlData = GetXmlRoot(path);

            if (ordersXmlData is null) return;

            CreateDatabase();

            var orders = GetOrders(ordersXmlData);

            using var dbContext = new ApplicationContext();

            dbContext.Orders.AddRange(orders);
            dbContext.SaveChanges();

            Console.WriteLine("Данные успешно заружены!");
            Console.ReadLine();
        }

        private static string GetFilePath()
        {
            string currentDirectoryPath = Environment.CurrentDirectory;
            return Path.Combine(currentDirectoryPath, _fileName);
        }

        private static XmlElement? GetXmlRoot(string path)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(path);

            return xmlDocument.DocumentElement;
        }

        /// <summary>
        /// Пересоздает базу данных для тестирования скрипта.
        /// </summary>
        private static void CreateDatabase()
        {
            using var db = new ApplicationContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }

        private static IEnumerable<Order> GetOrders(XmlElement ordersXmlData)
        {
            var orders = new List<Order>();

            using var dbContext = new ApplicationContext();

            foreach (XmlElement orderXmlData in ordersXmlData)
            {
                var order = GetOrder(orderXmlData, dbContext);
                orders.Add(order);
            }

            return orders;
        }

        private static Order GetOrder(XmlElement orderXmlData, ApplicationContext dbContext)
        {
            var order = new Order();

            foreach (XmlNode node in orderXmlData.ChildNodes)
            {
                switch (node.Name)
                {
                    case "no":
                        SetOrderNumber(order, node.InnerText);
                        break;
                    case "reg_date":
                        SetOrderDate(order, node.InnerText);
                        break;
                    case "sum":
                        SetOrderCost(order, node.InnerText);
                        break;
                    case "product":
                        SetOrderItem(order, node, dbContext);
                        break;
                    case "user":
                        SetOrderCustomer(order, node, dbContext);
                        break;
                    default:
                        throw new InvalidOperationException("Некорректное название тэга: " + node.Name);
                }
            }

            return order;
        }

        private static void SetOrderNumber(Order order, string number) => order.Number = number;

        private static void SetOrderDate(Order order, string date)
        {
            order.Date = DateTime.ParseExact(date, _format, CultureInfo.InvariantCulture);
        }

        private static void SetOrderCost(Order order, string cost)
        {
            order.Cost = float.TryParse(cost, CultureInfo.InvariantCulture, out float orderCost)
                ? orderCost
                : throw new InvalidOperationException("Некорректное значение стоимости заказа: " + cost);
        }

        private static void SetOrderItem(Order order, XmlNode productData, ApplicationContext dbContext)
        {
            var orderItem = GetOrderItem(productData, dbContext);
            order.OrderItems.Add(orderItem);
        }

        private static OrderItem GetOrderItem(XmlNode productData, ApplicationContext dbContext)
        {
            var product = GetProduct(productData, out int count, dbContext);

            return new OrderItem
            {
                Product = product,
                Count = count
            };
        }

        private static Product GetProduct(XmlNode productXmlData, out int count, ApplicationContext dbContext)
        {
            var product = FindOrCreateProduct(productXmlData, dbContext);
            count = 0;

            foreach (XmlNode node in productXmlData.ChildNodes)
            {
                switch (node.Name)
                {
                    case "quantity":
                        count = int.TryParse(node.InnerText, out int countValue) ? countValue :
                            throw new InvalidOperationException("Некорректное значение количества товаров: " + node.InnerText);
                        break;
                    case "price":
                        product.Price = float.TryParse(node.InnerText, CultureInfo.InvariantCulture, out float price) ? price :
                            throw new InvalidOperationException("Некорректное значение стоимости товара: " + node.InnerText);
                        break;
                    case "name":
                        break;
                    default:
                        throw new InvalidOperationException("Некорректное название тэга: " + node.Name);
                }
            }

            return product;
        }

        private static Product FindOrCreateProduct(XmlNode productXmlData, ApplicationContext dbContext)
        {
            var productNameData = productXmlData.SelectSingleNode("name") ??
                throw new InvalidOperationException("Некорректный формат входных данных: отсутствует наименование продукта.");

            var product = dbContext.Products.FirstOrDefault(p => p.Name.Equals(productNameData.InnerText)) ??
                dbContext.Products.Local.FirstOrDefault(p => p.Name.Equals(productNameData.InnerText));

            if (product is null)
            {
                product = CreateProduct(productNameData.InnerText);
                dbContext.Products.Add(product);
            }

            return product;
        }

        private static Product CreateProduct(string productName) => new() { Name = productName };

        private static void SetOrderCustomer(Order order, XmlNode customerData, ApplicationContext dbContext)
        {
            var customer = GetCustomer(customerData, dbContext);
            order.Customer = customer;
        }

        private static Customer GetCustomer(XmlNode customerXmlData, ApplicationContext dbContext)
        {
            var customer = FindOrCreateCustomer(customerXmlData, dbContext);

            foreach (XmlNode node in customerXmlData.ChildNodes)
            {
                switch (node.Name)
                {
                    case "fio":
                        customer.Name = node.InnerText;
                        break;
                    case "email":
                        customer.Email = node.InnerText;
                        break;
                    default:
                        throw new InvalidOperationException("Некорректное название тэга: " + node.Name);
                }
            }

            return customer;
        }

        private static Customer FindOrCreateCustomer(XmlNode customerXmlData, ApplicationContext dbContext)
        {
            var customerNameData = customerXmlData.SelectSingleNode("fio") ??
                throw new InvalidOperationException("Некорректный формат входных данных: отсутствует ФИО покупателя.");

            var customer = dbContext.Customers.FirstOrDefault(p => p.Name.Equals(customerNameData.InnerText)) ??
                dbContext.Customers.Local.FirstOrDefault(p => p.Name.Equals(customerNameData.InnerText));

            if (customer is null)
            {
                customer = CreateCustomer(customerNameData.InnerText);
                dbContext.Customers.Add(customer);
            }

            return customer;
        }

        private static Customer CreateCustomer(string customerName) => new() { Name = customerName };
    }
}
