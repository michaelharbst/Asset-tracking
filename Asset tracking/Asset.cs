using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Asset_tracking
{


    public abstract class Asset
    {
        public int AssetId { get; set; }
        public AssetType AssetType { get; protected set; }
        public string SerialNumber { get; set; }
        public string? EmployeeUserName { get; set; }
        public DateTime WarrantyExpirationDate { get; set; }
        public Price Price { get; set; }

        public string Brand { get; set; }
        public string Model { get; set; }
        public DateTime PurchaseDate { get; set; }
        public Office Office { get; set; }

        protected Asset() { } // EF Core requires a parameterless constructor for entity classes. It can be private or protected, but it must exist.

        protected Asset(Price price, DateTime purchaseDate, string brand, string model, Office office, string serialNumber, string? employeeUserName, DateTime warrantyExpirationDate    )
        {
            // AssetId = nextId++; // assign ID,  so it is uniqiue, no matter how the asset is generated.
            Price = price;
            Brand = brand;
            Model = model;
            PurchaseDate = purchaseDate;
            Office = office;
            SerialNumber = serialNumber;
            EmployeeUserName = employeeUserName;
            WarrantyExpirationDate = warrantyExpirationDate;
        }

    }


    public class Smartphone : Asset
    {
        public Smartphone() { } // EF Core requires a parameterless constructor for entity classes. It can be private or protected, but it must exist.
        public Smartphone(Price price, DateTime purchaseDate, string brand, string model, Office office, string serialNumber, string? employeeUserName, DateTime warrantyExpirationDate) : base(price, purchaseDate, brand, model, office, serialNumber, employeeUserName, warrantyExpirationDate)
        {
            AssetType = AssetType.Smartphone;
        }
    }
    public class Computer : Asset
    {
        public Computer() { } // EF Core requires a parameterless constructor for entity classes. It can be private or protected, but it must exist.
        public Computer(Price price, DateTime purchaseDate, string brand, string model, Office office, string serialNumber, string? employeeUserName, DateTime warrantyExpirationDate) : base(price, purchaseDate, brand, model, office, serialNumber, employeeUserName, warrantyExpirationDate)
        {
            AssetType = AssetType.Computer; 
        }
    }
}



