using System;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.UI;
using Foxtaur.LibGeo.Helpers;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Models.UI;
using Foxtaur.LibRenderer.Services.Abstractions.Drawers;
using ImageMagick;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer.Implementations.UI;

public class Ui : IUi
{
    private readonly ITextDrawer _textDrawer;
    private readonly IRectangleGenerator _rectangleGenerator;

    public bool IsNeedToReinitialize { get; set; }

    /// <summary>
    /// Rectangle mesh for UI top panel
    /// </summary>
    private Mesh _uiMeshTop;

    /// <summary>
    /// Texture for UI top panel
    /// </summary>
    private Texture _uiTextureTop;

    /// <summary>
    /// Rectangle mesh for UI bottom panel
    /// </summary>
    private Mesh _uiMeshBottom;

    /// <summary>
    /// Texture for UI bottom panel
    /// </summary>
    private Texture _uiTextureBottom;

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
        var topPanelRelativeHeight = RendererConstants.UiTopPanelHeight / (double)uiHeight;
        _uiMeshTop = _rectangleGenerator.GenerateRectangle(
            new PlanarPoint3D(0.0, topPanelRelativeHeight, 0.0),
            new PlanarPoint2D(0.0, 1.0),
            new PlanarPoint3D(1.0, 0.0, 0.0),
            new PlanarPoint2D(1.0, 0.0));

        _uiMeshTop.GenerateBuffers(silkGlContext);

        // Bottom
        var bottomPanelRelativeHeight = RendererConstants.UiBottomPanelHeight / (double)uiHeight;
        _uiMeshBottom = _rectangleGenerator.GenerateRectangle(
            new PlanarPoint3D(0.0, 1.0, 0.0),
            new PlanarPoint2D(0.0, 1.0),
            new PlanarPoint3D(1.0, 1.0 - bottomPanelRelativeHeight, 0.0),
            new PlanarPoint2D(1.0, 0.0));

        _uiMeshBottom.GenerateBuffers(silkGlContext);

        _uiShader = new Shader(silkGlContext, @"Resources/Shaders/ui_shader.vert", @"Resources/Shaders/ui_shader.frag");

        // Initial generation
        GenerateUi(silkGlContext, uiWidth, uiHeight, data);
    }

    public void DeInitialize()
    {
        _uiTextureTop.Dispose();
        _uiMeshTop.Dispose();

        _uiTextureBottom.Dispose();
        _uiMeshBottom.Dispose();

        _uiShader.Dispose();
    }

    private void GenerateUi(GL silkGlContext, int uiWidth, int uiHeight, UiData data)
    {
        _ = silkGlContext ?? throw new ArgumentNullException(nameof(silkGlContext));
        _ = data ?? throw new ArgumentNullException(nameof(data));

        // Top
        using (var uiTopPanelImage = new MagickImage(RendererConstants.UiPanelsBackgroundColor, uiWidth,
                   RendererConstants.UiTopPanelHeight))
        {
            // FPS
            var fpsText = $"FPS: {data.Fps}";
            var fpsTextSize = _textDrawer.GetTextBounds(uiTopPanelImage, RendererConstants.UiFontSize, fpsText);
            var fpsTextShiftY = RendererConstants.UiTopPanelHeight - (RendererConstants.UiTopPanelHeight - fpsTextSize.TextHeight) / 2.0 + fpsTextSize.Descent;

            _textDrawer.DrawText(uiTopPanelImage,
                RendererConstants.UiFontSize,
                RendererConstants.UiTextColor,
                new PlanarPoint2D(RendererConstants.UiFpsTextXPosition, fpsTextShiftY),
                fpsText);

            if (_uiTextureTop != null)
            {
                _uiTextureTop.Dispose();
            }

            _uiTextureTop = new Texture(silkGlContext, uiTopPanelImage);
        }

        // Bottom
        using (var uiBottomPanelImage = new MagickImage(RendererConstants.UiPanelsBackgroundColor, uiWidth, RendererConstants.UiBottomPanelHeight))
        {
            var latText = data.IsMouseInEarth ? data.MouseLat.ToLatString() : "N/A";
            var lonText = data.IsMouseInEarth ? data.MouseLon.ToLonString() : "N/A";
            var altitudeText = data.IsMouseInEarth ? data.MouseH.ToAltitudeString() : "N/A";

            var bottomText = $"Latitude: {latText}, Longitude: {lonText}, Altitude: {altitudeText}";

            var bottomTextSize = _textDrawer.GetTextBounds(uiBottomPanelImage, RendererConstants.UiFontSize, bottomText);
            var bottomTextShiftY = RendererConstants.UiTopPanelHeight - (RendererConstants.UiTopPanelHeight - bottomTextSize.TextHeight) / 2.0 + bottomTextSize.Descent;

            _textDrawer.DrawText(uiBottomPanelImage,
                RendererConstants.UiFontSize,
                RendererConstants.UiTextColor,
                new PlanarPoint2D(RendererConstants.UiBottomTextXPosition, bottomTextShiftY),
                bottomText);

            if (_uiTextureBottom != null)
            {
                _uiTextureBottom.Dispose();
            }

            _uiTextureBottom = new Texture(silkGlContext, uiBottomPanelImage);
        }
    }

    public unsafe void DrawUi(GL silkGlContext, int uiWidth, int uiHeight, UiData uiData)
    {
        _ = silkGlContext ?? throw new ArgumentNullException(nameof(silkGlContext));
        _ = uiData ?? throw new ArgumentNullException(nameof(uiData));

        if (IsNeedToReinitialize)
        {
            DeInitialize();
            Initialize(silkGlContext, uiWidth, uiHeight, uiData);

            uiData.MarkForRegeneration();

            IsNeedToReinitialize = false;
        }


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

        _uiMeshBottom.BindBuffers(silkGlContext);
        _uiTextureBottom.Bind();
        silkGlContext.DrawElements(PrimitiveType.Triangles, (uint)_uiMeshBottom.Indices.Count, DrawElementsType.UnsignedInt, null);
    }
}