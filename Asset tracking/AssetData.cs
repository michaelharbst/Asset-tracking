using Asset_tracking;

/// <summary>
/// Class only purpose is to have a non abstract class to deserialize json file into
/// </summary>
public class AssetData
{
    public int AssetId { get; set; }
    public AssetType AssetType { get; set; }
    public Price Price { get; set; }
    public string Brand { get; set; } = "";
    public string Model { get; set; } = "";
    public DateTime PurchaseDate { get; set; }
    public Office Office { get; set; }
}
