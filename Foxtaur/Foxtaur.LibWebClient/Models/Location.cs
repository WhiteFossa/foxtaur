using Foxtaur.LibWebClient.Enums;

namespace Foxtaur.LibWebClient.Models;

/// <summary>
/// Location
/// </summary>
public class Location
{
    /// <summary>
    /// Location ID
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Location name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Location type
    /// </summary>
    public LocationType Type { get; }

    /// <summary>
    /// Latitude
    /// </summary>
    public double Lat { get;  }

    /// <summary>
    /// Longitude
    /// </summary>
    public double Lon { get; }

    /// <summary>
    /// Fox, installed in this location.
    /// Valid only if Type == Fox, otherwise must be null.
    /// </summary>
    public Fox Fox { get; }

    public Location(
        Guid id,
        string name,
        LocationType type,
        double lat,
        double lon,
        Fox fox)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }

        if (type == LocationType.Fox && fox == null)
        {
            throw new ArgumentException(nameof(fox));
        }

        if (type != LocationType.Fox && fox != null)
        {
            throw new ArgumentException(nameof(fox));
        }

        Id = id;
        Name = name;
        Type = type;
        Lat = lat;
        Lon = lon;
        Fox = fox;
    }
}