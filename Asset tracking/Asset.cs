using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Asset_tracking
{


    public abstract class Asset
    {
        public int AssetId { get; set; }
        public  abstract AssetType  AssetType{ get; }
        public Price Price { get; set; }

        public string Brand { get; set; }
        public string Model { get; set; }
        public DateTime PurchaseDate { get; set; }
        public Office Office { get; set; }
        private static int nextId = 1;

        public Asset(Price price, DateTime purchaseDate, string brand, string model, Office office)
        {
            AssetId = nextId++; // assign ID,  so it is uniqiue, no matter how the asset is generated.
            Price = price;
            Brand = brand;
            Model = model;
            PurchaseDate = purchaseDate;
            Office = office;
        }

    }


    public class Smartphone : Asset
{
        public override AssetType AssetType =>  AssetType.Smartphone; //For loaded desrialized data can be converted correctly
    public Smartphone(Price price, DateTime purchaseDate, string brand, string model, Office office) : base(price, purchaseDate, brand, model, office)
    {
    }
}
    public class Computer : Asset
    {
        public override AssetType AssetType => AssetType.Computer; //For loaded desrialized data can be converted correctly
        public Computer(Price price, DateTime purchaseDate, string brand, string model, Office office) : base(price, purchaseDate, brand, model, office)
        {
        }
    }
}



