using System.Text;
using Asset_tracking;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;



AssetManager Assetmanager = new AssetManager();

Console.OutputEncoding = Encoding.UTF8;
Initialization();
StartMenu();
static void Initialization()
{
    try
    {
        using MyDbContext db = new MyDbContext();

        if (!db.Assets.Any())
        {
            Console.Write(
                "The database is empty. Load default assets (Y/N)? "
            );

            if (Console.ReadLine()?.Trim().ToLower() == "y")
            {
                AssetManager.CreateDefaultData();
            }
        }
    }
    catch (SqlException)
    {
        Console.WriteLine("The database could not be initialized.");

        return;
    }
    catch (Exception exception)
    {
        Console.WriteLine("The application could not start correctly.");
        Console.WriteLine(exception.Message);
        return;
    }
}



void StartMenu()
{
    while (true)
    {
        Console.Clear();
        Assetmanager.ViewAsset(false);
        Console.WriteLine("Start menu");
        Console.WriteLine();
        Console.WriteLine("1. View Assets with filtering options");
        Console.WriteLine("2. Add Assets");
        Console.WriteLine("3. remove Asset");
        Console.WriteLine("4. Modify Asset");
        Console.WriteLine("5. Search Asset");
        Console.WriteLine("6. Reports");
        Console.WriteLine("7. Export Assets");
        Console.WriteLine("8. Exit");
        Console.WriteLine();
        Console.Write("press 1-8: ");

        int.TryParse(Console.ReadKey(intercept: true).KeyChar.ToString(), out int menuChoice);


        switch (menuChoice)
        {
            case 1:
                Assetmanager.ViewAsset(true);
                break;
            case 2:
                Assetmanager.AddAsset();
                break;
            case 3:
                Assetmanager.DeleteAsset();
                break;
            case 4:
                Assetmanager.UpdateAsset();
                break;
            case 5:
                Assetmanager.SearchAsset();
                break;
            case 6:
                Assetmanager.Reports();
                break;
            case 7:
                Assetmanager.ExportAssets();
                break;
            case 8:
                ExitProgram();
                break;
            default:
                StartMenu();
                break;
        }
    }
}

void ExitProgram()
{
    Console.WriteLine("Are you sure you want to leave this wonderful app? if so press Y");
    string yesNo = string.Empty;
    bool inputOk = false;
    while (!inputOk)
    {
        yesNo = Console.ReadKey(intercept: true).KeyChar.ToString().ToLower();
        switch (yesNo)
        {
            case "y":
                inputOk = true;
                Environment.Exit(0);
                break;
            case "n":
                inputOk = true;
                break;
            default:
                break;
        }
    }
}










