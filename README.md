# Asset Tracking

Asset Tracking is a C# console application for managing company assets across offices in different countries.

The application currently supports smartphones and computers, but the class structure can be extended with additional asset types.

Data is stored in a Microsoft SQL Server database using Entity Framework Core.

## Features

- Create, view, update and delete assets
- Store assets in a SQL Server database
- Search by:
  - Asset ID
  - Brand
  - Model
  - Office
  - Purchase year
- Filter assets by:
  - Expired assets
  - Computers
  - Mobile devices
  - Office
- Sort assets by asset type and purchase date
- Display prices in USD and local currency
- Use fixed exchange rates for USD, SEK and TRY
- Display the correct currency symbols
- Highlight assets based on their remaining three-year lifetime
- Assign an optional employee or username to an asset
- Track serial numbers and warranty expiration dates
- Export asset data to:
  - TXT
  - CSV
  - JSON

## Reports

The application includes the following reports:

- Total asset value per office
- Asset count per office
- Assets close to expiration
- Five most expensive assets

## Technologies

- C#
- .NET Console Application
- Entity Framework Core
- Microsoft SQL Server
- LINQ
- EF Core migrations

## Asset types

The application currently supports:

- Smartphone
- Computer

Both asset types inherit from the abstract `Asset` base class.

EF Core stores the different asset types in the same `Assets` table and uses the `AssetType` column as a discriminator.

## Database setup

The application uses Microsoft SQL Server and expects SQL Server Express by default.

The connection string is located in `MyDbContext.cs`:

```text
Server=localhost\SQLEXPRESS;
Database=asset_Tracking;
Trusted_Connection=True;
TrustServerCertificate=True;
```

The connection string uses Windows authentication and does not contain a username or password.

If your SQL Server uses another instance name, update the connection string in `MyDbContext.cs`.

Examples:

### SQL Server Express

```text
Server=localhost\SQLEXPRESS;
```

### SQL Server LocalDB

```text
Server=(localdb)\MSSQLLocalDB;
```

### Default SQL Server instance

```text
Server=localhost;
```

The full connection string must retain the database and security settings:

```text
Database=asset_Tracking;
Trusted_Connection=True;
TrustServerCertificate=True;
```

## Entity Framework migrations

The repository includes the Entity Framework Core migration files required to create the database schema.

The application offers to apply pending migrations during initialization.

Migrations can also be applied manually from the Visual Studio Package Manager Console:

```powershell
Update-Database
```

If the Entity Framework tools cannot find the context, ensure that the Asset Tracking project is selected as both the startup project and the default project in the Package Manager Console.

## Default data

The application includes default test data for offices in:

- Sweden
- USA
- Turkey

The default data includes smartphones and computers with different brands, prices, purchase dates, employees and warranty dates.

Default data should only be loaded into an empty database to avoid duplicates.

## Installation

1. Clone the repository:

```bash
git clone https://github.com/michaelharbst/Asset-tracking.git
```

2. Open the solution in Visual Studio.

3. Restore the NuGet packages.

4. Check the connection string in `MyDbContext.cs`.

5. Ensure that SQL Server is running.

6. Build and run the application.

The application can create or update the database schema through the included Entity Framework migrations.

## Exported files

Exported TXT, CSV and JSON files are written to the `Exports` directory beside the compiled application.

During development, this will normally be:

```text
bin/Debug/net10.0/Exports
```

The exact path is displayed after a successful export.

Exported files are reports and are not used as the application's primary data storage.

## Currency conversion

All asset prices are stored in USD.

Local prices are calculated using fixed exchange rates for:

- USD
- SEK
- TRY

The exchange rates are hardcoded for educational purposes and are not retrieved from a live currency API.

## Validation and error handling

The application validates user input, including:

- Required brand and model
- Positive prices
- Valid purchase dates
- Required serial numbers
- Warranty dates after the purchase date
- Valid menu selections

Database and file errors are handled and displayed as user-friendly messages where appropriate.

## Possible future improvements

Possible future improvements include:

- Additional asset types
- A graphical user interface
- Live currency exchange rates
- User authentication and roles
- Employee management
- Maintenance tracking
- Async database operations
- Logging
- Unit and integration tests
