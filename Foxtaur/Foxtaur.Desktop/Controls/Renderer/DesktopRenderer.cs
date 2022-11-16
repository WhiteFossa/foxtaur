using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Services.Abstractions.CoordinateProviders;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Earth;
using Foxtaur.LibRenderer.Helpers;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Services.Abstractions.Camera;
using Foxtaur.LibRenderer.Services.Implementations.Camera;

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
    /// Silk.NET OpenGL context
    /// </summary>
    private GL _silkGLContext;

    private Shader _shader;

    /// <summary>
    /// Camera
    /// </summary>
    private readonly ICamera _camera = Program.Di.GetService<ICamera>();

    #region Earth

    /// <summary>
    /// Earth sphere (for raycasting)
    /// </summary>
    private Sphere _earthSphere;
    
    /// <summary>
    /// Earth mesh
    /// </summary>
    private Mesh _earthMesh;
    
    /// <summary>
    /// Earth texture
    /// </summary>
    private Texture _earthTexture;

    #endregion

    private Texture _redDebugTexture;

    /// <summary>
    /// True if mouse left button pressed and not released yet
    /// </summary>
    private bool _isMouseLeftButtonPressed = false;

    private PlanarPoint3D _debugPoint;
    
    // DI stuff
    private ICoordinatesProvider _sphereCoordinatesProvider = Program.Di.GetService<ISphereCoordinatesProvider>();
    private IEarthGenerator _earthGenerator = Program.Di.GetService<IEarthGenerator>();
    
    /// <summary>
    /// Constructor
    /// </summary>
    public DesktopRendererControl()
    {
        // Generating the Earth
        _earthSphere = _earthGenerator.GenerateEarthSphere();
        _earthMesh = _earthGenerator.GenerateFullEarth((float)Math.PI / 90.0f);
        
        // Creating camera
        _camera.Lat = 0.0f;
        _camera.Lon = 0.0f;
        _camera.H = RendererConstants.CameraOrbitHeight;
        _camera.Zoom = RendererConstants.CameraMinZoom;

        // Targetting camera
        _camera.Target= new Vector3(0.0f, 0.0f, 0.0f).AsPlanarPoint3D();

        // Setting-up input events
        PointerWheelChanged += OnWheel;
        PointerPressed += OnMousePressed;
        PointerReleased += OnMouseReleased;
        PointerMoved += OnMouseMoved;
    }

    /// <summary>
    /// OpenGL initialization
    /// </summary>
    protected unsafe override void OnOpenGlInit(GlInterface gl, int fb)
    {
        base.OnOpenGlInit(gl, fb);

        CalculateViewportSizes();

        _silkGLContext = GL.GetApi(gl.GetProcAddress);

        // Loading shaders
        _shader = new Shader(_silkGLContext, @"Resources/Shaders/shader.vert", @"Resources/Shaders/shader.frag");
        
        // Loading textures
        _earthTexture = new Texture(_silkGLContext, @"Resources/Textures/Basemaps/HYP_50M_SR_W_SMALL.jpeg");
        _redDebugTexture = new Texture(_silkGLContext, @"Resources/Textures/debugVector.png");
    }

    /// <summary>
    /// Called when frame have to be rendered
    /// </summary>
    protected unsafe override void OnOpenGlRender(GlInterface gl, int fb)
    {
        CalculateViewportSizes();

        _silkGLContext.ClearColor(Color.Black);
        _silkGLContext.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        _silkGLContext.Enable(EnableCap.DepthTest);

        _silkGLContext.Viewport(0, 0, (uint)_viewportWidth, (uint)_viewportHeight);

        _shader.Use();

        // Setting shader parameters (common)
        _shader.SetUniform2f("resolution", new Vector2(_viewportWidth, _viewportHeight));

        // Setting shader parameters (vertices)
        _shader.SetUniform4f("uModel", _camera.ModelMatrix);
        _shader.SetUniform4f("uView", _camera.ViewMatrix);
        _shader.SetUniform4f("uProjection", _camera.ProjectionMatrix);

        // Setting shader parameters (fragments)
        _shader.SetUniform1i("ourTexture", 0);
        _earthTexture.Bind();

        _silkGLContext.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

        // Earth
        _earthMesh.GenerateBuffers(_silkGLContext);
        _earthMesh.BindBuffers();
        _silkGLContext.DrawElements(PrimitiveType.Triangles, (uint)_earthMesh.Indices.Count, DrawElementsType.UnsignedInt, null);
        _earthMesh.Dispose();

        // Debug vector
        if (_debugPoint != null)
        {
            DrawDebugVector(_silkGLContext, new PlanarPoint3D(0, 0, 0), _debugPoint);
        }

        // Rotate camera (debug)
        if (!_isMouseLeftButtonPressed)
        {
            _camera.Lon += (float)Math.PI / 200.0f;
            if (_camera.Lon > Math.PI)
            {
                _camera.Lon -= 2.0f * (float)Math.PI;
            }
        }

        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    /// <summary>
    /// OpenGL de-initialization
    /// </summary>
    protected override void OnOpenGlDeinit(GlInterface gl, int fb)
    {
        _earthTexture.Dispose();

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

        _camera.AspectRatio = _viewportWidth / _viewportHeight;
    }

    /// <summary>
    /// Mouse wheel event
    /// </summary>
    private void OnWheel(object? sender, PointerWheelEventArgs e)
    {
        float steps = (float)Math.Abs(e.Delta.Y);

        if (e.Delta.Y > 0)
        {
            _camera.ZoomOut(steps);
        }
        else
        {
            _camera.ZoomIn(steps);
        }
    }

    /// <summary>
    /// Mouse pressed event
    /// </summary>
    private void OnMousePressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isMouseLeftButtonPressed = true;   
        }
    }
    
    /// <summary>
    /// Mouse released event
    /// </summary>
    private void OnMouseReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isMouseLeftButtonPressed = false;   
        }
    }
    
    /// <summary>
    /// Mouse move event
    /// </summary>
    private void OnMouseMoved(object? sender, PointerEventArgs e)
    {
        if (!_isMouseLeftButtonPressed)
        {
            return;
        }
        
        var x = (float)e.GetCurrentPoint(this).Position.X * _scaling;
        var y = (float)e.GetCurrentPoint(this).Position.Y * _scaling;
        
        MoveCameraByDragging(x, y);
    }

    /// <summary>
    /// Drag camera to a new position
    /// </summary>
    private void MoveCameraByDragging(float screenX, float screenY)
    {
        var cameraRay = Ray.CreateByScreenRaycasting(_camera, screenX, screenY, _viewportWidth, _viewportHeight);

        var intersections = cameraRay.Intersect(_earthSphere);
        if (intersections.Count < 2)
        {
            // Miss, don't touch the camera
            return;
        }
        
        // Closest to camera intersection
        var closestIntersection = _camera.Position3D.GetClosesPoint(intersections);

        _debugPoint = closestIntersection;
    }

    /// <summary>
    /// Draw debug vector
    /// </summary>
    private unsafe void DrawDebugVector(GL silkGlContext, PlanarPoint3D startPoint, PlanarPoint3D endPoint)
    {
        var mesh = new Mesh();
        mesh.AddVertex(new PlanarPoint3D(startPoint.X - 0.01f, startPoint.Y, startPoint.Z), new PlanarPoint2D(0, 0));
        mesh.AddVertex(new PlanarPoint3D(startPoint.X + 0.01f, startPoint.Y, startPoint.Z), new PlanarPoint2D(0, 1));
        
        mesh.AddVertex(new PlanarPoint3D(endPoint.X - 0.01f, endPoint.Y, endPoint.Z), new PlanarPoint2D(1, 0));
        mesh.AddVertex(new PlanarPoint3D(endPoint.X + 0.01f, endPoint.Y, endPoint.Z), new PlanarPoint2D(1, 1));
        
        mesh.AddIndex(0);
        mesh.AddIndex(1);
        mesh.AddIndex(2);
        
        mesh.AddIndex(2);
        mesh.AddIndex(3);
        mesh.AddIndex(1);
        
        mesh.GenerateBuffers(silkGlContext);
        
        mesh.BindBuffers();
        
        _redDebugTexture.Bind();
        silkGlContext.DrawElements(PrimitiveType.Triangles, (uint)mesh.Indices.Count, DrawElementsType.UnsignedInt, null);
        
        mesh.Dispose();
    }
}