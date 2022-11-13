using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibRenderer.Services.Implementations.CoordinateProviders;
using NLog;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer;

public class DesktopRendererControl : OpenGlControlBase
{
    /// <summary>
    /// Logger
    /// </summary>
    private Logger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Screen scaling
    /// </summary>
    private float _scaling;

    /// <summary>
    /// Viewport width
    /// </summary>
    private float _viewportWidth;

    /// <summary>
    /// Viewport height
    /// </summary>
    private float _viewportHeight;

    /// <summary>
    /// Renderer aspect ratio
    /// </summary>
    private float _aspectRatio;
    
    /// <summary>
    /// Silk.NET OpenGL context
    /// </summary>
    private GL _silkGLContext;
    
    private Shader _shader;

    /// <summary>
    ///     Camera
    /// </summary>
    private Camera _camera;

    /// <summary>
    /// Earth mesh
    /// </summary>
    private Mesh _earthMesh = new Mesh();
    
    private Texture _textureObject;
    
    // DI stuff
    private ICoordinatesProvider _sphereCoordinatesProvider = new SphereCoordinatesProvider();

    /// <summary>
    ///     Static constructor
    /// </summary>
    static DesktopRendererControl()
    {
        // Change of this fields require re-rendering
        //AffectsRender<DesktopRendererControl>(YawProperty, PitchProperty, RollProperty, DiscoProperty);
    }

