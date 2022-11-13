using Foxtaur.LibRenderer.Constants;

namespace Foxtaur.LibRenderer.Models;

/// <summary>
/// Geo point (always 3D - lat, lon, h)
/// </summary>
public class GeoPoint
{
    /// <summary>
    /// Latitude
    /// </summary>
    public float Lat { get; }

    /// <summary>
    /// Longitude
    /// </summary>
    public float Lon { get; }

    /// <summary>
    /// Height
    /// </summary>
    public float H { get; }

    public GeoPoint(float lat, float lon, float h)
    {
        if (lat < RendererConstants.MinLat || lat > RendererConstants.MaxLat)
        {
            throw new ArgumentException(nameof(lat));
        }
        
        if (lon < RendererConstants.MinLon || lat > RendererConstants.MaxLon)
        {
            throw new ArgumentException(nameof(lon));
        }

        if (h < RendererConstants.MinH || h > RendererConstants.MaxH)
        {
            throw new ArgumentException(nameof(h));
        }

        Lat = lat;
        Lon = lon;
        H = h;
    }
}