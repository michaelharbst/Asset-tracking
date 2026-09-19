using System.Text.Json;
using Asset_tracking;

internal class AssetManager
{
    public static List<Asset> Assets = new();
    private const string FileName = "assets.json";


    internal void AddAsset()
    {
        Header("ADD ASSET");

        Console.WriteLine("Select asset type:");
        Console.WriteLine("1. Smartphone");
        Console.WriteLine("2. Computer");

        int assetType = ReadNumberInRange("Select 1-2: ", 1, 2);

        string brand = ReadRequiredText("Enter brand: ");
        string model = ReadRequiredText("Enter model: ");
        decimal priceUSD = ReadPositiveDecimal("Enter price in USD: ");
        DateTime purchaseDate = ReadPurchaseDate();

        Console.WriteLine();
        Console.WriteLine("Select office:");
        Console.WriteLine("1. Sweden");
        Console.WriteLine("2. USA");
        Console.WriteLine("3. Turkey");

        int officeChoice = ReadNumberInRange("Select 1-3: ", 1, 3);

        Office office = officeChoice switch
        {
            1 => Office.Sweden,
            2 => Office.USA,
            3 => Office.Turkey,
            _ => throw new ArgumentOutOfRangeException()
        };

        Currency localCurrency = office switch
        {
            Office.Sweden => Currency.SEK,
            Office.USA => Currency.USD,
            Office.Turkey => Currency.TRY,
            _ => throw new ArgumentOutOfRangeException()
        };

        Price price = new Price(priceUSD, localCurrency);

        Asset asset;

        if (assetType == 1)
        {
            asset = new Smartphone(
                price,
                purchaseDate,
                brand,
                model,
                office
            );
        }
        else
        {
            asset = new Computer(
                price,
                purchaseDate,
                brand,
                model,
                office
            );
        }

        string validationErrors = ValidateAsset(asset);

        if (string.IsNullOrEmpty(validationErrors))
        {
            Assets.Add(asset);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(
                $"{asset.GetType().Name} {asset.Brand} {asset.Model} " +
                $"was added with ID {asset.AssetId}."
            );
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("The asset was not added:");
            Console.WriteLine(validationErrors);
            Console.ResetColor();
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to return to the menu.");
        Console.ReadKey();
    }

    internal void SortAsset()
    {
        Header("SORT ASSETS");

        if (Assets.Count == 0)
        {
            Console.WriteLine("There are no assets to sort.");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("Sort assets by:");
        Console.WriteLine("1. Asset type and purchase date");
        Console.WriteLine("2. Office and purchase date");

        int sortChoice = ReadNumberInRange("Select 1-2: ", 1, 2);

        switch (sortChoice)
        {
            case 1:
                Assets = Assets
                    .OrderBy(asset => asset.GetType().Name)
                    .ThenBy(asset => asset.PurchaseDate)
                    .ToList();
                break;

            case 2:
                Assets = Assets
                    .OrderBy(asset => asset.Office)
                    .ThenBy(asset => asset.PurchaseDate)
                    .ToList();
                break;
        }

        Console.WriteLine();
        Console.WriteLine("Assets were sorted successfully.");
        Console.WriteLine("Press any key to view the sorted list.");
        Console.ReadKey();

        ViewAsset();
    }
    internal void SearchAsset()
    {
        Header("SEARCH ASSETS");

        Console.Write("Enter asset ID, brand or model: ");
        string searchText = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(searchText))
        {
            Console.WriteLine("No search text was entered.");
            Console.ReadKey();
            return;
        }

        bool searchIsId = int.TryParse(searchText, out int assetId);

        List<Asset> foundAssets = Assets
            .Where(asset =>
                (searchIsId && asset.AssetId == assetId) ||
                asset.Brand.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase
                ) ||
                asset.Model.Contains(
                    searchText,
                    StringComparison.OrdinalIgnoreCase
                ))
            .ToList();

        Console.WriteLine();

        if (foundAssets.Count == 0)
        {
            Console.WriteLine("No matching assets were found.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{foundAssets.Count} asset(s) found:");
            Console.ResetColor();
            Console.WriteLine();

            PrintAssetHeader();

            foreach (Asset asset in foundAssets)
            {
                PrintAsset(asset);
            }
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to return to the menu.");
        Console.ReadKey();
    }

    internal void ViewAsset()
    {
        Header("View Assets");


        Header("ASSET LIST");
        Console.WriteLine("Asset Id".PadRight(15) + "Office".PadRight(15) + "Asset".PadRight(15) + "Brand".PadRight(15) + "Model".PadRight(15) + "Price (USD)".PadRight(15) + "Price (local)".PadRight(15) + "Purchase date".PadRight(15) + "age (days)".PadRight(15) + "Status");

        TimeSpan age = new TimeSpan();
        TimeSpan threeYears = new TimeSpan(3 * 365, 0, 0, 0, 0); //timespan longest units is days, so.....
        String status = "OK";
        foreach (var asset in Assets)
        {
            age = DateTime.Now - asset.PurchaseDate;
            if (threeYears - age < new TimeSpan(90, 0, 0, 0, 0)) // less than 3 month lifetime
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                status = "Yellow";
            }
            else if (threeYears - age < new TimeSpan(180, 0, 0, 0, 0)) // less than 6 month lifetime}

            {
                Console.ForegroundColor = ConsoleColor.Red;
                status = "Red";
            }
            Console.WriteLine(asset.AssetId.ToString().PadRight(15) + asset.Office.ToString().PadRight(15) + asset.GetType().Name.ToString().PadRight(15) + asset.Brand.PadRight(15) + asset.Model.PadRight(15) + asset.Price.PriceUSD.ToString().PadRight(15) + asset.Price.PriceLocal.ToString().PadRight(15) + asset.PurchaseDate.ToShortDateString().PadRight(15) + age.Days.ToString().PadRight(15) + status);
            Console.ResetColor();
            status = "OK";
        }


        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Press any key to return to the menu");
        Console.ReadKey();



    }

