using Foxtaur.LibRenderer.Constants;

namespace Foxtaur.LibRenderer.Models;

/// <summary>
///     Geo point (always 3D - lat, lon, h)
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

        Lat = lat;
        Lon = lon;
        H = h;
    }

    /// <summary>
    /// Add lat + delta, wrap over the Earth
    /// </summary>
    public static float SumLatitudesWithWrap(float lat, float delta)
    {
        var result = lat + delta;

        if (result > RendererConstants.MaxLat)
        {
            result = 2 * RendererConstants.MaxLat - result;
        }
        else if (result < RendererConstants.MinLat)
        {
            result = result + 2 * RendererConstants.MaxLat;
        }

        return result;
    }

    public static float SumLongitudesWithWrap(float lon, float delta)
    {
        var result = lon + delta;

        while (result > (float)Math.PI)
        {
            result -= 2.0f * (float)Math.PI;
        }

        while (result < -1.0f * (float)Math.PI)
        {
            result += 2.0f * (float)Math.PI;
        }

        return result;
    }
}