﻿
namespace PersistanceMap.Test.TableTypes
{
    public class OrderDetails
    {
        public int OrdersID { get; set; }

        public int ProductID { get; set; }

        public double UnitPrice { get; set; }

        public int Quantity { get; set; }

        public double Discount { get; set; }
    }
}