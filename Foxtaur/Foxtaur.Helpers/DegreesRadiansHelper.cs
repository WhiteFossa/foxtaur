namespace Foxtaur.Helpers;

/// <summary>
/// Degrees to radians and back
/// </summary>
public static class DegreesRadiansHelper
{
    /// <summary>
    /// Radians to degrees
    /// </summary>
    public static float ToDegrees(this float radians)
    {
        return radians * 180.0f / (float)Math.PI;
    }

    /// <summary>
    /// Degrees to radians
    /// </summary>
    public static float ToRadians(this float degrees)
    {
        return degrees * (float)Math.PI / 180.0f;
    }
}