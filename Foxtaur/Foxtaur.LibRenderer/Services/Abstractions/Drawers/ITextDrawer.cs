using Foxtaur.LibRenderer.Models;
using ImageMagick;

namespace Foxtaur.LibRenderer.Services.Abstractions.Drawers;

/// <summary>
/// Draws texts on images
/// </summary>
public interface ITextDrawer
{
    /// <summary>
    /// Draw given text on an image
    /// </summary>
    void DrawText(MagickImage image, int size, MagickColor color, PlanarPoint2D origin, string text);
}