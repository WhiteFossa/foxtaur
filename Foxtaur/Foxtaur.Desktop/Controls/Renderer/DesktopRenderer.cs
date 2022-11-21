using System;
using System.Diagnostics;
using System.Numerics;
using System.Timers;
using Avalonia;
using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.UI;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Helpers;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Models.UI;
using Foxtaur.LibRenderer.Services.Abstractions.Camera;
using Foxtaur.LibRenderer.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibRenderer.Services.Abstractions.Drawers;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Silk.NET.OpenGL;
using Color = System.Drawing.Color;

namespace Foxtaur.Desktop.Controls.Renderer;

public class DesktopRenderer : OpenGlControlBase
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
    private int _viewportWidth;

    /// <summary>
    /// Viewport height
    /// </summary>
    private int _viewportHeight;

    /// <summary>
    /// Silk.NET OpenGL context
    /// </summary>
    private GL _silkGlContext;

    /// <summary>
    /// Default shader
    /// </summary>
    private Shader _defaultShader;

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

    #region Camera

    /// <summary>
    /// True if we are dragging the Earth by mouse
    /// </summary>
    private bool _earthDragMode = false;

    /// <summary>
    /// Mouse X at press moment
    /// </summary>
    private float _pressX;

    /// <summary>
    /// Mouse Y at press moment
    /// </summary>
    private float _pressY;

    /// <summary>
    /// Mouse was pressed in this location
    /// </summary>
    private GeoPoint _pressGeoPoint;

    #endregion

    #region Surface run

    /// <summary>
    /// Surface run mode
    /// </summary>
    private bool _isSurfaceRunMode = false;

    /// <summary>
    /// Latitudal view angle (head movement)
    /// </summary>
    private float _surfaceRunLatViewAngle = 0.0f;

    /// <summary>
    /// Latitudal view angle (head movement started)
    /// </summary>
    private float _surfaceRunLatViewAnglePress;

    /// <summary>
    /// Longitudal view angle (head movement)
    /// </summary>
    private float _surfaceRunLonViewAngle = 0.0f;

    /// <summary>
    /// Laongitudal view angle (head movement started)
    /// </summary>
    private float _surfaceRunLonViewAnglePress;

    /// <summary>
    /// If true, then we are rotating the head
    /// </summary>
    private bool _rotateHeadMode = false;

    #endregion

    #region UI

    /// <summary>
    /// UI data
    /// </summary>
    private UiData _uiData;
    
    #endregion

    #region Debug

    /// <summary>
    /// FPS timer
    /// </summary>
    private Timer _fpsTimer;

    /// <summary>
    /// Times drawn since last OnFpsTimer
    /// </summary>
    private int _framesDrawn = 0;
    
    #endregion

    #region DI

    private ICoordinatesProvider _sphereCoordinatesProvider = Program.Di.GetService<ISphereCoordinatesProvider>();
    private IEarthGenerator _earthGenerator = Program.Di.GetService<IEarthGenerator>();
    private IRectangleGenerator _rectangleGenerator = Program.Di.GetService<IRectangleGenerator>();
    private ITextDrawer _textDrawer = Program.Di.GetService<ITextDrawer>();
    private IUi _ui = Program.Di.GetService<IUi>();

    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    public DesktopRenderer()
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
        _camera.Target = RendererConstants.EarthCenter.AsPlanarPoint3D();

        // UI
        _uiData = new UiData();

        // Listening for properties changes to process resize
        PropertyChanged += OnPropertyChangedListener;

        // Setting-up input events
        PointerWheelChanged += OnWheel;
        PointerPressed += OnMousePressed;
        PointerReleased += OnMouseReleased;
        PointerMoved += OnMouseMoved;
    }

    private void OnPropertyChangedListener(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name.Equals("Bounds"))
        {
            // Resize event
            OnResize((Rect)e.NewValue);
        }
    }

    /// <summary>
    /// Called when control resized
    /// </summary>
    private void OnResize(Rect bounds)
    {
        _scaling = (float)VisualRoot.RenderScaling; // TODO: In future we may handle scaling change
        
        _viewportWidth = (int)(bounds.Width * _scaling);
        _viewportHeight = (int)(bounds.Height * _scaling);
        
        _camera.AspectRatio = _viewportWidth / (float)_viewportHeight;
        
        // Marking GUI as requiring re-initialization
        _ui.IsNeedToReinitialize = true;
    }

    /// <summary>
    /// OpenGL initialization
    /// </summary>
    protected unsafe override void OnOpenGlInit(GlInterface gl, int fb)
    {
        base.OnOpenGlInit(gl, fb);

        _silkGlContext = GL.GetApi(gl.GetProcAddress);

        // Generating buffers
        _earthMesh.GenerateBuffers(_silkGlContext);

        // Loading shaders
        _defaultShader = new Shader(_silkGlContext, @"Resources/Shaders/shader.vert", @"Resources/Shaders/shader.frag");

        // Loading textures
        _earthTexture = new Texture(_silkGlContext, @"Resources/Textures/Basemaps/NE2_50M_SR_W.jpeg");
        //_earthTexture = new Texture(_silkGLContext, @"Resources/Textures/davydovo.png");
        
        // UI
        _ui.Initialize(_silkGlContext, _viewportWidth, _viewportHeight, _uiData); // We also need to re-initialize on viewport size change
        
        _fpsTimer = new Timer(1000);
        _fpsTimer.Elapsed += OnFpsTimer;
        _fpsTimer.AutoReset = true;
        _fpsTimer.Enabled = true;
    }

    private void OnFpsTimer(object? sender, ElapsedEventArgs e)
    {
        _uiData.Fps = _framesDrawn * (1000 / (float)_fpsTimer.Interval);
        _framesDrawn = 0;

        _uiData.MarkForRegeneration();
    }

    /// <summary>
    /// Called when frame have to be rendered
    /// </summary>
    protected unsafe override void OnOpenGlRender(GlInterface gl, int fb)
    {
        _silkGlContext.ClearColor(Color.Black);
        _silkGlContext.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        _silkGlContext.Enable(EnableCap.DepthTest);
        
        // Blending
        _silkGlContext.Enable(EnableCap.Blend);
        _silkGlContext.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

        _silkGlContext.Viewport(0, 0, (uint)_viewportWidth, (uint)_viewportHeight);

        // Surface mode camera positioning
        if (_isSurfaceRunMode)
        {
            SurfaceRunPositionCamera();
        }

        _defaultShader.Use();

        // Setting shader parameters (common)
        _defaultShader.SetUniform2f("resolution", new Vector2(_viewportWidth, _viewportHeight));

        // Setting shader parameters (vertices)
        _defaultShader.SetUniform4f("uModel", _camera.ModelMatrix);
        _defaultShader.SetUniform4f("uView", _camera.ViewMatrix);
        _defaultShader.SetUniform4f("uProjection", _camera.ProjectionMatrix);

        // Setting shader parameters (fragments)
        _defaultShader.SetUniform1i("ourTexture", 0);

        //_silkGLContext.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

        // Earth
        _earthMesh.BindBuffers(_silkGlContext);
        _earthTexture.Bind();
        _silkGlContext.DrawElements(PrimitiveType.Triangles, (uint)_earthMesh.Indices.Count, DrawElementsType.UnsignedInt, null);
        
        // UI
        _ui.DrawUi(_silkGlContext, _viewportWidth, _viewportHeight, _uiData);
        
        // Everything is drawn
        _framesDrawn++;
        
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    /// <summary>
    /// OpenGL de-initialization
    /// </summary>
    protected override void OnOpenGlDeinit(GlInterface gl, int fb)
    {
        _ui.DeInitialize();
        
        _earthMesh.Dispose();
        _earthTexture.Dispose();

        _defaultShader.Dispose();

        base.OnOpenGlDeinit(gl, fb);
    }

    /// <summary>
    /// Position camera during surface run
    /// </summary>
    private void SurfaceRunPositionCamera()
    {
        LimitSurfaceRunViewAngles();

        // Up
        var nadirVector = RendererConstants.EarthCenter - _camera.Position3D.AsVector3();
        _camera.Up = nadirVector;

        // Target
        var toNorthVector = _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint((float)Math.PI / 2.0f, 0, RendererConstants.EarthRadius)).AsVector3() - _camera.Position3D.AsVector3();
        var nadirNorthPerpVector = Vector3.Cross(nadirVector, toNorthVector); // Perpendicular to nadir vector and north vector

        var targetVector = nadirVector.RotateAround(nadirNorthPerpVector, (float)Math.PI / 2.0f);

        // Latitudal view
        targetVector = targetVector.RotateAround(nadirNorthPerpVector, _surfaceRunLatViewAngle);

        // Longitudal view
        targetVector = targetVector.RotateAround(nadirVector, _surfaceRunLonViewAngle);

        _camera.Target = new PlanarPoint3D(targetVector.X + _camera.Position3D.X, targetVector.Y + _camera.Position3D.Y, targetVector.Z + _camera.Position3D.Z);
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
        var x = (float)e.GetCurrentPoint(this).Position.X * _scaling;
        var y = (float)e.GetCurrentPoint(this).Position.Y * _scaling;

        _pressX = x;
        _pressY = y;

        var pressGeoPoint = GetMouseGeoCoordinates(x, y);
        if (pressGeoPoint != null)
        {
            _pressGeoPoint = pressGeoPoint;
        }

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            // Left click - dragging the camera
            if (pressGeoPoint != null)
            {
                _earthDragMode = true;
            }
        }

        if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
        {
            // Middle click - surface mode
            if (!_isSurfaceRunMode)
            {
                _camera.H = RendererConstants.SurfaceModeCameraOrbitHeight;

                _isSurfaceRunMode = true;
            }
            else
            {
                _camera.H = RendererConstants.CameraOrbitHeight;
                _camera.Target = RendererConstants.EarthCenter.AsPlanarPoint3D();
                _camera.Up = new Vector3(0.0f, -1.0f, 0.0f);

                _isSurfaceRunMode = false;
            }
        }

        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            // Right click - head rotation mode (only in surface mode)
            if (_isSurfaceRunMode)
            {
                _surfaceRunLatViewAnglePress = _surfaceRunLatViewAngle;
                _surfaceRunLonViewAnglePress = _surfaceRunLonViewAngle;
                _rotateHeadMode = true;
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
            _earthDragMode = false;
        }

        if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            _rotateHeadMode = false;
        }
    }

    /// <summary>
    /// Mouse move event
    /// </summary>
    private void OnMouseMoved(object? sender, PointerEventArgs e)
    {
        var x = (float)e.GetCurrentPoint(this).Position.X * _scaling;
        var y = (float)e.GetCurrentPoint(this).Position.Y * _scaling;

        var dx = x - _pressX;
        var dy = y - _pressY;
        
        var geoCoordinates = GetMouseGeoCoordinates(x, y);
        
        // Updating UI
        _uiData.IsMouseInEarth = geoCoordinates != null;
        if (_uiData.IsMouseInEarth)
        {
            _uiData.MouseLat = geoCoordinates.Lat;
            _uiData.MouseLon = geoCoordinates.Lon;
        }
        
        _uiData.MarkForRegeneration();
        
        if (_earthDragMode)
        {
            // Earth drag
            if (geoCoordinates != null)
            {
                var latDelta = GeoPoint.SumLatitudesWithWrap(geoCoordinates.Lat, -1.0f * _pressGeoPoint.Lat);
                var lonDelta = -1.0f * GeoPoint.SumLongitudesWithWrap(geoCoordinates.Lon, -1.0f * _pressGeoPoint.Lon);

                _camera.Lat = GeoPoint.SumLatitudesWithWrap(_camera.Lat, -1.0f * latDelta);
                _camera.Lon = GeoPoint.SumLongitudesWithWrap(_camera.Lon, -1.0f * lonDelta);
            }
        }

        if (_isSurfaceRunMode && _rotateHeadMode)
        {
            _surfaceRunLatViewAngle = RendererConstants.SurfaceRunHeadRotationSpeedLat * dy + _surfaceRunLatViewAnglePress;
            _surfaceRunLonViewAngle = RendererConstants.SurfaceRunHeadRotationSpeedLon * 2.0f * dx + _surfaceRunLonViewAnglePress; // 2.0f because lat is [-Pi; Pi], but lon is [-2 * Pi; 2 * Pi]
        }
    }

    private void LimitSurfaceRunViewAngles()
    {
        if (_surfaceRunLatViewAngle > (float)Math.PI / 2.0f)
        {
            _surfaceRunLatViewAngle = (float)Math.PI / 2.0f;
        }
        else if (_surfaceRunLatViewAngle < (float)Math.PI / -2.0f)
        {
            _surfaceRunLatViewAngle = (float)Math.PI / -2.0f;
        }

        while (_surfaceRunLonViewAngle > (float)Math.PI)
        {
            _surfaceRunLonViewAngle -= 2.0f * (float)Math.PI;
        }

        while (_surfaceRunLonViewAngle < -1.0f * (float)Math.PI)
        {
            _surfaceRunLonViewAngle += 2.0f * (float)Math.PI;
        }
    }

    /// <summary>
    /// Get mouse geographical coordinates (they might be null if mouse outside the Earth)
    /// </summary>
    private GeoPoint GetMouseGeoCoordinates(float x, float y)
    {
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
}