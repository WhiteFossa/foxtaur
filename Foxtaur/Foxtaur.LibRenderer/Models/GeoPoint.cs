namespace Foxtaur.LibRenderer.Models;

/// <summary>
/// Geo point (always 3D - lat, lon, h)
/// </summary>
public class GeoPoint
{
    /// <summary>
    /// Latitude
    /// </summary>
    public float Lat { get; set; }

    /// <summary>
    /// Longitude
    /// </summary>
    public float Lon { get; set; }

    /// <summary>
    /// Height
    /// </summary>
    public float H { get; set; }
}