using System.Numerics;
using Foxtaur.Helpers;
using Foxtaur.LibGeo.Models;

namespace Foxtaur.LibGeo.Helpers;

/// <summary>
/// Helper for geographic stuff
/// </summary>
public static class GeoHelper
{
    /// <summary>
    /// Radians to degrees string (sign is discarded)
    /// </summary>
    public static string ToDegreesStringSignless(this double radians, bool isLat)
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
    public static string ToLatString(this double lat)
    {
        var postfix = lat >= 0 ? "N" : "S";

        return $"{lat.ToDegreesStringSignless(true)}{postfix}";
    }

    /// <summary>
    /// Radians to longitude string
    /// </summary>
    public static string ToLonString(this double lon)
    {
        var postfix = lon >= 0 ? "W" : "E";

        return $"{lon.ToDegreesStringSignless(false)}{postfix}";
    }

    /// <summary>
    /// Vector of at least 3 components to PlanarPoint3D
    /// </summary>
    public static PlanarPoint3D AsPlanarPoint3D(this MathNet.Numerics.LinearAlgebra.Vector<double> vector)
    {
        return new PlanarPoint3D(vector[0], vector[1], vector[2]);
    }
}