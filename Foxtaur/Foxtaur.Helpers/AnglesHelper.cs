namespace Foxtaur.Helpers;

/// <summary>
/// Operations with angles
/// </summary>
public static class AnglesHelper
{
    /// <summary>
    /// Add one angle to another (in radians) with wrap. Result will be in [0; 2 * Pi] range
    /// </summary>
    public static double AddAngleWithWrap(this double angle1, double angle2)
    {
        var result = angle1 + angle2;

        while (result < 0)
        {
            result += 2 * Math.PI;
        }

        while (result > 2 * Math.PI)
        {
            result -= 2 * Math.PI;
        }

        return result;
    }
}