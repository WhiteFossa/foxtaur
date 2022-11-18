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
using Plane = Foxtaur.LibRenderer.Models.Plane;

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
    private Texture _blueDebugTexture;

    #region Camera

    /// <summary>
    /// True if mouse left button pressed and not released yet
    /// </summary>
    private bool _isMouseLeftButtonPressed = false;

    /// <summary>
    /// Mouse was pressed in this location
    /// </summary>
    private GeoPoint _pressGeoPoint;

    #endregion

    /// <summary>
    /// Surface walk mode
    /// </summary>
    private bool _isSurfaceMode = false;

    #region DI

    private ICoordinatesProvider _sphereCoordinatesProvider = Program.Di.GetService<ISphereCoordinatesProvider>();
    private IEarthGenerator _earthGenerator = Program.Di.GetService<IEarthGenerator>();
    
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    public DesktopRendererControl()
    {
        // Generating the Earth
        _earthSphere = _earthGenerator.GenerateEarthSphere();
        _earthMesh = _earthGenerator.GenerateFullEarth((float)Math.PI / 900.0f);
        
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
        //_earthTexture = new Texture(_silkGLContext, @"Resources/Textures/davydovo.png");
        _redDebugTexture = new Texture(_silkGLContext, @"Resources/Textures/debugVector.png");
        _blueDebugTexture = new Texture(_silkGLContext, @"Resources/Textures/debugVectorBlue.png");
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

        if (_isSurfaceMode)
        {
            // Debugging stuff
            _camera.Lon = 0.0f;

            var cameraPosition = _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(_camera.Lat, _camera.Lon, _camera.H));
            
            // Nadir
            var nadirVector = new Ray(cameraPosition, new PlanarPoint3D(0, 0, 0));
            DrawDebugVectorRed(_silkGLContext, nadirVector.Begin, nadirVector.End);

            // North vector
            //var northPole = _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint((float)Math.PI / 2.0f, 0.0f, RendererConstants.EarthRadius));
            //var northVector = new Ray(_camera.Position3D, northPole);
            //DrawDebugVector(_silkGLContext, northVector.Begin, northVector.End);
            
            // Target vector
            var targetVector = new Vector3(nadirVector.End.X - nadirVector.Begin.X, nadirVector.End.Y - nadirVector.Begin.Y, nadirVector.End.Z - nadirVector.Begin.Z);
            targetVector = Vector3.Transform(targetVector, Matrix4x4.CreateRotationX((float)Math.PI / 2.0f - _camera.Lat));

            var targetRay = new Ray(cameraPosition, new PlanarPoint3D(cameraPosition.X + targetVector.X, cameraPosition.Y + targetVector.Y, cameraPosition.Z + targetVector.Z));
            DrawDebugVectorBlue(_silkGLContext, targetRay.Begin, targetRay.End);

            _camera.Target = targetRay.End;
            //_camera.Up = new Vector3(cameraPosition.X * -1.0f, cameraPosition.Y * -1.0f, cameraPosition.Z * -1.0f);
        }

        _shader.Use();

        // Setting shader parameters (common)
        _shader.SetUniform2f("resolution", new Vector2(_viewportWidth, _viewportHeight));

        // Setting shader parameters (vertices)
        _shader.SetUniform4f("uModel", _camera.ModelMatrix);
        _shader.SetUniform4f("uView", _camera.ViewMatrix);
        _shader.SetUniform4f("uProjection", _camera.ProjectionMatrix);

        // Setting shader parameters (fragments)
        _shader.SetUniform1i("ourTexture", 0);

        //_silkGLContext.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        
        // Earth
        _earthMesh.GenerateBuffers(_silkGLContext);
        _earthMesh.BindBuffers();
        _earthTexture.Bind();
        _silkGLContext.DrawElements(PrimitiveType.Triangles, (uint)_earthMesh.Indices.Count, DrawElementsType.UnsignedInt, null);
        _earthMesh.Dispose();

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
            // Left click - dragging the camera
            _isMouseLeftButtonPressed = true;
            
            var pressGeoPoint = GetMouseGeoCoordinates((float)e.GetCurrentPoint(this).Position.X, (float)e.GetCurrentPoint(this).Position.Y);
            if (pressGeoPoint != null)
            {
                _pressGeoPoint = pressGeoPoint;
            }
        }

        if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
        {
            // Middle click - surface mode
            if (!_isSurfaceMode)
            {
                _camera.H = RendererConstants.EarthRadius + 0.00001f;
            
                _isSurfaceMode = true;    
            }
            else
            {
                _camera.H = RendererConstants.CameraOrbitHeight;
                _camera.Target = new PlanarPoint3D(0, 0, 0);
                _camera.Up = new Vector3(0.0f, -1.0f, 0.0f);

                _isSurfaceMode = false;
            }
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
        
        var newGeoPoint = GetMouseGeoCoordinates((float)e.GetCurrentPoint(this).Position.X, (float)e.GetCurrentPoint(this).Position.Y);
        if (newGeoPoint != null)
        {
            var latDelta = GeoPoint.SumLatitudesWithWrap(newGeoPoint.Lat, -1.0f * _pressGeoPoint.Lat);
            var lonDelta = GeoPoint.SumLongitudesWithWrap(newGeoPoint.Lon, -1.0f * _pressGeoPoint.Lon);

            _camera.Lat = GeoPoint.SumLatitudesWithWrap(_camera.Lat, -1.0f * latDelta);
            _camera.Lon = GeoPoint.SumLongitudesWithWrap(_camera.Lon, -1.0f * lonDelta);
        }
    }

    /// <summary>
    /// Get mouse geographical coordinates (they might be null if mouse outside the Earth)
    /// </summary>
    private GeoPoint GetMouseGeoCoordinates(float screenX, float screenY)
    {
        var x = screenX * _scaling;
        var y = screenY * _scaling;
        
        var cameraRay = Ray.CreateByScreenRaycasting(_camera, x, y, _viewportWidth, _viewportHeight);

        var intersections = cameraRay.Intersect(_earthSphere);
        if (intersections.Count < 2)
        {
            // Miss
            return null;
        }
        
        // Closest to camera intersection
        var closestIntersection = _camera.Position3D.GetClosesPoint(intersections);
        return _sphereCoordinatesProvider.Planar3DToGeo(closestIntersection);
    }
    
    private unsafe void DrawDebugVectorRed(GL silkGlContext, PlanarPoint3D startPoint, PlanarPoint3D endPoint)
    {
        var mesh = new Mesh();
        mesh.AddVertex(new PlanarPoint3D(startPoint.X - 0.01f, startPoint.Y - 0.01f, startPoint.Z - 0.01f), new PlanarPoint2D(0, 0));
        mesh.AddVertex(new PlanarPoint3D(startPoint.X + 0.01f, startPoint.Y + 0.01f, startPoint.Z + 0.01f), new PlanarPoint2D(0, 1));

        mesh.AddVertex(new PlanarPoint3D(endPoint.X - 0.01f, endPoint.Y - 0.01f, endPoint.Z - 0.01f), new PlanarPoint2D(1, 0));
        mesh.AddVertex(new PlanarPoint3D(endPoint.X + 0.01f, endPoint.Y + 0.01f, endPoint.Z + 0.01f), new PlanarPoint2D(1, 1));
        
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
    
    private unsafe void DrawDebugVectorBlue(GL silkGlContext, PlanarPoint3D startPoint, PlanarPoint3D endPoint)
    {
        var mesh = new Mesh();
        mesh.AddVertex(new PlanarPoint3D(startPoint.X - 0.01f, startPoint.Y - 0.01f, startPoint.Z - 0.01f), new PlanarPoint2D(0, 0));
        mesh.AddVertex(new PlanarPoint3D(startPoint.X + 0.01f, startPoint.Y + 0.01f, startPoint.Z + 0.01f), new PlanarPoint2D(0, 1));

        mesh.AddVertex(new PlanarPoint3D(endPoint.X - 0.01f, endPoint.Y - 0.01f, endPoint.Z - 0.01f), new PlanarPoint2D(1, 0));
        mesh.AddVertex(new PlanarPoint3D(endPoint.X + 0.01f, endPoint.Y + 0.01f, endPoint.Z + 0.01f), new PlanarPoint2D(1, 1));
        
        mesh.AddIndex(0);
        mesh.AddIndex(1);
        mesh.AddIndex(2);
        
        mesh.AddIndex(2);
        mesh.AddIndex(3);
        mesh.AddIndex(1);
        
        mesh.GenerateBuffers(silkGlContext);
        
        mesh.BindBuffers();
        
        _blueDebugTexture.Bind();
        silkGlContext.DrawElements(PrimitiveType.Triangles, (uint)mesh.Indices.Count, DrawElementsType.UnsignedInt, null);

        mesh.Dispose();
    }
}