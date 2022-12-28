using Foxtaur.LibWebClient.Enums;

namespace Foxtaur.LibWebClient.Models.DTOs;

/// <summary>
/// Locations (they are part of distance)
/// </summary>
public class LocationDto
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
    /// Id of fox, installed in this location.
    /// Valid only if Type == Fox, otherwise must be null.
    /// </summary>
    public Guid? FoxId { get; }

    public LocationDto(
        Guid id,
        string name,
        LocationType type,
        double lat,
        double lon,
        Guid? foxId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }

        if (type == LocationType.Fox && !foxId.HasValue)
        {
            throw new ArgumentException(nameof(foxId));
        }

        if (type != LocationType.Fox && foxId.HasValue)
        {
            throw new ArgumentException(nameof(foxId));
        }

        Id = id;
        Name = name;
        Type = type;
        Lat = lat;
        Lon = lon;
        FoxId = foxId;
    }
}