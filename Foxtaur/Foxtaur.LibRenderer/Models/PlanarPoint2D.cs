using Foxtaur.LibRenderer.Constants;

namespace Foxtaur.LibRenderer.Models;

/// <summary>
/// Point with planar coordinates (2D)
/// </summary>
public class PlanarPoint2D
{
    /// <summary>
    /// Planar X
    /// </summary>
    public float X { get; }

    /// <summary>
    /// Planar Y
    /// </summary>
    public float Y { get; }

    public PlanarPoint2D(float x, float y)
    {
        if (x < RendererConstants.MinPlanarCoord || x > RendererConstants.MaxPlanarCoord)
        {
            throw new ArgumentException(nameof(x));
        }
        
        if (y < RendererConstants.MinPlanarCoord || y > RendererConstants.MaxPlanarCoord)
        {
            throw new ArgumentException(nameof(y));
        }

        X = x;
        Y = y;
    }
}