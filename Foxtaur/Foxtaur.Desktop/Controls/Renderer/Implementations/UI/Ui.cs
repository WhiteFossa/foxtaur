using System;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
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
    private IRectangleGenerator _rectangleGenerator;

    /// <summary>
    /// Rectangle mesh for UI
    /// </summary>
    private Mesh _uiMesh;
    
    /// <summary>
    /// Shader for UI
    /// </summary>
    private Shader _uiShader;
    
    /// <summary>
    /// Texture, where UI is being drawn
    /// </summary>
    private Texture _uiTexture;
    
    public Ui(ITextDrawer textDrawer,
        IRectangleGenerator rectangleGenerator)
    {
        _textDrawer = textDrawer;
        _rectangleGenerator = rectangleGenerator;
    }

    public void Initialize(GL silkGlContext)
    {
        _ = silkGlContext ?? throw new ArgumentNullException(nameof(silkGlContext));
        
        _uiMesh = _rectangleGenerator.GenerateRectangle(
            new PlanarPoint3D(0.0f, 1.0f, 0.0f),
            new PlanarPoint2D(0.0f, 1.0f),
            new PlanarPoint3D(1.0f, 0.0f, 0.0f),
            new PlanarPoint2D(1.0f, 0.0f));
        
        _uiMesh.GenerateBuffers(silkGlContext);
        
        _uiShader = new Shader(silkGlContext, @"Resources/Shaders/ui_shader.vert", @"Resources/Shaders/ui_shader.frag");
    }

    public void DeInitialize()
    {
        DisposeUiTexture();
        _uiMesh.Dispose();
        _uiShader.Dispose();
    }

    public void GenerateUi(GL silkGlContext, int uiWidth, int uiHeight, UiData data)
    {
        _ = silkGlContext ?? throw new ArgumentNullException(nameof(silkGlContext));

        using (var uiImage = new MagickImage(MagickColors.Transparent, uiWidth, uiHeight))
        {
            _textDrawer.DrawText(uiImage, 72, MagickColors.Red,  new PlanarPoint2D(0, 72), $"FPS: { data.Fps }");
            
            _uiTexture = new Texture(silkGlContext, uiImage);    
        }
    }

    public unsafe void DrawUi(GL silkGlContext)
    {
        // UI is not ready yet
        if (_uiTexture == null)
        {
            return;
        }
        
        silkGlContext.Disable(EnableCap.DepthTest);
        _uiShader.Use();
        _uiShader.SetUniform1i("ourTexture", 0);
    
        _uiMesh.BindBuffers(silkGlContext);
        _uiTexture.Bind();
        silkGlContext.DrawElements(PrimitiveType.Triangles, (uint)_uiMesh.Indices.Count, DrawElementsType.UnsignedInt, null);
    }

    private void DisposeUiTexture()
    {
        _uiTexture.Dispose();
        _uiTexture = null;
    }
}