    // <summary>
    //
    // Standard header that is the standard for each screen.
    // <param name = "title" ></ param >
    private static void Header(string title)
    {
        Console.Clear();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("********   " + title + "   ***********");
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
    }

    /// <summary>
    /// Default test data
    /// </summary>
    internal static void LoadData()
    {
        Price price = new Price(200, Currency.USD);
        Assets.Add(new Smartphone(price, DateTime.Now.AddMonths(-36 + 4), "Motorola", "X3", Office.USA));
        Assets.Add(new Smartphone(new Price(400, Currency.USD), DateTime.Now.AddMonths(-36 + 5), "Motorola", "X3", Office.USA));
        Assets.Add(new Smartphone(new Price(400, Currency.USD), DateTime.Now.AddMonths(-36 + 10), "Motorola", "X2", Office.USA));
        Assets.Add(new Smartphone(new Price(4500, Currency.TRY), DateTime.Now.AddMonths(-36 + 6), "Samsung", "Galaxy 10", Office.Turkey));
        Assets.Add(new Smartphone(new Price(4500, Currency.SEK), DateTime.Now.AddMonths(-36 + 7), "Samsung", "Galaxy 10", Office.Sweden));
        Assets.Add(new Smartphone(new Price(3000, Currency.SEK), DateTime.Now.AddMonths(-36 + 4), "Sony", "XPeria 7", Office.Sweden));
        Assets.Add(new Smartphone(new Price(3000, Currency.SEK), DateTime.Now.AddMonths(-36 + 5), "Sony", "XPeria 7", Office.Sweden));
        Assets.Add(new Smartphone(new Price(220, Currency.TRY), DateTime.Now.AddMonths(-36 + 12), "Siemens", "Brick", Office.Turkey));
        Assets.Add(new Computer(new Price(100, Currency.USD), DateTime.Now.AddMonths(-38), "Dell", "Desktop 900", Office.USA));
        Assets.Add(new Computer(new Price(100, Currency.USD), DateTime.Now.AddMonths(-37), "Dell", "Desktop 900", Office.USA));
        Assets.Add(new Computer(new Price(300, Currency.USD), DateTime.Now.AddMonths(-36 + 1), "Lenovo", "X100", Office.USA));
        Assets.Add(new Computer(new Price(300, Currency.USD), DateTime.Now.AddMonths(-36 + 4), "Lenovo", "X200", Office.USA));
        Assets.Add(new Computer(new Price(500, Currency.USD), DateTime.Now.AddMonths(-36 + 9), "Lenovo", "X300", Office.USA));
        Assets.Add(new Computer(new Price(1500, Currency.SEK), DateTime.Now.AddMonths(-36 + 7), "Dell", "Optiplex 100", Office.Sweden));
        Assets.Add(new Computer(new Price(1400, Currency.SEK), DateTime.Now.AddMonths(-36 + 8), "Dell", "Optiplex 200", Office.Sweden));
        Assets.Add(new Computer(new Price(1300, Currency.SEK), DateTime.Now.AddMonths(-36 + 9), "Dell", "Optiplex 300", Office.Sweden));
        Assets.Add(new Computer(new Price(1600, Currency.TRY), DateTime.Now.AddMonths(-36 + 14), "Asus", "ROG 600", Office.Turkey));
        Assets.Add(new Computer(new Price(1200, Currency.TRY), DateTime.Now.AddMonths(-36 + 4), "Asus", "ROG 500", Office.Turkey));
        Assets.Add(new Computer(new Price(1200, Currency.TRY), DateTime.Now.AddMonths(-36 + 3), "Asus", "ROG 500", Office.Turkey));
        Assets.Add(new Computer(new Price(1300, Currency.TRY), DateTime.Now.AddMonths(-36 + 2), "Asus", "ROG 500", Office.Turkey));
        SortAssets();
    }


