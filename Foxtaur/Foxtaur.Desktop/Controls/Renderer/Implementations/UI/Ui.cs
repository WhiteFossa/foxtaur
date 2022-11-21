using System;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.UI;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Models.UI;
using Foxtaur.LibRenderer.Services.Abstractions.Drawers;
using ImageMagick;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer.Implementations.UI;

public class Ui : IUi
{
    private ITextDrawer _textDrawer;

    public Ui(ITextDrawer textDrawer)
    {
        _textDrawer = textDrawer;
    }
    
    public Texture GenerateUi(GL silkGlContext, int uiWidth, int uiHeight, UiData data)
    {
        _ = silkGlContext ?? throw new ArgumentNullException(nameof(silkGlContext));

        using (var uiImage = new MagickImage(MagickColors.Transparent, uiWidth, uiHeight))
        {
            _textDrawer.DrawText(uiImage, 72, MagickColors.Red,  new PlanarPoint2D(0, 72), $"FPS: { data.Fps }");
            
            return new Texture(silkGlContext, uiImage);    
        }
    }
}