    /// <summary>
    ///     Constructor
    /// </summary>
    public DesktopRendererControl()
    {
        // Creating camera
        _camera = new Camera()
        {
            Lat = 0.0f,
            Lon = 0.0f,
            H = RendererConstants.EarthRadius * 2.5f
        };

        // Loading points
        var earthVertices = new List<float>();

        var earthIndices = new List<uint>();

        uint verticesCounter = 0;

        var step = (float)Math.PI / 900.0f;

        for (var lat = (float)Math.PI / -2.0f; lat < (float)Math.PI / 2.0f; lat += step)
        {
            var latNorther = lat + step;

            for (var lon = (float)Math.PI; lon > -1.0f * (float)Math.PI; lon -= step)
            {
                var lonEaster = lon - step;

                var geoPoint0 = new GeoPoint(lat, lon, RendererConstants.EarthRadius);
                var geoPoint1 = new GeoPoint(latNorther, lon, RendererConstants.EarthRadius);
                var geoPoint2 = new GeoPoint(latNorther, lonEaster, RendererConstants.EarthRadius);
                var geoPoint3 = new GeoPoint(lat, lonEaster, RendererConstants.EarthRadius);

                var p3D0 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint0);
                var t2D0 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint0);

                var p3D1 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint1);
                var t2D1 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint1);

                var p3D2 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint2);
                var t2D2 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint2);

                var p3D3 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint3);
                var t2D3 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint3);

                var i3D0 = verticesCounter;
                earthVertices.Add(p3D0.X);
                earthVertices.Add(p3D0.Y);
                earthVertices.Add(p3D0.Z);
                earthVertices.Add(t2D0.X);
                earthVertices.Add(t2D0.Y);
                verticesCounter++;

                var i3D1 = verticesCounter;
                earthVertices.Add(p3D1.X);
                earthVertices.Add(p3D1.Y);
                earthVertices.Add(p3D1.Z);
                earthVertices.Add(t2D1.X);
                earthVertices.Add(t2D1.Y);
                verticesCounter++;

                var i3D2 = verticesCounter;
                earthVertices.Add(p3D2.X);
                earthVertices.Add(p3D2.Y);
                earthVertices.Add(p3D2.Z);
                earthVertices.Add(t2D2.X);
                earthVertices.Add(t2D2.Y);
                verticesCounter++;

                var i3D3 = verticesCounter;
                earthVertices.Add(p3D3.X);
                earthVertices.Add(p3D3.Y);
                earthVertices.Add(p3D3.Z);
                earthVertices.Add(t2D3.X);
                earthVertices.Add(t2D3.Y);
                verticesCounter++;

                earthIndices.Add(i3D0);
                earthIndices.Add(i3D1);
                earthIndices.Add(i3D2);

                earthIndices.Add(i3D2);
                earthIndices.Add(i3D3);
                earthIndices.Add(i3D0);
            }
        }
        
        _earthMesh.Vertices = earthVertices.ToArray();
        _earthMesh.Indices = earthIndices.ToArray();
    }

    /// <summary>
    ///     OpenGL initialization
    /// </summary>
    protected unsafe override void OnOpenGlInit(GlInterface gl, int fb)
    {
        base.OnOpenGlInit(gl, fb);

        CalculateViewportSizes();

        _silkGLContext = GL.GetApi(gl.GetProcAddress);
        
        // Loading shaders
        _shader = new Shader(_silkGLContext, @"Resources/Shaders/shader.vert", @"Resources/Shaders/shader.frag");

        // Earth
        _earthMesh.GenerateBuffers(_silkGLContext);
        
        // Loading texture
        _textureObject = new Texture(_silkGLContext, @"Resources/Textures/Basemaps/HYP_50M_SR_W_SMALL.jpeg");
    }

    /// <summary>
    ///     Called when frame have to be rendered
    /// </summary>
    protected unsafe override void OnOpenGlRender(GlInterface gl, int fb)
    {
        CalculateViewportSizes();
        
        _silkGLContext.ClearColor(Color.Black);
        _silkGLContext.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        _silkGLContext.Enable(EnableCap.DepthTest);

        _silkGLContext.Viewport(0, 0, (uint)_viewportWidth, (uint)_viewportHeight); // TODO: Move me to constants

        _shader.Use();

        // Projections
        var cameraPositionP3D = _camera.GetCameraPosition();
        var cameraPosition = new Vector3(cameraPositionP3D.X, cameraPositionP3D.Y, cameraPositionP3D.Z);
        var cameraTarget = new Vector3(0.0f, 0.0f, 0.0f);
        var cameraDirection = Vector3.Normalize(cameraPosition - cameraTarget);

        var cameraUp = new Vector3(0.0f, -1.0f, 0.0f);

        var model = Matrix4x4.CreateRotationZ(0.0f) * Matrix4x4.CreateRotationY(0.0f) * Matrix4x4.CreateRotationX(0.0f); // Rotation
        var view = Matrix4x4.CreateLookAt(cameraPosition, cameraDirection, cameraUp); // Camera position
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(1.0f, _aspectRatio, 0.1f, 100.0f); // Zoom

        // Setting shader parameters (common)
        _shader.SetUniform2f("resolution", new Vector2(_viewportWidth, _viewportHeight));
        
        // Setting shader parameters (vertex)
        _shader.SetUniform4f("uModel", model);
        _shader.SetUniform4f("uView", view);
        _shader.SetUniform4f("uProjection", projection);
        
        // Setting shader parameters (fragments)
        _shader.SetUniform1i("ourTexture", 0);
        _textureObject.Bind();

        //_silkGLContext.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

        // Earth
        _earthMesh.BindBuffers();
        _silkGLContext.DrawElements(PrimitiveType.Triangles, (uint)_earthMesh.Indices.Length, DrawElementsType.UnsignedInt, null);

        // Rotate camera (debug)
        _camera.Lon += (float)Math.PI / 400.0f;
        if (_camera.Lon > Math.PI)
        {
            _camera.Lon -= 2.0f * (float)Math.PI;
        }

        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    /// <summary>
    ///     OpenGL de-initialization
    /// </summary>
    protected override void OnOpenGlDeinit(GlInterface gl, int fb)
    {
        // Earth
        _earthMesh.Dispose();
        _textureObject.Dispose();
        
        _shader.Dispose();
        base.OnOpenGlDeinit(gl, fb);
    }

    /// <summary>
    /// Calculate scaling and viewport sizes
    /// </summary>
    private void CalculateViewportSizes()
    {
        _scaling = (float)VisualRoot.RenderScaling;

        _viewportWidth = (float)Bounds.Width * _scaling;
        _viewportHeight = (float)Bounds.Height * _scaling;

        _aspectRatio = _viewportWidth / _viewportHeight;
    }
}