using Foxtaur.LibGeo.Models;

namespace Foxtaur.LibRenderer.Models;

/// <summary>
/// Sphere in 3D space
/// </summary>
public class Sphere
{
    /// <summary>
    /// Center
    /// </summary>
    public PlanarPoint3D Center { get; private set; }

    /// <summary>
    /// Radius
    /// </summary>
    public double Radius { get; private set; }

    public Sphere(PlanarPoint3D cener, double radius)
    {
        Center = cener;
        Radius = radius;
    }
}