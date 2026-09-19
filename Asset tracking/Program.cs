using System.Collections;
using System.Diagnostics;
using Asset_tracking;



AssetManager Assetmanager = new AssetManager();

Initialization();

void Initialization()
{
    Assetmanager.Load();
}

StartMenu();

void StartMenu()
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine("Start menu");
        Console.WriteLine();
        Console.WriteLine("1. Add Asset");
        Console.WriteLine("2. View Assets");
        Console.WriteLine("3. remove Asset");
        Console.WriteLine("4. Sort Assets");
        Console.WriteLine("5. Search Asset");
        Console.WriteLine("6. Exit");
        Console.WriteLine();
        Console.Write("press 1-6: ");

        int.TryParse(Console.ReadKey(intercept: true).KeyChar.ToString(), out int menuChoice);


        switch (menuChoice)
        {
            case 1:
                Assetmanager.AddAsset();
                break;
            case 2:
                Assetmanager.ViewAsset();
                break;
            case 3:
                Assetmanager.DeleteAsset();
                break;
            case 4:            
                Assetmanager.SortAsset();
                break;
            case 5:
                Assetmanager.SearchAsset();
                break;
            case 6:
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
                Console.WriteLine("Save inventory before exiting (y/n): ");
                if (Console.ReadKey(intercept: true).KeyChar.ToString().ToLower() == "y")
                {
                    Assetmanager.Save();
                }
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










