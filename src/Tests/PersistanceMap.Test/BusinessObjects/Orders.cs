﻿using System;

namespace PersistanceMap.Test.BusinessObjects
{
    public class Orders
    {
        [Ignore]
        public string IgnoreProperty { get; set; }

        public int OrderID { get; set; }

        public string CustomerID { get; set; }

        public int EmployeeID { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime RequiredDate { get; set; }

        public DateTime ShippedDate { get; set; }

        public int ShipVia { get; set; }

        public double Freight { get; set; }

        public string ShipName { get; set; }

        public string ShipAddress { get; set; }

        public string ShipCity { get; set; }

        public string ShipRegion { get; set; }

        public string ShipPostalCode { get; set; }

        public string ShipCountry { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1} {2}", OrderID, CustomerID, ShipName);
        }
    }
}