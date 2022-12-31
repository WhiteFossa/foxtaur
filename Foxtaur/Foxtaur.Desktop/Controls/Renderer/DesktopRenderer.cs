using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Timers;
using Avalonia;
using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Distances;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.UI;
using Foxtaur.Desktop.Controls.Renderer.Enums;
using Foxtaur.Desktop.Controls.Renderer.Models;
using Foxtaur.Helpers;
using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Helpers;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibGeo.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibGeo.Services.Abstractions.DemProviders;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Helpers;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Models.UI;
using Foxtaur.LibRenderer.Services.Abstractions.Camera;
using Foxtaur.LibRenderer.Services.Abstractions.Zoom;
using Foxtaur.LibResources.Constants;
using Foxtaur.LibResources.Enums;
using Foxtaur.LibResources.Models;
using Foxtaur.LibResources.Models.HighResMap;
using Foxtaur.LibSettings.Services.Abstractions;
using Foxtaur.LibWebClient.Models;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Silk.NET.OpenGL;
using Timer = System.Timers.Timer;

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
    private double _scaling;

    /// <summary>
    /// Viewport width
    /// </summary>
    private int _viewportWidth;

    /// <summary>
    /// Viewport height
    /// </summary>
    private int _viewportHeight;

    /// <summary>
    /// Default shader
    /// </summary>
    private Shader _defaultShader;
    
    #region Earth

    /// <summary>
    /// Earth sphere (for raycasting)
    /// </summary>
    private Sphere _earthSphere;

    /// <summary>
    /// Earth segments
    /// </summary>
    private List<EarthSegment> _earthSegments = new List<EarthSegment>();

    /// <summary>
    /// Currently visible Earth segments
    /// </summary>
    private List<EarthSegment> _visibleEarthSegments = new List<EarthSegment>();

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
    private double _pressX;

    /// <summary>
    /// Mouse Y at press moment
    /// </summary>
    private double _pressY;

    /// <summary>
    /// Mouse was pressed in this location
    /// </summary>
    private GeoPoint _pressGeoPoint;

    /// <summary>
    /// If true, then zoom level changed and we have to regenerate related stuff
    /// </summary>
    private bool _isZoomLevelChanged;
    
    #endregion

    #region Surface run

    /// <summary>
    /// Surface run mode
    /// </summary>
    private bool _isSurfaceRunMode = false;

    /// <summary>
    /// Latitudal view angle (head movement)
    /// </summary>
    private double _surfaceRunLatViewAngle = 0.0f;

    /// <summary>
    /// Latitudal view angle (head movement started)
    /// </summary>
    private double _surfaceRunLatViewAnglePress;

    /// <summary>
    /// Longitudal view angle (head movement)
    /// </summary>
    private double _surfaceRunLonViewAngle = 0.0f;

    /// <summary>
    /// Laongitudal view angle (head movement started)
    /// </summary>
    private double _surfaceRunLonViewAnglePress;

    /// <summary>
    /// If true, then we are rotating the head
    /// </summary>
    private bool _rotateHeadMode = false;

    /// <summary>
    /// Surface run mode (i.e. is user moving?)
    /// </summary>
    private SurfaceRunMode _surfaceRunMode = SurfaceRunMode.Stop;

    /// <summary>
    /// Surface run direction (radians, relative to north)
    /// </summary>
    private double _surfaceRunDirection = 0.0;

    /// <summary>
    /// Surface run rotation mode
    /// </summary>
    private SurfaceRunRotationMode _surfaceRunRotationMode = SurfaceRunRotationMode.None;
 
    #endregion
    
    #region Maps

    /// <summary>
    /// Add this altitude to all map mesh nodes (to lift map over the surface)
    /// </summary>
    private double _currentMapsSurfaceAltitudeIncrement = RendererConstants.MapsAltitudeIncrementSatelliteMode;
    
    #endregion

    #region UI

    /// <summary>
    /// UI data
    /// </summary>
    private UiData _uiData;

    #endregion

    #region FPS

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
    
    private readonly ICamera _camera = Program.Di.GetService<ICamera>();
    private ICoordinatesProvider _sphereCoordinatesProvider = Program.Di.GetService<ISphereCoordinatesProvider>();
    private IEarthGenerator _earthGenerator = Program.Di.GetService<IEarthGenerator>();
    private IUi _ui = Program.Di.GetService<IUi>();
    private IDemProvider _demProvider = Program.Di.GetService<IDemProvider>();
    private IZoomService _zoomService = Program.Di.GetService<IZoomService>();
    private IMapSegmentGenerator _mapSegmentGenerator = Program.Di.GetService<IMapSegmentGenerator>();
    private ISettingsService _settingsService = Program.Di.GetService<ISettingsService>();
    private IDistanceProvider _distanceProvider = Program.Di.GetService<IDistanceProvider>();

    #endregion
    
    #region Locks

    private object _demRegenerationLock = new object();

    #endregion

    #region Distance
    
    private bool _isDistanceRegenerationNeeded = false;

    private Distance _activeDistance;
    
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    public DesktopRenderer()
    {
        // Listening for properties changes to process resize
        PropertyChanged += OnPropertyChangedListener;
        
        // We need to regenerate meshes on DEM updates
        _demProvider.OnRegenerateDemFragment += OnDemChanged;
        
        // DEM scale change
        _settingsService.OnDemScaleChanged += OnDemScaleChanged;
    }

    private void OnPropertyChangedListener(object sender, AvaloniaPropertyChangedEventArgs e)
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
        _scaling = VisualRoot.RenderScaling; // TODO: In future we may handle scaling change

        _viewportWidth = (int)(bounds.Width * _scaling);
        _viewportHeight = (int)(bounds.Height * _scaling);

        _camera.AspectRatio = _viewportWidth / (double)_viewportHeight;

        // Marking GUI as requiring re-initialization
        _ui.IsNeedToReinitialize = true;
    }

    /// <summary>
    /// OpenGL initialization
    /// </summary>
    protected override void OnOpenGlInit(GlInterface gl, int fb)
    {
        base.OnOpenGlInit(gl, fb);

        var silkGlContext = GL.GetApi(gl.GetProcAddress);
        
        #region Initialization
        
        // Generating the Earth
        _earthSphere = _earthGenerator.GenerateEarthSphere();
        GenerateEarthSegments();

        // Creating camera
        _camera.Lat = 0.0f;
        _camera.Lon = 0.0f;
        _camera.H = RendererConstants.CameraOrbitHeight;
        _camera.Zoom = RendererConstants.CameraMinZoom;

        // Targetting camera
        _camera.Target = GeoConstants.EarthCenter.AsPlanarPoint3D();

        // UI
        _uiData = new UiData();

        // Setting-up input events
        PointerWheelChanged += OnWheel;
        PointerPressed += OnMousePressed;
        PointerReleased += OnMouseReleased;
        PointerMoved += OnMouseMoved;
        
        // Zoom events
        _camera.OnZoomChanged += OnZoomChanged;
        _zoomService.OnZoomLevelChanged += OnZoomLevelChanged;

        #endregion
        

        // Loading shaders
        _defaultShader = new Shader(silkGlContext, @"Resources/Shaders/shader.vert", @"Resources/Shaders/shader.frag");

        // Loading textures
        _earthTexture = new Texture(silkGlContext, @"Resources/Textures/Basemaps/HYP_HR_SR_OB_DR_resized.jpeg");

        // UI
        _ui.Initialize(silkGlContext, _viewportWidth, _viewportHeight, _uiData); // We also need to re-initialize on viewport size change

        _fpsTimer = new Timer(1000);
        _fpsTimer.Elapsed += OnFpsTimer;
        _fpsTimer.AutoReset = true;
        _fpsTimer.Enabled = true;
    }
    
    private void OnFpsTimer(object sender, ElapsedEventArgs e)
    {
        _uiData.Fps = _framesDrawn * (1000 / _fpsTimer.Interval);
        _framesDrawn = 0;

        _uiData.MarkForRegeneration();
    }

    /// <summary>
    /// Called when frame have to be rendered
    /// </summary>
    protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
    {
        var silkGlContext = GL.GetApi(gl.GetProcAddress);
        
        silkGlContext.ClearColor(Color.Black);
        silkGlContext.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        silkGlContext.Enable(EnableCap.DepthTest);

        // Blending
        silkGlContext.Enable(EnableCap.Blend);
        silkGlContext.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

        silkGlContext.Viewport(0, 0, (uint)_viewportWidth, (uint)_viewportHeight);

        // Surface mode camera positioning
        if (_isSurfaceRunMode)
        {
            ProcessSurfaceRunMovement();
            
            SurfaceRunPositionCamera();
        }

        _defaultShader.Use();

        // Setting shader parameters (common)
        //_defaultShader.SetUniform2f("resolution", new Vector2(_viewportWidth, _viewportHeight));

        // Setting shader parameters (vertices)
        _defaultShader.SetUniform4f("uModel", _camera.ModelMatrix.ToMatrix4x4());
        _defaultShader.SetUniform4f("uView", _camera.ViewMatrix.ToMatrix4x4());
        _defaultShader.SetUniform4f("uProjection", _camera.ProjectionMatrix.ToMatrix4x4());

        // Setting shader parameters (fragments)
        _defaultShader.SetUniform1i("ourTexture", 0);

        //silkGlContext.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

        // If zoom level changed, we have to regenerate everything Earth-related
        if (_isZoomLevelChanged)
        {
            _earthSegments.ForEach(es => es.MarkToRegeneration());
            
            _isZoomLevelChanged = false;
        }
        
        // Work only with those segments
        FindVisibleEarthSegments();
        
        // Regenerating Earth segments
        RegenerateEarthSegments(silkGlContext);
        
        // Draw Earth
        DrawEarth(silkGlContext);

        // Draw the distance
        if (_isDistanceRegenerationNeeded)
        {
            _distanceProvider.DisposeDistanceSegment();

            _isDistanceRegenerationNeeded = false;
        }

        _distanceProvider.GenerateDistanceSegment(silkGlContext, _currentMapsSurfaceAltitudeIncrement);
        _distanceProvider.DrawDistance(silkGlContext);

        // UI
        _ui.DrawUi(silkGlContext, _viewportWidth, _viewportHeight, _uiData);

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

        // Cleaning Earth segments
        DisposeEarthSegments();
        
        _earthTexture.Dispose();

        _defaultShader.Dispose();

        base.OnOpenGlDeinit(gl, fb);
    }

    private void DisposeEarthSegments()
    {
        foreach (var earthSegment in _earthSegments)
        {
            if (earthSegment.Mesh != null)
            {
                earthSegment.Mesh.Dispose();   
            }
        }
    }

    /// <summary>
    /// Position camera during surface run
    /// </summary>
    private void SurfaceRunPositionCamera()
    {
        LimitSurfaceRunViewAngles();
        
        // Camera height
        _camera.H = RendererConstants.SurfaceRunModeCameraOrbitHeight + _demProvider.GetSurfaceAltitude(_camera.Lat, _camera.Lon, _zoomService.ZoomLevel).ScaleAltitude(_settingsService.GetDemScale());

        // Up
        var nadirVector = GeoConstants.EarthCenter - _camera.Position3D.AsVector();
        _camera.Up = nadirVector;

        // Target
        var toNorthVector = _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(Math.PI / 2.0, 0, _camera.H)).AsVector() - _camera.Position3D.AsVector();
        var nadirNorthPerpVector = DoubleHelper.Cross3(nadirVector, toNorthVector); // Perpendicular to nadir vector and north vector
        
        var targetVector = nadirVector.RotateAround(nadirNorthPerpVector, Math.PI / 2.0);

        // Latitudal view
        targetVector = targetVector.RotateAround(nadirNorthPerpVector, _surfaceRunLatViewAngle);

        // Longitudal view
        targetVector = targetVector.RotateAround(nadirVector, _surfaceRunDirection); // Run direction
        targetVector = targetVector.RotateAround(nadirVector, _surfaceRunLonViewAngle); // Head direction

        _camera.Target = new PlanarPoint3D(targetVector[0] + _camera.Position3D.X, targetVector[1] + _camera.Position3D.Y, targetVector[2] + _camera.Position3D.Z);
    }

    /// <summary>
    /// Mouse wheel event
    /// </summary>
    private void OnWheel(object sender, PointerWheelEventArgs e)
    {
        var steps = Math.Abs(e.Delta.Y);

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
    private void OnMousePressed(object sender, PointerPressedEventArgs e)
    {
        var x = e.GetCurrentPoint(this).Position.X * _scaling;
        var y = e.GetCurrentPoint(this).Position.Y * _scaling;

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
                _isSurfaceRunMode = true;
                
                _camera.Zoom = RendererConstants.SurfaceRunMinZoom;
                _zoomService.IsSurfaceRunMode = true;
                
                _currentMapsSurfaceAltitudeIncrement = RendererConstants.MapsAltitudeIncrementSurfaceRunMode;
                _isDistanceRegenerationNeeded = true; // We need to regenerate because of changed altitude increment
            }
            else
            {
                _camera.H = RendererConstants.CameraOrbitHeight;
                _camera.Target = GeoConstants.EarthCenter.AsPlanarPoint3D();
                _camera.Up = Vector<double>.Build.DenseOfArray(new double[] { 0.0, -1.0, 0.0 });
                
                _currentMapsSurfaceAltitudeIncrement = RendererConstants.MapsAltitudeIncrementSatelliteMode;
                _isDistanceRegenerationNeeded = true;

                _zoomService.IsSurfaceRunMode = false;
                _isSurfaceRunMode = false;
            }

            _camera.IsSurfaceRunMode = _isSurfaceRunMode;
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
    private void OnMouseReleased(object sender, PointerReleasedEventArgs e)
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
    private void OnMouseMoved(object sender, PointerEventArgs e)
    {
        var x = e.GetCurrentPoint(this).Position.X * _scaling;
        var y = e.GetCurrentPoint(this).Position.Y * _scaling;

        var dx = x - _pressX;
        var dy = y - _pressY;

        var geoCoordinates = GetMouseGeoCoordinates(x, y);

        // Updating UI
        _uiData.IsMouseInEarth = geoCoordinates != null;
        if (_uiData.IsMouseInEarth)
        {
            _uiData.MouseLat = geoCoordinates.Lat;
            _uiData.MouseLon = geoCoordinates.Lon;
            _uiData.MouseH = geoCoordinates.H;
        }

        _uiData.MarkForRegeneration();

        if (_earthDragMode)
        {
            // Earth drag
            if (geoCoordinates != null)
            {
                var latDelta = GeoPoint.SumLatitudesWithWrap(geoCoordinates.Lat, -1.0f * _pressGeoPoint.Lat);
                var lonDelta = GeoPoint.SumLongitudesWithWrap(geoCoordinates.Lon, -1.0f * _pressGeoPoint.Lon);

                _camera.Lat = GeoPoint.SumLatitudesWithWrap(_camera.Lat, -1.0f * latDelta);
                _camera.Lon = GeoPoint.SumLongitudesWithWrap(_camera.Lon, -1.0f * lonDelta);
            }
        }

        if (_isSurfaceRunMode && _rotateHeadMode)
        {
            _surfaceRunLatViewAngle = RendererConstants.SurfaceRunHeadRotationSpeedLat * dy * _camera.Zoom + _surfaceRunLatViewAnglePress;
            _surfaceRunLonViewAngle = RendererConstants.SurfaceRunHeadRotationSpeedLon * 2.0f * dx * _camera.Zoom + _surfaceRunLonViewAnglePress; // 2.0f because lat is [-Pi; Pi], but lon is [-2 * Pi; 2 * Pi]
        }
    }

    private void LimitSurfaceRunViewAngles()
    {
        if (_surfaceRunLatViewAngle > Math.PI / 2.0)
        {
            _surfaceRunLatViewAngle = Math.PI / 2.0;
        }
        else if (_surfaceRunLatViewAngle < Math.PI / -2.0)
        {
            _surfaceRunLatViewAngle = Math.PI / -2.0;
        }

        while (_surfaceRunLonViewAngle > Math.PI)
        {
            _surfaceRunLonViewAngle -= 2.0 * Math.PI;
        }

        while (_surfaceRunLonViewAngle < -1.0 * Math.PI)
        {
            _surfaceRunLonViewAngle += 2.0 * Math.PI;
        }
    }

    /// <summary>
    /// Get mouse geographical coordinates (they might be null if mouse outside the Earth)
    /// </summary>
    private GeoPoint GetMouseGeoCoordinates(double x, double y)
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

        if (!cameraRay.IsPointOnEndSide(closestIntersection))
        {
            // Out-of-scene
            return null;
        }

        var result = _sphereCoordinatesProvider.Planar3DToGeo(closestIntersection);

        // Asking DEM provider for actual altitude
        var altitude = _demProvider.GetSurfaceAltitude(result.Lat, result.Lon, _zoomService.ZoomLevel);

        return new GeoPoint(result.Lat, result.Lon, altitude);
    }

    /// <summary>
    /// Called when DEM fragment is regenerated, we have to regenerate all related meshes after it
    /// </summary>
    private void OnDemChanged(object sender, OnRegenerateDemFragmentArgs args)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            lock (_demRegenerationLock)
            {
                var segmentsToRegenerate = _earthSegments
                    .Where(es => es.GeoSegment.IsCoveredBy(args.Segment));

                foreach (var segment in segmentsToRegenerate)
                {
                    segment.MarkToRegeneration();
                }
                
                // Marking distance's map for regeneration too
                _isDistanceRegenerationNeeded = true;
            }
        });
    }

    private void GenerateEarthSegments()
    {
        _earthSegments.Clear();
        
        for (var lat = -0.5 * Math.PI; lat < 0.5 * Math.PI; lat += _zoomService.ZoomLevelData.SegmentSize)
        {
            for (var lon = Math.PI; lon > GeoPoint.SumLongitudesWithWrap(-1.0 * Math.PI, _zoomService.ZoomLevelData.SegmentSize); lon -= _zoomService.ZoomLevelData.SegmentSize)
            {
                _earthSegments.Add(_earthGenerator.GenerateEarthSegment(
                    new GeoSegment(
                        GeoPoint.SumLatitudesWithWrap(lat, _zoomService.ZoomLevelData.SegmentSize),
                        lon,
                        lat,
                        GeoPoint.SumLongitudesWithWrap(lon, -1.0 * _zoomService.ZoomLevelData.SegmentSize))));
            }
        }
    }

    private void RegenerateEarthSegments(GL silkGl)
    {
        var toRegenerateInThisFrame = _visibleEarthSegments
            .Where(es => es.IsRegenerationNeeded)
            .TakeLast(RendererConstants.MaxSegmentsPerFrameRegeneration);

        // Regenerating meshes
        foreach (var segment in toRegenerateInThisFrame)
        {
            _earthGenerator.GenerateMeshForSegment(segment);
        }

        // Regenerating buffers
        foreach (var segment in toRegenerateInThisFrame)
        {
            segment.Mesh.GenerateBuffers(silkGl);
        }

        // Done
        foreach (var segment in toRegenerateInThisFrame)
        {
            segment.MarkAsRegenerated();
        }
    }

    private unsafe void DrawEarth(GL silkGl)
    {
        _earthTexture.Bind();
        
        foreach (var earthSegment in _visibleEarthSegments)
        {
            if (earthSegment.Mesh == null)
            {
                continue; // Mesh is not generated yet
            }
            
            earthSegment.Mesh.BindBuffers(silkGl);
            silkGl.DrawElements(PrimitiveType.Triangles, (uint)earthSegment.Mesh.Indices.Count, DrawElementsType.UnsignedInt, null);   
        }
    }

    private void FindVisibleEarthSegments()
    {
        _visibleEarthSegments.Clear();

        var undergroundPoint = _isSurfaceRunMode
            ? _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(_camera.Lat, _camera.Lon, RendererConstants.SurfaceRunModeUndergroundPlaneHeight)) 
            : new PlanarPoint3D(GeoConstants.EarthCenter[0], GeoConstants.EarthCenter[1], GeoConstants.EarthCenter[2]);
        
        foreach (var earthSegment in _earthSegments)
        {
            var viewportSegment = _camera.ProjectSegmentToViewport(earthSegment.GeoSegment);
            
            // Segment corners
            var c1 = new PlanarPoint2D(viewportSegment.Left, viewportSegment.Bottom);
            var c2 = new PlanarPoint2D(viewportSegment.Left, viewportSegment.Top);
            var c3 = new PlanarPoint2D(viewportSegment.Right, viewportSegment.Top);
            var c4 = new PlanarPoint2D(viewportSegment.Right, viewportSegment.Bottom);

            var isC1InViewport = c1.IsPointInCullingViewport();
            var isC2InViewport = c2.IsPointInCullingViewport();
            var isC3InViewport = c3.IsPointInCullingViewport();
            var isC4InViewport = c4.IsPointInCullingViewport();

            var isViewportCoveredBySegment = viewportSegment.IsCullingViewpointCoveredBySegment();

            if (isC1InViewport || isC2InViewport || isC3InViewport || isC4InViewport || isViewportCoveredBySegment)
            {
                // Removing far-side segments
                var p1 = _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(earthSegment.GeoSegment.SouthLat, earthSegment.GeoSegment.WestLon, GeoConstants.EarthRadius));
                var p2 = _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(earthSegment.GeoSegment.NorthLat, earthSegment.GeoSegment.WestLon, GeoConstants.EarthRadius));
                var p3 = _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(earthSegment.GeoSegment.NorthLat, earthSegment.GeoSegment.EastLon, GeoConstants.EarthRadius));
                var p4 = _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(earthSegment.GeoSegment.SouthLat, earthSegment.GeoSegment.EastLon, GeoConstants.EarthRadius));

                if (_camera.IsPointOnCameraSideOfEarth(undergroundPoint, p1)
                    || _camera.IsPointOnCameraSideOfEarth(undergroundPoint, p2)
                    || _camera.IsPointOnCameraSideOfEarth(undergroundPoint, p3)
                    || _camera.IsPointOnCameraSideOfEarth(undergroundPoint, p4))
                {
                    _visibleEarthSegments.Add(earthSegment);    
                }
            }
        }
    }

    /// <summary>
    /// Called when camera zoom changed
    /// </summary>
    private void OnZoomChanged(object sender, OnZoomChangedArgs args)
    {
        _zoomService.Zoom = args.Zoom;
    }
    
    /// <summary>
    /// Called when zoom level (i.e. segments sizes, DEM resolution and so on) changed
    /// </summary>
    private void OnZoomLevelChanged(object sender, OnZoomLevelChangedArgs args)
    {
        _isZoomLevelChanged = true;
    }

    private void OnDemScaleChanged(object sender, ISettingsService.OnDemScaleChangedArgs args)
    {
        _earthSegments
            .ForEach(es => es.MarkToRegeneration());
        
        _isDistanceRegenerationNeeded = true;
    }

    public void OnKeyPressed(KeyEventArgs e)
    {
        // Surface run direction
        if (e.Key == _settingsService.GetSurfaceRunTurnRightButton())
        {
            _surfaceRunRotationMode = SurfaceRunRotationMode.Right;
        }
        
        if (e.Key == _settingsService.GetSurfaceRunTurnLeftButton())
        {
            _surfaceRunRotationMode = SurfaceRunRotationMode.Left;
        }
        
        if (e.Key == _settingsService.GetSurfaceRunForwardButton())
        {
            // Forward
            _surfaceRunMode = SurfaceRunMode.Forward;
        }
        else if (e.Key == _settingsService.GetSurfaceRunBackwardButton())
        {
            // Backward
            _surfaceRunMode = SurfaceRunMode.Backward;
        }
    }

    public void OnKeyReleased(KeyEventArgs e)
    {
        if (e.Key == _settingsService.GetSurfaceRunTurnRightButton() || e.Key == _settingsService.GetSurfaceRunTurnLeftButton())
        {
            _surfaceRunRotationMode = SurfaceRunRotationMode.None;
        }

        if (e.Key == _settingsService.GetSurfaceRunForwardButton() || e.Key == _settingsService.GetSurfaceRunBackwardButton())
        {
            _surfaceRunMode = SurfaceRunMode.Stop;            
        }
    }
    
    private void ProcessSurfaceRunMovement()
    {
        // Rotation (can happen even if we are stopped)
        switch (_surfaceRunRotationMode)
        {
            case SurfaceRunRotationMode.Left:
                _surfaceRunDirection = _surfaceRunDirection.AddAngleWithWrap(_settingsService.GetSurfaceRunTurnSpeed().ToRadians());
                break;
            
            case SurfaceRunRotationMode.Right:
                _surfaceRunDirection = _surfaceRunDirection.AddAngleWithWrap(-1.0 * _settingsService.GetSurfaceRunTurnSpeed().ToRadians());
                break;
            
            case SurfaceRunRotationMode.None:
                break;
            
            default:
                throw new ArgumentException(nameof(_surfaceRunRotationMode));
        }
        
        // Running
        if (_surfaceRunMode == SurfaceRunMode.Stop)
        {
            return;
        }
        
        // Under camera point
        var cameraPoint = _camera.Position3D.AsVector();
        
        // North vector
        var toNorthVector = (_sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(Math.PI / 2.0, 0, _camera.H)).AsVector() - _camera.Position3D.AsVector());
            
        // Target vector (will be added to current coordinates)
        var targetVector = toNorthVector.Normalize() * _settingsService.GetSurfaceRunSpeed();
        
        // Rotating (to be able to move not only to the North)
        var nadirVector = GeoConstants.EarthCenter - cameraPoint;
        targetVector = targetVector.RotateAround(nadirVector, _surfaceRunDirection);
        
        // Moving camera
        Vector<double> newCameraPosition3D;

        if (_surfaceRunMode == SurfaceRunMode.Forward)
        {
            newCameraPosition3D = cameraPoint + targetVector;
        }
        else if (_surfaceRunMode == SurfaceRunMode.Backward)
        {
            newCameraPosition3D = cameraPoint - targetVector;
        }
        else
        {
            throw new ArgumentException("Incorrect surface run state.");
        }
        
        var newCameraPositionGeo = _sphereCoordinatesProvider.Planar3DToGeo(newCameraPosition3D.AsPlanarPoint3D());

        _camera.Lat = newCameraPositionGeo.Lat;
        _camera.Lon = newCameraPositionGeo.Lon;
    }

    /// <summary>
    /// Set active distance
    /// </summary>
    public void SetActiveDistance(Distance distance)
    {
        _activeDistance = distance ?? throw new ArgumentNullException(nameof(distance));
        
        _isDistanceRegenerationNeeded= true;
        _distanceProvider.SetActiveDistance(_activeDistance);
    }

    /// <summary>
    /// Move camera to the center of distance (if we have active distance of course)
    /// We believe that distances are small and don't pass over 180 lon
    /// TODO: Fix it
    /// </summary>
    public void FocusOnDistance()
    {
        if (_activeDistance == null)
        {
            return;
        }

        var latCenter = (_activeDistance.Map.NorthLat + _activeDistance.Map.SouthLat) / 2.0;
        var lonCenter = (_activeDistance.Map.EastLon + _activeDistance.Map.WestLon) / 2.0;

        _camera.Lat = latCenter;
        _camera.Lon = lonCenter;
    }
}