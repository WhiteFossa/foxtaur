namespace Foxtaur.LibRenderer.Helpers;

/// <summary>
/// Helper for geographic stuff
/// </summary>
public static class GeoHelper
{
    /// <summary>
    /// Radians to degrees string
    /// </summary>
    public static string ToDegreesString(this float radians)
    {
        var degreesRaw = radians * 180.0f / (float)Math.PI ;

        var degrees = (int)degreesRaw;

        var minutesRaw = 60.0f * (degreesRaw - degrees);

        var minutes = (int)minutesRaw;

        var seconds = 60.0f * (minutesRaw - minutes);
        
        return $"{ degrees }° { minutes }' { seconds }''";
    }
}