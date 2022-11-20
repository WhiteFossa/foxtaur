using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Services.Abstractions.Drawers;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Foxtaur.LibRenderer.Services.Implementations.Drawers;

public class TextDrawer : ITextDrawer
{
    public Font LoadFontFromFile(string path, int size, FontStyle style)
    {
        
        var collection = new FontCollection();
        var fontFamily = collection.Add(path);
        return fontFamily.CreateFont(size, style);
    }

    public void DrawText(Image<Rgba32> image, Font font, Color color, PlanarPoint2D origin, string text)
    {
        image.Mutate(x=> x.DrawText(text, font, color, new PointF(origin.X, origin.Y)));
    }
}