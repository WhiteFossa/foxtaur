using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Services.Abstractions.Drawers;
using ImageMagick;

namespace Foxtaur.LibRenderer.Services.Implementations.Drawers;

public class TextDrawer : ITextDrawer
{
    public void DrawText(MagickImage image, int size, MagickColor color, PlanarPoint2D origin, string text)
    {
        new Drawables()
            .FontPointSize(size)
            .Font("Open Sans")
            .StrokeColor(color)
            .FillColor(color)
            .Text(origin.X, origin.Y, text)
            .Draw(image);
    }
}