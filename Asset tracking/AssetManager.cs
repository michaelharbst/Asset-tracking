using System.Globalization;
using System.Text;
using System.Text.Json;
using Asset_tracking;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

internal class AssetManager
{
    internal void AddAsset()
    {
        Header("ADD ASSET");

        Console.WriteLine("Select asset type:");
        Console.WriteLine("1. Smartphone");
        Console.WriteLine("2. Computer");

        int assetType = ReadNumberInRange("Select 1-2: ", 1, 2);

        string brand = ReadRequiredText("Enter brand: ");
        string model = ReadRequiredText("Enter model: ");
        string serialNumber = ReadRequiredText("Enter serial number: ");

        decimal priceUSD = ReadPositiveDecimal("Enter price in USD: ");
        DateTime purchaseDate = ReadPurchaseDate();

        DateTime warrantyExpirationDate = ReadWarrantyExpirationDate(purchaseDate);

        Console.Write("Enter employee/username (optional): ");

        string employeeInput = Console.ReadLine()?.Trim() ?? "";

        string? employeeUsername = string.IsNullOrWhiteSpace(employeeInput)
            ? null
            : employeeInput;

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
            asset = new Smartphone(price, purchaseDate, brand, model, office, serialNumber, employeeUsername, warrantyExpirationDate);
        }
        else
        {
            asset = new Computer(price, purchaseDate, brand, model, office, serialNumber, employeeUsername, warrantyExpirationDate);
        }

        try
        {
            using MyDbContext db = new MyDbContext();

            string validationErrors = ValidateAsset(asset);

            if (string.IsNullOrEmpty(validationErrors))
            {
                db.Assets.Add(asset);
                db.SaveChanges();

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
        catch (DbUpdateException)
        {
            ShowError(
                "The asset could not be saved to the database."
            );
        }
        catch (SqlException)
        {
            ShowError(
                "The application could not communicate " +
                "with the database."
            );
        }
        catch (Exception)
        {
            ShowError(
                "An unexpected error occurred while adding the asset."
            );
        }
    }
    internal void SearchAsset()
    {
        Header("SEARCH ASSETS");

        Console.Write("Enter asset ID, brand, model, office or purchase year: ");

        string searchText = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(searchText))
        {
            Console.WriteLine("No search text was entered.");
            Console.ReadKey();
            return;
        }

        bool searchIsNumber = int.TryParse(searchText, out int number);

        bool searchIsYear = searchIsNumber &&
            number >= 2000 &&
            number <= DateTime.Today.Year;

        bool searchIsOffice = Enum.TryParse(searchText, ignoreCase: true, out Office office);

        try
        {
            using MyDbContext db = new MyDbContext();

            List<Asset> foundAssets = db.Assets
                .Where(asset =>
                    (searchIsNumber && asset.AssetId == number) ||
                    asset.Brand.Contains(searchText) ||
                    asset.Model.Contains(searchText) ||
                    (searchIsOffice && asset.Office == office) ||
                    (searchIsYear && asset.PurchaseDate.Year == number))
                .OrderBy(asset => asset.AssetType)
                .ThenBy(asset => asset.PurchaseDate)
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
        catch (DbUpdateException)
        {
            ShowError(
                "The asset could not be saved to the database."
            );
        }
        catch (SqlException)
        {
            ShowError(
                "The application could not communicate " +
                "with the database."
            );
        }
        catch (Exception)
        {
            ShowError(
                "An unexpected error occurred while adding the asset."
            );
        }
    }


    internal void ViewAsset(bool filterOption)
    {
        try
        {
            using MyDbContext db = new MyDbContext();
            IQueryable<Asset> assetsQuery = db.Assets;

            do
            {
                List<Asset> assets = assetsQuery
                      .OrderBy(asset => asset.AssetType)
                      .ThenBy(asset => asset.PurchaseDate)
                      .ToList();

                Header("ASSET LIST");

                Console.WriteLine(
                    "Asset Id".PadRight(10) +
                    "Office".PadRight(12) +
                    "Asset".PadRight(15) +
                    "Brand".PadRight(15) +
                    "Model".PadRight(20) +
                    "Serial number".PadRight(20) +
                    "Employee".PadRight(20) +
                    "Price USD".PadRight(12) +
                    "Price local".PadRight(15) +
                    "Purchase date".PadRight(15) +
                    "Warranty".PadRight(15) +
                    "Age (days)".PadRight(12)
                );

                Console.WriteLine(new string('-', 201));

                if (assets.Count == 0)
                {
                    Console.WriteLine(
                        "No assets matched the selected filter."
                    );
                }

                foreach (Asset asset in assets)
                {
                    int ageInDays =
                        (DateTime.Today - asset.PurchaseDate.Date).Days;

                    DateTime lifetimeExpirationDate =
                        asset.PurchaseDate.AddYears(3);

                    DateTime threeMonthWarningDate =
                        lifetimeExpirationDate.AddMonths(-3);

                    DateTime sixMonthWarningDate =
                        lifetimeExpirationDate.AddMonths(-6);


                    if (DateTime.Today >= threeMonthWarningDate)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else if (DateTime.Today >= sixMonthWarningDate)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }

                    string employee = string.IsNullOrWhiteSpace(
                        asset.EmployeeUserName)
                        ? ""
                        : asset.EmployeeUserName;

                    Console.WriteLine(
                        asset.AssetId.ToString().PadRight(10) +
                        asset.Office.ToString().PadRight(12) +
                        asset.GetType().Name.PadRight(15) +
                        asset.Brand.PadRight(15) +
                        asset.Model.PadRight(20) +
                        asset.SerialNumber.PadRight(20) +
                        employee.PadRight(20) +
                        FormatCurrency(asset.Price.PriceUSD, Currency.USD).PadRight(12) +
                        FormatCurrency(asset.Price.PriceLocal, asset.Price.Currency).PadRight(15) +
                        asset.PurchaseDate
                            .ToString("yyyy-MM-dd").PadRight(15) +
                        asset.WarrantyExpirationDate
                            .ToString("yyyy-MM-dd").PadRight(15) +
                        ageInDays.ToString().PadRight(12)                         
                    );

                    Console.ResetColor();
                    assetsQuery = db.Assets;
                }

                // If the filter option is enabled, display the filter menu and allow the user to select a filter option. Otherwise, exit the loop.
                // the filter assetsQuery is used at the top of the loop and set at the bottom, so it uses the value from the previous iteration to querry the DB.
                if (filterOption)
                {
                    Console.WriteLine("1. Show all assets");
                    Console.WriteLine("2. Show expired assets");
                    Console.WriteLine("3. Show computers");
                    Console.WriteLine("4. Show mobile devices");
                    Console.WriteLine("5. Show assets by office");
                    Console.WriteLine("0. Return to menu");

                    int choice = ReadNumberInRange("Select 0-5: ", 0, 5);

                    switch (choice)
                    {
                        case 0:
                            return;
                        case 1:
                            // No filter: All assets are shown, so no additional filtering is needed.
                            break;

                        case 2:
                            {
                                DateTime expirationLimit = DateTime.Today.AddYears(-3);

                                assetsQuery = assetsQuery.Where(asset => asset.PurchaseDate <= expirationLimit
                                );
                                break;
                            }

                        case 3:
                            assetsQuery = assetsQuery.Where(asset => asset.AssetType == AssetType.Computer);
                            break;

                        case 4:
                            assetsQuery = assetsQuery.Where(asset => asset.AssetType == AssetType.Smartphone);
                            break;

                        case 5:
                            {
                                Console.WriteLine();
                                Console.WriteLine("Select office:");
                                Console.WriteLine("1. Sweden");
                                Console.WriteLine("2. USA");
                                Console.WriteLine("3. Turkey");

                                int officeChoice = ReadNumberInRange(
                                    "Select 1-3: ",
                                    1,
                                    3
                                );

                                Office selectedOffice = officeChoice switch
                                {
                                    1 => Office.Sweden,
                                    2 => Office.USA,
                                    3 => Office.Turkey,
                                    _ => throw new ArgumentOutOfRangeException()
                                };

                                assetsQuery = assetsQuery.Where(asset =>
                                    asset.Office == selectedOffice
                                );

                                break;
                            }
                    }
                }
            }
            while (filterOption);
        }
        catch (SqlException)
        {
            ShowError("The application could not read data from the database.");
        }
        catch (Exception)
        {
            ShowError("An unexpected error occurred.");
        }
    }

