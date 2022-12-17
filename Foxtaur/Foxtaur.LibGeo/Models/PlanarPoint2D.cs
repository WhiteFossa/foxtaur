namespace Foxtaur.LibGeo.Models;

/// <summary>
///     Point with planar coordinates (2D)
/// </summary>
public class PlanarPoint2D
{
    /// <summary>
    ///     Planar X
    /// </summary>
    public double X { get; }

    /// <summary>
    ///     Planar Y
    /// </summary>
    public double Y { get; }

    public PlanarPoint2D(double x, double y)
    {
        X = x;
        Y = y;
    }
}