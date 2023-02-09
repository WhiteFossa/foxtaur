using System;
using System.Collections.Generic;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.UI;
using Foxtaur.Desktop.Controls.Renderer.Models;
using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibGeo.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Models.UI;
using Foxtaur.LibRenderer.Services.Abstractions.Camera;
using Foxtaur.LibRenderer.Services.Abstractions.Drawers;
using ImageMagick;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer.Implementations.UI;

public class HuntersDisplay : IHuntersDisplay
{
    private readonly ITextDrawer _textDrawer;
    private readonly IRectangleGenerator _rectangleGenerator;
    private readonly ICamera _camera;
    private ICoordinatesProvider _sphereCoordinatesProvider;
    
    public bool IsNeedToReinitialize { get; set; }

    /// <summary>
    /// Shader (it's common with UI-class shader)
    /// </summary>
    private Shader _uiShader;
    
    public HuntersDisplay(ITextDrawer textDrawer,
        IRectangleGenerator rectangleGenerator,
        ICamera camera,
        ISphereCoordinatesProvider sphereCoordinatesProvider)
    {
        _textDrawer = textDrawer;
        _rectangleGenerator = rectangleGenerator;
        _camera = camera;
        _sphereCoordinatesProvider = sphereCoordinatesProvider;
    }
    
    public void Initialize(GL silkGlContext, int uiWidth, int uiHeight)
    {
        _ = silkGlContext ?? throw new ArgumentNullException(nameof(silkGlContext));
        
        _uiShader = new Shader(silkGlContext, @"Resources/Shaders/ui_shader.vert", @"Resources/Shaders/ui_shader.frag");
    }

    public void DeInitialize()
    {
        _uiShader.Dispose();
    }

    public unsafe void DrawUi(GL silkGlContext, int uiWidth, int uiHeight, IReadOnlyCollection<Hunter> hunters)
    {
        _ = silkGlContext ?? throw new ArgumentNullException(nameof(silkGlContext));
        _ = hunters ?? throw new ArgumentNullException(nameof(hunters));

        if (IsNeedToReinitialize)
        {
            DeInitialize();
            Initialize(silkGlContext, uiWidth, uiHeight);

            IsNeedToReinitialize = false;
        }
        

        silkGlContext.Disable(EnableCap.DepthTest);
        _uiShader.Use();
        _uiShader.SetUniform1i("ourTexture", 0);

        foreach (var hunter in hunters)
        { 
            var hunterPosition3D = _sphereCoordinatesProvider.GeoToPlanar3D(hunter.Position);
            var hunterDisplayPosition = _camera.ProjectPointToViewportNormalized(hunterPosition3D);

            var hunterMesh = _rectangleGenerator.GenerateRectangle(
                new PlanarPoint3D(hunterDisplayPosition.X - RendererConstants.FlatUiHunterHalfWidth, hunterDisplayPosition.Y, 0.0),
                new PlanarPoint2D(0.0, 1.0),
                new PlanarPoint3D(hunterDisplayPosition.X + RendererConstants.FlatUiHunterHalfWidth, hunterDisplayPosition.Y - RendererConstants.FlatUiHunterHeight, 0.0),
                new PlanarPoint2D(1.0, 0.0));

            hunterMesh.GenerateBuffers(silkGlContext);
            hunterMesh.BindBuffers(silkGlContext);
            
            hunter.UiTexture.Bind();
            
            silkGlContext.DrawElements(PrimitiveType.Triangles, (uint)hunterMesh.Indices.Count, DrawElementsType.UnsignedInt, null);
            
            hunterMesh.Dispose();
        }
    }
}