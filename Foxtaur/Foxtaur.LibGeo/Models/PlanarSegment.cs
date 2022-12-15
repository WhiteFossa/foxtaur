namespace Foxtaur.LibGeo.Models;

/// <summary>
/// Planar segment (rectangle)
/// </summary>
public class PlanarSegment
{
    /// <summary>
    /// Top
    /// </summary>
    public double Top { get; private set; }

    /// <summary>
    /// Bottom
    /// </summary>
    public double Bottom { get; private set; }

    /// <summary>
    /// Left
    /// </summary>
    public double Left { get; private set; }

    /// <summary>
    /// Right
    /// </summary>
    public double Right { get; private set; }

    public PlanarSegment(double top, double left, double bottom, double right)
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