using Foxtaur.LibRenderer.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Foxtaur.LibRenderer.Services.Abstractions.Drawers;

/// <summary>
/// Draws texts on images
/// </summary>
public interface ITextDrawer
{
    /// <summary>
    /// Load font from file
    /// </summary>
    Font LoadFontFromFile(string path, int size, FontStyle style);
    
    /// <summary>
    /// Draw given text on an image
    /// </summary>
    void DrawText(Image<Rgba32> image, Font font, Color color, PlanarPoint2D origin, string text);
}