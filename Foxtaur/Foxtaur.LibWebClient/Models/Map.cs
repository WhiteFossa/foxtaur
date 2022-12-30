namespace Foxtaur.LibWebClient.Models;

/// <summary>
/// Map
/// </summary>
public class Map
{
    /// <summary>
    /// Map Id
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Bounds - north latitude
    /// </summary>
    public double NorthLat { get; }

    /// <summary>
    /// Bounds - south latitude
    /// </summary>
    public double SouthLat { get; }

    /// <summary>
    /// Bounds - east longitude
    /// </summary>
    public double EastLon { get; }

    /// <summary>
    /// Bounds - west longitude
    /// </summary>
    public double WestLon { get; }

    /// <summary>
    /// Full URL
    /// </summary>
    public string Url { get; }
    
    public Map(Guid id,
        string name,
        double northLat,
        double southLat,
        double eastLon,
        double westLon,
        string url
    )
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException(nameof(url));
        }

        Id = id;
        Name = name;
        NorthLat = northLat;
        SouthLat = southLat;
        EastLon = eastLon;
        WestLon = westLon;
        Url = url;
    }
}