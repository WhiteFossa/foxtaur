using Foxtaur.LibGeo.Constants;

namespace Foxtaur.LibGeo.Models;

/// <summary>
/// Geo point (always 3D - lat, lon, h)
/// </summary>
public class GeoPoint
{
    /// <summary>
    /// Latitude
    /// </summary>
    public double Lat { get; }

    /// <summary>
    /// Longitude
    /// </summary>
    public double Lon { get; }

    /// <summary>
    /// Height
    /// </summary>
    public double H { get; }

    public GeoPoint(double lat, double lon, double h)
    {
        if (lat < GeoConstants.MinLat || lat > GeoConstants.MaxLat)
        {
            throw new ArgumentException(nameof(lat));
        }

        if (lon < GeoConstants.MinLon || lat > GeoConstants.MaxLon)
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
    public static double SumLatitudesWithWrap(double lat, double delta)
    {
        var result = lat + delta;

        if (result > GeoConstants.MaxLat)
        {
            result = 2 * GeoConstants.MaxLat - result;
        }
        else if (result < GeoConstants.MinLat)
        {
            result = result + 2 * GeoConstants.MaxLat;
        }

        return result;
    }

    public static double SumLongitudesWithWrap(double lon, double delta)
    {
        var result = lon + delta;

        while (result > Math.PI)
        {
            result -= 2.0 * Math.PI;
        }

        while (result < -1.0 * Math.PI)
        {
            result += 2.0 * Math.PI;
        }

        return result;
    }
}