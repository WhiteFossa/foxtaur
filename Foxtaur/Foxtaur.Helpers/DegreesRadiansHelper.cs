namespace Foxtaur.Helpers;

/// <summary>
/// Degrees to radians and back
/// </summary>
public static class DegreesRadiansHelper
{
    /// <summary>
    /// Radians to degrees
    /// </summary>
    public static double ToDegrees(this double radians)
    {
        return radians * 180.0f / Math.PI;
    }

    /// <summary>
    /// Degrees to radians
    /// </summary>
    public static double ToRadians(this double degrees)
    {
        return degrees * Math.PI / 180.0f;
    }
}