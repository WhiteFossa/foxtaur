using System.Numerics;
using Foxtaur.LibGeo.Models;

namespace Foxtaur.LibGeo.Helpers;

/// <summary>
/// Helper for geographic stuff
/// </summary>
public static class GeoHelper
{
    /// <summary>
    /// Radians to degrees
    /// </summary>
    public static float ToDegrees(this float radians)
    {
        return radians * 180.0f / (float)Math.PI;
    }
    
    /// <summary>
    /// Radians to degrees string (sign is discarded)
    /// </summary>
    public static string ToDegreesStringSignless(this float radians, bool isLat)
    {
        var degreesRaw = Math.Abs(radians.ToDegrees());

        var degrees = (int)degreesRaw;

        var minutesRaw = 60.0f * (degreesRaw - degrees);

        var minutes = (int)minutesRaw;

        var seconds = 60.0f * (minutesRaw - minutes);

        return isLat ? $"{degrees:00}° {minutes:00}' {seconds:00}''" : $"{degrees:000}° {minutes:00}' {seconds:00}''";
    }

    /// <summary>
    /// Radians to latitude string
    /// </summary>
    public static string ToLatString(this float lat)
    {
        var postfix = lat >= 0 ? "N" : "S";

        return $"{lat.ToDegreesStringSignless(true)}{postfix}";
    }

    /// <summary>
    /// Radians to longitude string
    /// </summary>
    public static string ToLonString(this float lon)
    {
        var postfix = lon >= 0 ? "W" : "E";

        return $"{lon.ToDegreesStringSignless(false)}{postfix}";
    }

    /// <summary>
    /// Vector3 to PlanarPorint3D
    /// </summary>
    public static PlanarPoint3D AsPlanarPoint3D(this Vector3 vector)
    {
        return new PlanarPoint3D(vector.X, vector.Y, vector.Z);
    }
}