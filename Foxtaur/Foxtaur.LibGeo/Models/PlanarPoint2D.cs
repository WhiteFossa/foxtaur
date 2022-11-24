namespace Foxtaur.LibGeo.Models;

/// <summary>
///     Point with planar coordinates (2D)
/// </summary>
public class PlanarPoint2D
{
    /// <summary>
    ///     Planar X
    /// </summary>
    public float X { get; }

    /// <summary>
    ///     Planar Y
    /// </summary>
    public float Y { get; }

    public PlanarPoint2D(float x, float y)
    {
        X = x;
        Y = y;
    }
}