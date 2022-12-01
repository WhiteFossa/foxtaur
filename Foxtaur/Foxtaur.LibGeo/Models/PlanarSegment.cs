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
        if (top < bottom) // Not <= because we can have degenerate segments. The same is for left - right
        {
            throw new ArgumentException();
        }

        if (left > right)
        {
            throw new ArgumentException();
        }

        Top = top;
        Bottom = bottom;
        Left = left;
        Right = right;
    }
}