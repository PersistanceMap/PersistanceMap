﻿using NUnit.Framework;
using PersistanceMap.Test.BusinessObjects;
using System.Linq;

namespace PersistanceMap.Test.Integration
{
    [TestFixture]
    public class JoinTests : TestBase
    {
        [Test]
        public void SimpleJoinWithProjectionTest()
        {
            var dbConnection = new DatabaseConnection(new SqlContextProvider(ConnectionString));
            using (var context = dbConnection.Open())
            {
                var orders = context.From<Orders>().Join<OrderDetails>((det, order) => det.OrderID == order.OrderID).Select<OrderWithDetail>();

                Assert.IsTrue(orders.Any());
                Assert.IsFalse(string.IsNullOrEmpty(orders.First().ShipName));
                Assert.IsTrue(orders.First().ProductID > 0);
            }
        }

        [Test]
        public void SimpleJoinInFromWithProjectionTest()
        {
            var dbConnection = new DatabaseConnection(new SqlContextProvider(ConnectionString));
            using (var context = dbConnection.Open())
            {
                var orders = context.From<Orders, OrderDetails>((det, order) => det.OrderID == order.OrderID).Select<OrderWithDetail>();
                /* *Expected Query*
                select CustomerID, EmployeeID, OrderDate, RequiredDate, ShippedDate, ShipVia, Freight, ShipName, ShipAddress, ShipCity, ShipRegion, ShipPostalCode, ShipCountry, ProductID, UnitPrice, Quantity, Discount 
                from Orders 
                join OrderDetails on (OrderDetails.OrderID = Orders.OrderID)
                */

                Assert.IsTrue(orders.Any());
                Assert.IsFalse(string.IsNullOrEmpty(orders.First().ShipName));
                Assert.IsTrue(orders.First().ProductID > 0);
            }
        }

        [Test]
        public void SimpleJoinWithOnWithProjectionTest()
        {
            var dbConnection = new DatabaseConnection(new SqlContextProvider(ConnectionString));
            using (var context = dbConnection.Open())
            {
                var orders = context.From<Orders>().Join<OrderDetails>(opt => opt.On((det, order) => det.OrderID == order.OrderID)).Select<OrderWithDetail>();

                Assert.IsTrue(orders.Any());
                Assert.IsFalse(string.IsNullOrEmpty(orders.First().ShipName));
                Assert.IsTrue(orders.First().ProductID > 0);
            }
        }

        [Test]
        public void SimpleJoinWithOnAndIdentifierOptionWithProjectionTest()
        {
            var dbConnection = new DatabaseConnection(new SqlContextProvider(ConnectionString));
            using (var context = dbConnection.Open())
            {
                var orders = context.From<Orders>().Join<OrderDetails>(opt => opt.As(() => "detail"), opt => opt.On((det, order) => det.OrderID == order.OrderID)).Select<OrderWithDetail>();

                Assert.IsTrue(orders.Any());
                Assert.IsFalse(string.IsNullOrEmpty(orders.First().ShipName));
                Assert.IsTrue(orders.First().ProductID > 0);
            }
        }

        [Test]
        public void SimpleJoinWithOnAndIdentifiersWithProjectionTest()
        {
            var dbConnection = new DatabaseConnection(new SqlContextProvider(ConnectionString));
            using (var context = dbConnection.Open())
            {
                var orders = context.From<Orders>(
                        opt => opt.As(() => "orders"))
                    .Join<OrderDetails>(
                        opt => opt.As(() => "detail"), 
                        opt => opt.On("orders", (detail, order) => detail.OrderID == order.OrderID))
                    .Select<OrderWithDetail>();

                Assert.IsTrue(orders.Any());
                Assert.IsFalse(string.IsNullOrEmpty(orders.First().ShipName));
                Assert.IsTrue(orders.First().ProductID > 0);
            }
        }