    //following methods are usefull help methods

    private static string ReadRequiredText(string message)
    {
        while (true)
        {
            Console.Write(message);
            string input = Console.ReadLine()?.Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("The value cannot be empty.");
            Console.ResetColor();
        }
    }

    private static decimal ReadPositiveDecimal(string message)
    {
        while (true)
        {
            Console.Write(message);

            if (decimal.TryParse(Console.ReadLine(), out decimal value)
                && value > 0)
            {
                return value;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Enter a positive number.");
            Console.ResetColor();
        }
    }

    private static int ReadNumberInRange(
        string message,
        int minimum,
        int maximum)
    {
        while (true)
        {
            Console.Write(message);

            if (int.TryParse(Console.ReadLine(), out int value)
                && value >= minimum
                && value <= maximum)
            {
                return value;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                $"Enter a number between {minimum} and {maximum}."
            );
            Console.ResetColor();
        }
    }

    private static DateTime ReadPurchaseDate()
    {
        while (true)
        {
            Console.Write("Enter purchase date (yyyy-MM-dd): ");
            string input = Console.ReadLine()?.Trim() ?? "";
            bool inputOk = DateTime.TryParseExact(input, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime purchaseDate);
            if (inputOk && purchaseDate.Date <= DateTime.Today)
            {
                return purchaseDate;
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (!inputOk)
            {
                Console.WriteLine("Enter a valid date in yyyy-MM-dd format. ");
            }
            if (purchaseDate.Date > DateTime.Today)
            {
                Console.WriteLine("The purchase date cannot be in the future.");
            }
            Console.ResetColor();
        }
    }

    private static string ValidateAsset(Asset asset)
    {
        string errors = "";

        if (string.IsNullOrWhiteSpace(asset.Brand))
        {
            errors += "The asset must have a brand.\n";
        }

        if (string.IsNullOrWhiteSpace(asset.Model))
        {
            errors += "The asset must have a model.\n";
        }

        if (asset.Price.PriceUSD <= 0)
        {
            errors += "The asset must have a positive price.\n";
        }

        if (asset.PurchaseDate.Date > DateTime.Today)
        {
            errors += "The purchase date cannot be in the future.\n";
        }

        return errors;
    }

    private static void PrintAssetHeader()
    {
        Console.WriteLine(
            "Asset Id".PadRight(12) +
            "Office".PadRight(12) +
            "Asset".PadRight(15) +
            "Brand".PadRight(15) +
            "Model".PadRight(20) +
            "Price USD".PadRight(12) +
            "Price local".PadRight(15) +
            "Purchase date"
        );
    }

    private static void PrintAsset(Asset asset)
    {
        Console.WriteLine(
            asset.AssetId.ToString().PadRight(12) +
            asset.Office.ToString().PadRight(12) +
            asset.GetType().Name.PadRight(15) +
            asset.Brand.PadRight(15) +
            asset.Model.PadRight(20) +
            asset.Price.PriceUSD.ToString("0.00").PadRight(12) +
            asset.Price.PriceLocal.ToString("0.00").PadRight(15) +
            asset.PurchaseDate.ToString("yyyy-MM-dd")
        );
    }
    private static void SortAssets()
    {
        Assets = Assets
            .OrderBy(asset => asset.GetType().Name)
            .ThenBy(asset => asset.PurchaseDate)
            .ToList();
    }
    internal void Save()

    {
        JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(Assets, options);

        File.WriteAllText(FileName, json);

        Console.WriteLine("Assets saved successfully.");
        Console.ReadKey();

    }

    internal void Load()
    {
        if (!File.Exists(FileName))
        {
            LoadData();

            Console.WriteLine(
                "No saved asset file was found. Default data was loaded."
            );

            Console.ReadKey();
            return;
        }

        try
        {
            string json = File.ReadAllText(FileName);

            List<AssetData> loadedData =
                JsonSerializer.Deserialize<List<AssetData>>(json)
                ?? new List<AssetData>();

            Assets.Clear();

            foreach (AssetData data in loadedData)
            {
                Asset asset = data.AssetType switch
                {
                    AssetType.Smartphone => new Smartphone(
                        data.Price,
                        data.PurchaseDate,
                        data.Brand,
                        data.Model,
                        data.Office)
                    {
                        AssetId = data.AssetId
                    },

                    AssetType.Computer => new Computer(
                        data.Price,
                        data.PurchaseDate,
                        data.Brand,
                        data.Model,
                        data.Office)
                    {
                        AssetId = data.AssetId
                    },

                    _ => throw new InvalidOperationException(
                        $"Unknown asset type: {data.AssetType}"
                    )
                };

                Assets.Add(asset);
            }

            Console.WriteLine();
            Console.WriteLine($"{Assets.Count} assets loaded.");
        }
        catch (Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("The asset file contains invalid JSON.");
            Console.WriteLine(exception.Message);
            Console.ResetColor();
        }

        Console.ReadKey();
    }


    internal void DeleteAsset()
    {
        Header("REMOVE ASSET");

        PrintAssetHeader();
        Console.WriteLine();

        foreach (Asset asset in Assets)
        {
            PrintAsset(asset);
        }

        Console.WriteLine();
        Console.Write("Enter the ID of the asset to remove: ");

        if (!int.TryParse(Console.ReadLine(), out int assetId))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Asset does not exists.");
            Console.ResetColor();
            Console.ReadKey();
        }
        else
        {
            Asset foundAsset = Assets.Single(asset => asset.AssetId == assetId);
            {
                Assets.Remove(foundAsset);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(
                    $"Asset {foundAsset.AssetId} was removed."
                );
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to return to the menu.");
            Console.ReadKey();
        }
    }
}