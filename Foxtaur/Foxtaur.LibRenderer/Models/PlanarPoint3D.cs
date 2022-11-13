using Foxtaur.LibRenderer.Constants;

namespace Foxtaur.LibRenderer.Models;

/// <summary>
/// Point with planar coordinates
/// </summary>
public class PlanarPoint3D : PlanarPoint2D
{
    /// <summary>
    /// Planar Z
    /// </summary>
    public float Z { get; }

    public PlanarPoint3D(float x, float y, float z) : base(x, y)
    {
        Z = z;
    }
}