        [Test]
        public void SimpleJoinWithOnAndOrWithProjectionTest()
        {
            var dbConnection = new DatabaseConnection(new SqlContextProvider(ConnectionString));
            using (var context = dbConnection.Open())
            {
                //TODO: Or allways returns false! create connection that realy works!
                var orders = context.From<Orders>()
                    .Join<OrderDetails>(opt => opt.On((detail, order) => detail.OrderID == order.OrderID), opt => opt.Or((detail, order) => false))
                    .Select<OrderWithDetail>();

                //TODO: Or allways returns false! create connection that realy works!
                Assert.Fail();

                Assert.IsTrue(orders.Any());
                Assert.IsFalse(string.IsNullOrEmpty(orders.First().ShipName));
                Assert.IsTrue(orders.First().ProductID > 0);
            }
        }

        [Test]
        public void SimpleJoinWithOnAndAndWithProjectionTest()
        {
            var dbConnection = new DatabaseConnection(new SqlContextProvider(ConnectionString));
            using (var context = dbConnection.Open())
            {
                //TODO: And allways returns false! create connection that realy works!
                var orders = context.From<Orders>()
                    .Join<OrderDetails>(opt => opt.On((detail, order) => detail.OrderID == order.OrderID), opt => opt.And((detail, order) => false))
                    .Select<OrderWithDetail>();

                //TODO: And allways returns false! create connection that realy works!
                Assert.Fail();

                Assert.IsTrue(orders.Any());
                Assert.IsFalse(string.IsNullOrEmpty(orders.First().ShipName));
                Assert.IsTrue(orders.First().ProductID > 0);
            }
        }

        [Test]
        public void SimpleJoinWithOnAndIncludeTest()
        {
            var dbConnection = new DatabaseConnection(new SqlContextProvider(ConnectionString));
            using (var context = dbConnection.Open())
            {
                // join using include
                var orders = context
                    .From<Orders>()
                    .Join<OrderDetails>(opt => opt.On((det, order) => det.OrderID == order.OrderID), opt => opt.Include(i => i.OrderID))
                    .Select<OrderDetails>();

                /* *Expected Query*
                 select OrderDetails.OrderID, ProductID, UnitPrice, Quantity, Discount 
                 from Orders 
                 join OrderDetails on (OrderDetails.OrderID = Orders.OrderID)
                */

                Assert.IsTrue(orders.Any());
                Assert.IsTrue(orders.First().OrderID > 0);
                Assert.IsTrue(orders.First().ProductID > 0);
            }
        }

        [Test]
        public void MultipleJoinsWithOnAndIncludeTest()
        {
            var dbConnection = new DatabaseConnection(new SqlContextProvider(ConnectionString));
            using (var context = dbConnection.Open())
            {
                // multiple joins using On<T> with include
                var orders = context
                    .From<Orders>()
                    .Join<OrderDetails>(opt => opt.On((det, order) => det.OrderID == order.OrderID))
                    .Join<Products>(
                        opt => opt.On<OrderDetails>((product, det) => product.ProductID == det.ProductID),
                        opt => opt.Include(p => p.ProductID),
                        opt => opt.Include(p => p.UnitPrice))
                    .Select<OrderWithDetail>();

                /* *Expected Query*
                 select Products.ProductID, Products.UnitPrice, CustomerID, EmployeeID, OrderDate, RequiredDate, ShippedDate, ShipVia, Freight, ShipName, ShipAddress, ShipCity, ShipRegion, ShipPostalCode, ShipCountry, Quantity, Discount 
                 from Orders 
                 join OrderDetails on (OrderDetails.OrderID = Orders.OrderID)
                 join Products on (Products.ProductID = OrderDetails.ProductID)
                */

                Assert.IsTrue(orders.Any());
                Assert.IsFalse(string.IsNullOrEmpty(orders.First().ShipName));
                Assert.IsTrue(orders.First().ProductID > 0);
            }
        }
    }
}
