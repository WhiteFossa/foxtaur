namespace Foxtaur.LibGeo.Models;

/// <summary>
/// Planar segment (rectangle)
/// </summary>
public class PlanarSegment
{
    /// <summary>
    /// Top
    /// </summary>
    public float Top { get; private set; }

    /// <summary>
    /// Bottom
    /// </summary>
    public float Bottom { get; private set; }

    /// <summary>
    /// Left
    /// </summary>
    public float Left { get; private set; }

    /// <summary>
    /// Right
    /// </summary>
    public float Right { get; private set; }

    public PlanarSegment(float top, float left, float bottom, float right)
    {
        Top = top;
        Bottom = bottom;
        Left = left;
        Right = right;
    }
}