using System;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.UI;
using Foxtaur.LibRenderer.Constants;
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
    /// Rectangle mesh for UI top panel
    /// </summary>
    private Mesh _uiMeshTop;
    
    /// <summary>
    /// Texture for UI top panel
    /// </summary>
    private Texture _uiTextureTop;
    
    /// <summary>
    /// Shader for UI
    /// </summary>
    private Shader _uiShader;
    
    public Ui(ITextDrawer textDrawer,
        IRectangleGenerator rectangleGenerator)
    {
        _textDrawer = textDrawer;
        _rectangleGenerator = rectangleGenerator;
    }

    public void Initialize(GL silkGlContext, int uiWidth, int uiHeight, UiData data)
    {
        _ = silkGlContext ?? throw new ArgumentNullException(nameof(silkGlContext));
        _ = data ?? throw new ArgumentNullException(nameof(data));
        
        // Top
        var topPanelRelativeHeight = RendererConstants.UiTopPanelHeight / (float)uiHeight;
        _uiMeshTop = _rectangleGenerator.GenerateRectangle(
            new PlanarPoint3D(0.0f, topPanelRelativeHeight, 0.0f),
            new PlanarPoint2D(0.0f, topPanelRelativeHeight),
            new PlanarPoint3D(1.0f, 0.0f, 0.0f),
            new PlanarPoint2D(1.0f, 0.0f));
        
        _uiMeshTop.GenerateBuffers(silkGlContext);
        
        // Bottom
        
        _uiShader = new Shader(silkGlContext, @"Resources/Shaders/ui_shader.vert", @"Resources/Shaders/ui_shader.frag");
        
        // Initial generation
        GenerateUi(silkGlContext, uiWidth, uiHeight, data);
    }

    public void DeInitialize()
    {
        _uiTextureTop.Dispose();
        _uiMeshTop.Dispose();
        _uiShader.Dispose();
    }

    private void GenerateUi(GL silkGlContext, int uiWidth, int uiHeight, UiData data)
    {
        _ = silkGlContext ?? throw new ArgumentNullException(nameof(silkGlContext));
        _ = data ?? throw new ArgumentNullException(nameof(data));

        using (var uiImage = new MagickImage(new MagickColor(30, 30, 60, 128), uiWidth, uiHeight))
        {
            _textDrawer.DrawText(uiImage, 30, RendererConstants.UiTextColor,  new PlanarPoint2D(0, 30), $"FPS: { data.Fps }");
            
            _uiTextureTop = new Texture(silkGlContext, uiImage);    
        }
    }

    public unsafe void DrawUi(GL silkGlContext, int uiWidth, int uiHeight, UiData uiData)
    {
        _ = silkGlContext ?? throw new ArgumentNullException(nameof(silkGlContext));
        _ = uiData ?? throw new ArgumentNullException(nameof(uiData));

        if (uiData.IsRegenerationRequested)
        {
            GenerateUi(silkGlContext, uiWidth, uiHeight, uiData);
            uiData.MarkAsRegenerated();
        }
        
        silkGlContext.Disable(EnableCap.DepthTest);
        _uiShader.Use();
        _uiShader.SetUniform1i("ourTexture", 0);
    
        _uiMeshTop.BindBuffers(silkGlContext);
        _uiTextureTop.Bind();
        silkGlContext.DrawElements(PrimitiveType.Triangles, (uint)_uiMeshTop.Indices.Count, DrawElementsType.UnsignedInt, null);
    }
}