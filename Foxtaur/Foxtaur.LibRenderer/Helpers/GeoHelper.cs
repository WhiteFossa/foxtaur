namespace Foxtaur.LibRenderer.Helpers;

/// <summary>
/// Helper for geographic stuff
/// </summary>
public static class GeoHelper
{
    /// <summary>
    /// Radians to degrees string (sign is discarded)
    /// </summary>
    public static string ToDegreesStringSignless(this float radians)
    {
        var degreesRaw = Math.Abs(radians * 180.0f / (float)Math.PI);

        var degrees = (int)degreesRaw;

        var minutesRaw = 60.0f * (degreesRaw - degrees);

        var minutes = (int)minutesRaw;

        var seconds = 60.0f * (minutesRaw - minutes);
        
        return $"{ degrees }° { minutes }' { seconds }''";
    }

    /// <summary>
    /// Radians to latitude string
    /// </summary>
    public static string ToLatString(this float lat)
    {
        var postfix = lat >= 0 ? "N" : "S";

        return $"{ lat.ToDegreesStringSignless() }{postfix}";
    }
    
    /// <summary>
    /// Radians to longitude string
    /// </summary>
    public static string ToLonString(this float lon)
    {
        var postfix = lon >= 0 ? "W" : "E";

        return $"{ lon.ToDegreesStringSignless() }{postfix}";
    }
}