    internal void Reports()
    {
        int choice;
        do
        {
            Header("REPORT OPTIONS");
            Console.WriteLine("1. Report for Value Per Office");
            Console.WriteLine("2. Report for Asset Count Per Office");
            Console.WriteLine("3. Report for Assets Close to Expiration");
            Console.WriteLine("4. Report for Most Expensive Assets");
            Console.WriteLine("0. Return to menu");

            choice = ReadNumberInRange("Select 0-4: ", 0, 4);

            switch (choice)
            {
                case 0:
                    return;
                case 1:
                    ShowTotalValuePerOffice();
                    break;
                case 2:
                    ShowAssetCountPerOffice();
                    break;
                case 3:
                    ShowAssetsCloseToExpiration();
                    break;
                case 4:
                    ShowMostExpensiveAssets();
                    break;
            }
        }
        while (choice != 0);
    }



    private void ShowTotalValuePerOffice()
    {
        Header("TOTAL ASSET VALUE PER OFFICE");

        try
        {
            using MyDbContext db = new MyDbContext();

            List<Asset> assets = db.Assets
                .OrderBy(asset => asset.Office)
                .ThenBy(asset => asset.AssetType)
                .ThenBy(asset => asset.PurchaseDate)
                .ToList();

            if (assets.Count == 0)
            {
                Console.WriteLine("There are no assets to report.");
                Console.WriteLine();
                Console.WriteLine(
                    "Press any key to return to the reports menu."
                );
                Console.ReadKey();
                return;
            }

            var assetsByOffice = assets
                .GroupBy(asset => asset.Office)
                .OrderBy(group => group.Key);

            foreach (var officeGroup in assetsByOffice)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine();
                Console.WriteLine($"Office: {officeGroup.Key}");
                Console.ResetColor();

                Console.WriteLine();

                Console.WriteLine(
                    "ID".PadRight(8) +
                    "Asset".PadRight(15) +
                    "Brand".PadRight(18) +
                    "Employee".PadRight(20) +
                    "Price USD".PadRight(15) +
                    "Price local"
                );

                Console.WriteLine(new string('-', 90));

                foreach (Asset asset in officeGroup)
                {
                    string employee = string.IsNullOrWhiteSpace(
                        asset.EmployeeUserName)
                        ? "Unassigned"
                        : asset.EmployeeUserName;

                    string localPrice = $"{asset.Price.PriceLocal:0.00} " + $"{asset.Price.Currency}";
                    localPrice = FormatCurrency(asset.Price.PriceLocal, asset.Price.Currency);

                    Console.Write(
                        asset.AssetId.ToString().PadRight(8) +
                        asset.GetType().Name.PadRight(15) +
                        asset.Brand.PadRight(18) +
                        employee.PadRight(20)
                    );

                    Console.ForegroundColor = ConsoleColor.Green;

                    string priceUSD = FormatCurrency(asset.Price.PriceUSD, Currency.USD); // $           
                    Console.Write(priceUSD.PadRight(15) + localPrice);

                    Console.ResetColor();
                    Console.WriteLine();
                }

                decimal totalUSD = officeGroup.Sum(asset =>
                    asset.Price.PriceUSD
                );

                decimal totalLocal = officeGroup.Sum(asset =>
                    asset.Price.PriceLocal
                );

                Currency localCurrency = officeGroup.First().Price.Currency;

                Console.WriteLine(new string('-', 90));

                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine("TOTAL".PadRight(61) + FormatCurrency(totalUSD, Currency.USD).PadRight(15) + FormatCurrency(totalLocal, localCurrency));

                Console.ResetColor();

                Console.WriteLine(new string('=', 90));
            }

            Console.WriteLine();
            Console.WriteLine(
                "Press any key to return to the reports menu."
            );
            Console.ReadKey();
        }
        catch (SqlException)
        {
            ShowError(
                "The application could not read data from the database."
            );
        }
        catch (Exception)
        {
            ShowError(
                "An unexpected error occurred."
            );
        }
    }

    private void ShowAssetCountPerOffice()
    {
        Header("ASSET COUNT PER OFFICE");

        try
        {
            using MyDbContext db = new MyDbContext();

            List<Asset> assets = db.Assets
                .OrderBy(asset => asset.Office)
                .ThenBy(asset => asset.AssetType)
                .ThenBy(asset => asset.PurchaseDate)
                .ToList();

            if (assets.Count == 0)
            {
                Console.WriteLine("There are no assets to report.");
                Console.WriteLine();
                Console.WriteLine(
                    "Press any key to return to the reports menu."
                );
                Console.ReadKey();
                return;
            }

            var assetsByOffice = assets
                .GroupBy(asset => asset.Office)
                .OrderBy(group => group.Key);

            foreach (var officeGroup in assetsByOffice)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine();
                Console.WriteLine($"Office: {officeGroup.Key}");
                Console.ResetColor();

                Console.WriteLine();

                PrintAssetHeader();

                foreach (Asset asset in officeGroup)
                {
                    PrintAsset(asset);
                }

                int assetCount = officeGroup.Count();

                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine(
                    $"Number of assets in {officeGroup.Key}: " +
                    assetCount
                );

                Console.ResetColor();

                Console.WriteLine(new string('=', 174));
            }

            Console.WriteLine();
            Console.WriteLine(
                "Press any key to return to the reports menu."
            );
            Console.ReadKey();
        }
        catch (SqlException)
        {
            ShowError(
                "The application could not read data from the database."
            );
        }
        catch (Exception)
        {
            ShowError(
                "An unexpected error occurred."
            );
        }
    }

    private void ShowAssetsCloseToExpiration()
    {
        Header("ASSETS CLOSE TO EXPIRATION");
        try
        {
            using MyDbContext db = new MyDbContext();

            DateTime today = DateTime.Today;
            DateTime sixMonthsFromToday = today.AddMonths(6);

            var expiringAssets = db.Assets
                .ToList()
                .Select(asset => new
                {
                    Asset = asset,
                    ExpirationDate =
                        asset.PurchaseDate.AddYears(3)
                })
                .Where(item =>
                    item.ExpirationDate >= today &&
                    item.ExpirationDate <= sixMonthsFromToday)
                .OrderBy(item => item.ExpirationDate)
                .ToList();

            if (expiringAssets.Count == 0)
            {
                Console.WriteLine(
                    "No assets expire within the next six months."
                );
            }
            else
            {
                Console.WriteLine(
                    "Asset Id".PadRight(10) +
                    "Asset".PadRight(15) +
                    "Brand".PadRight(15) +
                    "Model".PadRight(20) +
                    "Office".PadRight(12) +
                    "Expiration".PadRight(15) +
                    "Days left"
                );

                Console.WriteLine(new string('-', 97));

                foreach (var item in expiringAssets)
                {
                    int daysRemaining =
                        (item.ExpirationDate - today).Days;

                    if (daysRemaining < 90)
                    {
                        Console.ForegroundColor =
                            ConsoleColor.Red;
                    }
                    else
                    {
                        Console.ForegroundColor =
                            ConsoleColor.Yellow;
                    }

                    Console.WriteLine(
                        item.Asset.AssetId.ToString().PadRight(10) +
                        item.Asset.GetType().Name.PadRight(15) +
                        item.Asset.Brand.PadRight(15) +
                        item.Asset.Model.PadRight(20) +
                        item.Asset.Office.ToString().PadRight(12) +
                        item.ExpirationDate
                            .ToString("yyyy-MM-dd").PadRight(15) +
                        daysRemaining
                    );

                    Console.ResetColor();
                }
            }

            Console.WriteLine();
            Console.WriteLine(
                "Press any key to return to the reports menu."
            );
            Console.ReadKey();
        }
        catch (SqlException)
        {
            ShowError(
                "The application could not read data from the database."
            );
        }
        catch (Exception)
        {
            ShowError(
                "An unexpected error occurred."
            );
        }
    }
    private void ShowMostExpensiveAssets()
    {
        Header("MOST EXPENSIVE ASSETS");
        try
        {
            using MyDbContext db = new MyDbContext();

            List<Asset> mostExpensiveAssets = db.Assets
                .OrderByDescending(asset => asset.Price.PriceUSD)
                .Take(5)
                .ToList();

            if (mostExpensiveAssets.Count == 0)
            {
                Console.WriteLine("There are no assets to report.");
            }
            else
            {
                Console.WriteLine(
                    $"Showing the {mostExpensiveAssets.Count} " +
                    "most expensive assets:"
                );

                Console.WriteLine();

                PrintAssetHeader();

                foreach (Asset asset in mostExpensiveAssets)
                {
                    PrintAsset(asset);
                }
            }

            Console.WriteLine();
            Console.WriteLine(
                "Press any key to return to the reports menu."
            );
            Console.ReadKey();
        }
        catch (SqlException)
        {
            ShowError(
                "The application could not read data from the database."
            );
        }
        catch (Exception)
        {
            ShowError(
                "An unexpected error occurred."
            );
        }
    }


    internal void ExportAssets()
    {
        Header("EXPORT ASSETS");

        Console.WriteLine("Select export format:");
        Console.WriteLine("1. TXT");
        Console.WriteLine("2. CSV");
        Console.WriteLine("3. JSON");
        Console.WriteLine("0. Return to main menu");

        int choice = ReadNumberInRange("Select 0-3: ", 0, 3);

        if (choice == 0)
        {
            return;
        }

        try
        {
            switch (choice)
            {
                case 1:
                    ExportAssetsToTxt();
                    break;

                case 2:
                    ExportAssetsToCsv();
                    break;

                case 3:
                    ExportAssetsToJson();
                    break;
            }
        }
        catch (UnauthorizedAccessException)
        {
            ShowError(
                "The application does not have permission " +
                "to write the export file."
            );
        }
        catch (IOException exception)
        {
            ShowError(
                $"The export file could not be written: " +
                $"{exception.Message}"
            );
        }
        catch (JsonException exception)
        {
            ShowError(
                $"The JSON export failed: {exception.Message}"
            );
        }
        catch (Exception exception)
        {
            ShowError(
                $"The export failed: {exception.Message}"
            );
        }

        Console.WriteLine();
        Console.WriteLine(
            "Press any key to return to the main menu."
        );
        Console.ReadKey();
    }

    private void ExportAssetsToTxt()
    {
        using MyDbContext db = new MyDbContext();

        List<Asset> assets = db.Assets
            .OrderBy(asset => asset.AssetType)
            .ThenBy(asset => asset.PurchaseDate)
            .ToList();

        if (assets.Count == 0)
        {
            Console.WriteLine();
            Console.WriteLine("There are no assets to export.");
            return;
        }

        string exportDirectory = GetExportDirectory();

        string fileName =
            $"assets_{DateTime.Now:yyyyMMdd_HHmmssfff}.txt";

        string filePath = Path.Combine(
            exportDirectory,
            fileName
        );

        StringBuilder text = new StringBuilder();

        text.AppendLine("ASSET REPORT");
        text.AppendLine(
            $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
        );
        text.AppendLine(new string('=', 70));
        text.AppendLine();

        foreach (Asset asset in assets)
        {
            text.AppendLine($"Asset ID: {asset.AssetId}");
            text.AppendLine(
                $"Asset type: {asset.GetType().Name}"
            );
            text.AppendLine($"Office: {asset.Office}");
            text.AppendLine($"Brand: {asset.Brand}");
            text.AppendLine($"Model: {asset.Model}");
            text.AppendLine(
                $"Serial number: {asset.SerialNumber}"
            );
            text.AppendLine(
                $"Employee: " +
                $"{asset.EmployeeUserName ?? "Unassigned"}"
            );
            text.AppendLine(
                $"Price USD: " +
                $"{FormatCurrency(asset.Price.PriceUSD, Currency.USD)}"
            );
            text.AppendLine(
                $"Price local: " +
                $"{FormatCurrency(
                    asset.Price.PriceLocal,
                    asset.Price.Currency)}"
            );
            text.AppendLine(
                $"Purchase date: " +
                $"{asset.PurchaseDate:yyyy-MM-dd}"
            );
            text.AppendLine(
                $"Warranty expiration: " +
                $"{asset.WarrantyExpirationDate:yyyy-MM-dd}"
            );

            text.AppendLine(new string('-', 70));
        }

        File.WriteAllText(
            filePath,
            text.ToString(),
            Encoding.UTF8
        );

        ShowExportSuccess(filePath, assets.Count);
    }

    private void ExportAssetsToCsv()
    {
        using MyDbContext db = new MyDbContext();

        List<Asset> assets = db.Assets
            .OrderBy(asset => asset.AssetType)
            .ThenBy(asset => asset.PurchaseDate)
            .ToList();

        if (assets.Count == 0)
        {
            Console.WriteLine();
            Console.WriteLine("There are no assets to export.");
            return;
        }

        string exportDirectory = GetExportDirectory();

        string fileName =
            $"assets_{DateTime.Now:yyyyMMdd_HHmmssfff}.csv";

        string filePath = Path.Combine(
            exportDirectory,
            fileName
        );

        StringBuilder csv = new StringBuilder();

        csv.AppendLine(
            "AssetId,AssetType,Office,Brand,Model," +
            "SerialNumber,EmployeeUsername,PriceUSD," +
            "Currency,PriceLocal,PurchaseDate," +
            "WarrantyExpirationDate"
        );

        foreach (Asset asset in assets)
        {
            string[] values =
            {
            asset.AssetId.ToString(
                CultureInfo.InvariantCulture),

            asset.GetType().Name,
            asset.Office.ToString(),
            asset.Brand,
            asset.Model,
            asset.SerialNumber,
            asset.EmployeeUserName ?? "",

            asset.Price.PriceUSD.ToString(
                CultureInfo.InvariantCulture),

            asset.Price.Currency.ToString(),

            asset.Price.PriceLocal.ToString(
                CultureInfo.InvariantCulture),

            asset.PurchaseDate.ToString("yyyy-MM-dd"),

            asset.WarrantyExpirationDate.ToString(
                "yyyy-MM-dd"),
        };

            csv.AppendLine(
                string.Join(",", values.Select(EscapeCsvValue))
            );
        }

        File.WriteAllText(
            filePath,
            csv.ToString(),
            Encoding.UTF8
        );

        ShowExportSuccess(filePath, assets.Count);
    }


    private void ExportAssetsToJson()
    {
        using MyDbContext db = new MyDbContext();

        List<Asset> assets = db.Assets
            .OrderBy(asset => asset.AssetType)
            .ThenBy(asset => asset.PurchaseDate)
            .ToList();

        if (assets.Count == 0)
        {
            Console.WriteLine();
            Console.WriteLine("There are no assets to export.");
            return;
        }

        var exportData = assets.Select(asset => new
        {
            asset.AssetId,

            AssetType = asset.GetType().Name,

            Office = asset.Office.ToString(),

            asset.Brand,
            asset.Model,
            asset.SerialNumber,
            asset.EmployeeUserName,

            PriceUSD = asset.Price.PriceUSD,

            Currency = asset.Price.Currency.ToString(),

            PriceLocal = asset.Price.PriceLocal,

            PurchaseDate =
                asset.PurchaseDate.ToString("yyyy-MM-dd"),

            WarrantyExpirationDate =
                asset.WarrantyExpirationDate.ToString(
                    "yyyy-MM-dd")
        })
        .ToList();

        JsonSerializerOptions options =
            new JsonSerializerOptions
            {
                WriteIndented = true
            };

        string json = JsonSerializer.Serialize(
            exportData,
            options
        );

        string exportDirectory = GetExportDirectory();

        string fileName =
            $"assets_{DateTime.Now:yyyyMMdd_HHmmssfff}.json";

        string filePath = Path.Combine(
            exportDirectory,
            fileName
        );

        File.WriteAllText(
            filePath,
            json,
            Encoding.UTF8
        );

        ShowExportSuccess(filePath, assets.Count);
    }

    // <summary>
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
    internal static void CreateDefaultData()
    {
        using MyDbContext db = new MyDbContext();
        List<Asset> Assets = new();

        Price price = new Price(200, Currency.USD);
        Assets.Add(new Smartphone(price, DateTime.Now.AddMonths(-36 + 4), "Motorola", "X3", Office.USA, "SN001", "John Doe", DateTime.Now.AddMonths(12)));
        Assets.Add(new Smartphone(new Price(400, Currency.USD), DateTime.Now.AddMonths(-36 + 5), "Motorola", "X3", Office.USA, "SN002", "Jane Smith", DateTime.Now.AddMonths(12)));
        Assets.Add(new Smartphone(new Price(400, Currency.USD), DateTime.Now.AddMonths(-36 + 10), "Motorola", "X2", Office.USA, "SN003", "Bob Johnson", DateTime.Now.AddMonths(12)));
        Assets.Add(new Smartphone(new Price(4500, Currency.TRY), DateTime.Now.AddMonths(-36 + 6), "Samsung", "Galaxy 10", Office.Turkey, "SN004", "Ali Yilmaz", DateTime.Now.AddMonths(12)));
        Assets.Add(new Smartphone(new Price(4500, Currency.SEK), DateTime.Now.AddMonths(-36 + 7), "Samsung", "Galaxy 10", Office.Sweden, "SN005", null, DateTime.Now.AddMonths(12)));
        Assets.Add(new Smartphone(new Price(3000, Currency.SEK), DateTime.Now.AddMonths(-36 + 4), "Sony", "XPeria 7", Office.Sweden, "SN006", "Anna Karlsson", DateTime.Now.AddMonths(12)));
        Assets.Add(new Smartphone(new Price(3000, Currency.SEK), DateTime.Now.AddMonths(-36 + 5), "Sony", "XPeria 7", Office.Sweden, "SN007", "Anna Karlsson", DateTime.Now.AddMonths(12)));
        Assets.Add(new Smartphone(new Price(220, Currency.TRY), DateTime.Now.AddMonths(-36 + 12), "Siemens", "Brick", Office.Turkey, "SN008", "Ali Yilmaz", DateTime.Now.AddMonths(12)));
        Assets.Add(new Computer(new Price(100, Currency.USD), DateTime.Now.AddMonths(-38), "Dell", "Desktop 900", Office.USA, "SN009", "John Doe", DateTime.Now.AddMonths(12)));
        Assets.Add(new Computer(new Price(100, Currency.USD), DateTime.Now.AddMonths(-37), "Dell", "Desktop 900", Office.USA, "SN010", "Jane Smith", DateTime.Now.AddMonths(12)));
        Assets.Add(new Computer(new Price(300, Currency.USD), DateTime.Now.AddMonths(-36 + 1), "Lenovo", "X100", Office.USA, "SN011", "Bob Johnson", DateTime.Now.AddMonths(12)));
        Assets.Add(new Computer(new Price(300, Currency.USD), DateTime.Now.AddMonths(-36 + 4), "Lenovo", "X200", Office.USA, "SN012", null, DateTime.Now.AddMonths(12)));
        Assets.Add(new Computer(new Price(500, Currency.USD), DateTime.Now.AddMonths(-36 + 9), "Lenovo", "X300", Office.USA, "SN013", "Charlie Wilson", DateTime.Now.AddMonths(12)));
        Assets.Add(new Computer(new Price(1500, Currency.SEK), DateTime.Now.AddMonths(-36 + 7), "Dell", "Optiplex 100", Office.Sweden, "SN014", "Elsa Lindqvist", DateTime.Now.AddMonths(12)));
        Assets.Add(new Computer(new Price(1400, Currency.SEK), DateTime.Now.AddMonths(-36 + 8), "Dell", "Optiplex 200", Office.Sweden, "SN015", "Elsa Lindqvist", DateTime.Now.AddMonths(12)));
        Assets.Add(new Computer(new Price(1300, Currency.SEK), DateTime.Now.AddMonths(-36 + 9), "Dell", "Optiplex 300", Office.Sweden, "SN₀₁₆", "Elsa Lindqvist", DateTime.Now.AddMonths(12)));
        Assets.Add(new Computer(new Price(1600, Currency.TRY), DateTime.Now.AddMonths(-36 + 14), "Asus", "ROG 600", Office.Turkey, "SN017", "Ali Yilmaz", DateTime.Now.AddMonths(12)));
        Assets.Add(new Computer(new Price(1200, Currency.TRY), DateTime.Now.AddMonths(-36 + 4), "Asus", "ROG 500", Office.Turkey, "SN018", "Ali Yilmaz", DateTime.Now.AddMonths(12)));
        Assets.Add(new Computer(new Price(1200, Currency.TRY), DateTime.Now.AddMonths(-36 + 3), "Asus", "ROG 500", Office.Turkey, "SN019", "Ali Yilmaz", DateTime.Now.AddMonths(12)));
        Assets.Add(new Computer(new Price(1300, Currency.TRY), DateTime.Now.AddMonths(-36 + 2), "Asus", "ROG 500", Office.Turkey, "SN020", "Ali Yilmaz", DateTime.Now.AddMonths(12)));

        db.Assets.AddRange(Assets);
        int rowsAffected = db.SaveChanges();
        Console.WriteLine($"{rowsAffected} default assets were added to the database.");
        Console.ReadKey();

    }


    //following methods are usefull help methods

    private static Currency GetCurrencyForOffice(Office office)
    {
        return office switch
        {
            Office.Sweden => Currency.SEK,
            Office.USA => Currency.USD,
            Office.Turkey => Currency.TRY,
            _ => throw new ArgumentOutOfRangeException(
                nameof(office)
            )
        };
    }

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
            "Asset Id".PadRight(10) +
            "Office".PadRight(12) +
            "Asset".PadRight(15) +
            "Brand".PadRight(15) +
            "Model".PadRight(20) +
            "Serial number".PadRight(20) +
            "Employee".PadRight(20) +
            "Price USD".PadRight(15) +
            "Price local".PadRight(20) +
            "Purchase date".PadRight(15) +
            "Warranty"
        );
    }

    private static void PrintAsset(Asset asset)
    {
        string employee = string.IsNullOrWhiteSpace(
            asset.EmployeeUserName)
            ? ""
            : asset.EmployeeUserName;

        Console.WriteLine(
            asset.AssetId.ToString().PadRight(10) +
            asset.Office.ToString().PadRight(12) +
            asset.GetType().Name.PadRight(15) +
            asset.Brand.PadRight(15) +
            asset.Model.PadRight(20) +
            asset.SerialNumber.PadRight(20) +
            employee.PadRight(20) +
            FormatCurrency(asset.Price.PriceUSD, Currency.USD).PadRight(15) +
            FormatCurrency(asset.Price.PriceLocal, asset.Price.Currency).PadRight(20) +
            asset.PurchaseDate.ToString("yyyy-MM-dd").PadRight(15) +
            asset.WarrantyExpirationDate.ToString("yyyy-MM-dd")
        );
    }

    internal void DeleteAsset()
    {
        Header("REMOVE ASSET");

        PrintAssetHeader();
        Console.WriteLine();

        try
        {
            using MyDbContext db = new MyDbContext();
            foreach (Asset asset in db.Assets)
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
                Asset foundAsset = db.Assets.Single(asset => asset.AssetId == assetId);
                {
                    db.Assets.Remove(foundAsset);
                    db.SaveChanges();

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

        catch (DbUpdateConcurrencyException)
        {
            ShowError(
                "The asset was changed or removed by another operation. " +
                "Please reload the data and try again."
            );
        }
        catch (DbUpdateException exception)
        {
            ShowError(
                "The database could not save the changes."
            );
        }
        catch (SqlException exception)
        {
            ShowError(
                "The application could not communicate with the database."
            );
        }
        catch (Exception exception)
        {
            ShowError(
                "An unexpected error occurred. " +
                "The operation could not be completed."
            );
        }
    }

    internal void UpdateAsset()
    {
        Header("UPDATE ASSET");

        try
        {
            using MyDbContext db = new MyDbContext();

            if (!db.Assets.Any())
            {
                Console.WriteLine("There are no assets to update.");
                Console.WriteLine();
                Console.WriteLine("Press any key to return to the menu.");
                Console.ReadKey();
                return;
            }

            PrintAssetHeader();

            foreach (Asset asset in db.Assets.OrderBy(asset => asset.AssetType).ThenBy(asset => asset.PurchaseDate))
            {
                PrintAsset(asset);
            }

            Console.WriteLine();

            int assetId = ReadNumberInRange("Enter the ID of the asset to update: ", 1, db.Assets.Max(asset => asset.AssetId));
            Asset? foundAsset = db.Assets.Find(assetId);

            if (foundAsset == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(
                    $"No asset with ID {assetId} was found."
                );
                Console.ResetColor();

                Console.WriteLine();
                Console.WriteLine("Press any key to return to the menu.");
                Console.ReadKey();
                return;
            }

            while (true)
            {
                Header("UPDATE ASSET");

                Console.WriteLine("Current asset:");
                Console.WriteLine();

                PrintAssetHeader();
                PrintAsset(foundAsset);

                Console.WriteLine();
                Console.WriteLine("Select the field to update:");
                Console.WriteLine("1. Brand");
                Console.WriteLine("2. Model");
                Console.WriteLine("3. Price in USD");
                Console.WriteLine("4. Purchase date");
                Console.WriteLine("5. Office");
                Console.WriteLine("6. Serial number");
                Console.WriteLine("7. Employee/username");
                Console.WriteLine("8. Warranty expiration date");
                Console.WriteLine("9. Save changes");
                Console.WriteLine("0. Cancel");

                int updateChoice = ReadNumberInRange("Select 0-10: ", 0, 9);

                switch (updateChoice)
                {
                    case 10:
                        Console.WriteLine();
                        Console.WriteLine(
                            "The update was cancelled."
                        );
                        Console.WriteLine(
                            "Press any key to return to the menu."
                        );
                        Console.ReadKey();
                        return;

                    case 1:
                        foundAsset.Brand = ReadRequiredText(
                            "Enter the new brand: "
                        );
                        break;

                    case 2:
                        foundAsset.Model = ReadRequiredText(
                            "Enter the new model: "
                        );
                        break;

                    case 3:
                        {
                            decimal priceUSD = ReadPositiveDecimal(
                                "Enter the new price in USD: "
                            );

                            Currency currency = GetCurrencyForOffice(
                                foundAsset.Office
                            );

                            foundAsset.Price = new Price(
                                priceUSD,
                                currency
                            );

                            break;
                        }

                    case 4:
                        foundAsset.PurchaseDate = ReadPurchaseDate();
                        break;

                    case 5:
                        {
                            Console.WriteLine();
                            Console.WriteLine("Select office:");
                            Console.WriteLine("1. Sweden");
                            Console.WriteLine("2. USA");
                            Console.WriteLine("3. Turkey");

                            int officeChoice = ReadNumberInRange(
                                "Select 1-3: ", 1, 3);

                            Office newOffice = officeChoice switch
                            {
                                1 => Office.Sweden,
                                2 => Office.USA,
                                3 => Office.Turkey,
                                _ => throw new ArgumentOutOfRangeException()
                            };

                            Currency newCurrency =
                                GetCurrencyForOffice(newOffice);

                            decimal currentPriceUSD =
                                foundAsset.Price.PriceUSD;

                            foundAsset.Office = newOffice;

                            foundAsset.Price = new Price(
                                currentPriceUSD,
                                newCurrency
                            );

                            break;
                        }

                    case 6:
                        foundAsset.SerialNumber = ReadRequiredText(
                            "Enter the new serial number: "
                        );
                        break;

                    case 7:
                        {
                            Console.Write(
                                "Enter employee/username " +
                                "(leave empty to remove): "
                            );

                            string employeeInput =
                                Console.ReadLine()?.Trim() ?? "";

                            foundAsset.EmployeeUserName =
                                string.IsNullOrWhiteSpace(employeeInput)
                                    ? null
                                    : employeeInput;

                            break;
                        }

                    case 8:
                        foundAsset.WarrantyExpirationDate =
                            ReadWarrantyExpirationDate(
                                foundAsset.PurchaseDate
                            );
                        break;

                    case 9:
                        {
                            string validationErrors =
                                ValidateAsset(foundAsset);

                            if (!string.IsNullOrEmpty(validationErrors))
                            {
                                Console.WriteLine();
                                Console.ForegroundColor =
                                    ConsoleColor.Yellow;

                                Console.WriteLine(
                                    "The asset was not updated:"
                                );

                                Console.WriteLine(validationErrors);
                                Console.ResetColor();

                                Console.WriteLine(
                                    "Press any key to continue editing."
                                );
                                Console.ReadKey();

                                break;
                            }

                            if (!db.ChangeTracker.HasChanges())
                            {
                                Console.WriteLine();
                                Console.WriteLine(
                                    "No changes were made."
                                );

                                Console.WriteLine(
                                    "Press any key to return to the menu."
                                );
                                Console.ReadKey();

                                return;
                            }

                            db.SaveChanges();

                            Console.WriteLine();
                            Console.ForegroundColor =
                                ConsoleColor.Green;

                            Console.WriteLine(
                                $"{foundAsset.GetType().Name} " +
                                $"{foundAsset.Brand} " +
                                $"{foundAsset.Model} with ID " +
                                $"{foundAsset.AssetId} was updated."
                            );

                            Console.ResetColor();

                            Console.WriteLine();
                            Console.WriteLine(
                                "Press any key to return to the menu."
                            );
                            Console.ReadKey();

                            return;
                        }
                }
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            ShowError(
                "The asset was changed or removed by another operation. " +
                "Please reload the data and try again."
            );
        }
        catch (DbUpdateException exception)
        {
            ShowError(
                "The database could not save the changes."
            );
        }
        catch (SqlException exception)
        {
            ShowError(
                "The application could not communicate with the database."
            );
        }
        catch (Exception exception)
        {
            ShowError(
                "An unexpected error occurred. " +
                "The operation could not be completed."
            );
        }
    }

    private static DateTime ReadWarrantyExpirationDate(
    DateTime purchaseDate)
    {
        while (true)
        {
            Console.Write(
                "Enter warranty expiration date (yyyy-MM-dd): "
            );

            string input = Console.ReadLine()?.Trim() ?? "";

            bool inputOk = DateTime.TryParseExact(
                input,
                "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime warrantyExpirationDate
            );

            if (!inputOk)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(
                    "Enter a valid date in yyyy-MM-dd format."
                );
                Console.ResetColor();
                continue;
            }

            if (warrantyExpirationDate.Date <= purchaseDate.Date)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(
                    "The warranty expiration date must be after " +
                    "the purchase date."
                );
                Console.ResetColor();
                continue;
            }

            return warrantyExpirationDate;
        }
    }
    private static string FormatCurrency(decimal amount, Currency currency)
    {
        CultureInfo culture = currency switch
        {
            Currency.USD => new CultureInfo("en-US"),
            Currency.SEK => new CultureInfo("sv-SE"),
            Currency.TRY => new CultureInfo("tr-TR"),
            _ => CultureInfo.InvariantCulture
        };

        return amount.ToString("C2", culture);
    }

    private static string EscapeCsvValue(string value)
    {
        if (value.Contains(',') ||
            value.Contains('"') ||
            value.Contains('\n') ||
            value.Contains('\r'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

    private static string GetExportDirectory()
    {
        string exportDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Exports"
        );

        Directory.CreateDirectory(exportDirectory);

        return exportDirectory;
    }

    private static void ShowExportSuccess(
    string filePath,
    int assetCount)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;

        Console.WriteLine(
            $"{assetCount} asset(s) were exported successfully."
        );

        Console.ResetColor();

        Console.WriteLine();
        Console.WriteLine($"File: {filePath}");
    }

    private static void ShowError(string message)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.WriteLine("Press any key to return to the menu.");
        Console.ReadKey();
    }
}