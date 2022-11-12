using System.Drawing;
using System.Drawing.Imaging;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Services.Abstractions;
using ImageMagick;

namespace Foxtaur.LibRenderer.Services.Implementations;

public class TexturesLoader : ITexturesLoader
{
    public Texture LoadTextureFromFile(string path)
    {
        var image = new MagickImage(path);
        var pc = image.GetPixels();
        
        var result = new Texture();
        
        result.Width = image.Width;
        result.Height = image.Height;
        
        result.Data = new byte[result.Width * result.Height * 3];
        
        for (var y = 0; y < result.Height; y++)
        {
            for (var x = 0; x < result.Width; x++)
            {
                var pixel = pc.GetValue(x, y);

                result.Data[y * result.Width + x] = pixel[0];
                result.Data[y * result.Width + x + 1] = pixel[1];
                result.Data[y * result.Width + x + 2] = pixel[2];
            }
        }

        return result;
    }
}