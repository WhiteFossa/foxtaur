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
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.UI;
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
using Foxtaur.LibResources.Models;
using Foxtaur.LibResources.Models.HighResMap;
using Foxtaur.LibSettings.Services.Abstractions;
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

    #endregion
    
    #region Locks

    private object _demRegenerationLock = new object();

    #endregion
    
    #region Debug

    private HighResMapFragment _davydovoFragment = new HighResMapFragment(54.807812457.ToRadians(),
        54.757759918.ToRadians(),
        -39.823302090.ToRadians(),
        -39.879142801.ToRadians(),
        ResourcesConstants.MapsRemotePath + @"Davydovo/Davydovo.tif.zst",
        false);

    private HighResMap _davydovoHighResMap;

    private MapSegment _davydovoMapSegment;

    private bool _isDavydovoLoaded;

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
        
        // Debug
        _davydovoHighResMap = new HighResMap(Guid.NewGuid(), "Davydovo", _davydovoFragment);
        
        var davydovoThread = new Thread(() => _davydovoFragment.Download(OnDavydovoLoad));
        davydovoThread.Start();
    }

    private void OnDavydovoLoad(FragmentedResourceBase davydovo)
    {
        // Now we have texture loaded, time to generate map segment
        _isDavydovoLoaded = true;
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
        _earthTexture = new Texture(silkGlContext, @"Resources/Textures/Basemaps/NE2_50M_SR_W.jpeg");

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
            InitiateEarthMeshesRegeneration();
            
            _isZoomLevelChanged = false;
        }
        
        // Work only with those segments
        FindVisibleEarthSegments();
        
        // Regenerating Earth segments
        RegenerateEarthSegments(silkGlContext);
        
        // Draw Earth
        DrawEarth(silkGlContext);

        // Draw maps
        if (_isDavydovoLoaded)
        {
            if (_davydovoMapSegment == null)
            {
                _davydovoMapSegment = _mapSegmentGenerator.Generate(_davydovoHighResMap, _currentMapsSurfaceAltitudeIncrement);
            }
            
            // Do we have texture?
            if (_davydovoMapSegment.Texture == null)
            {
                _davydovoMapSegment.UpdateTexture(new Texture(silkGlContext, _davydovoMapSegment.Map.Fragment.GetImage()));
            }

            if (_davydovoMapSegment.Mesh.VerticesBufferObject == null)
            {
                _davydovoMapSegment.Mesh.GenerateBuffers(silkGlContext);
            }
            
            _davydovoMapSegment.Texture.Bind();
            _davydovoMapSegment.Mesh.BindBuffers(silkGlContext);
            
            silkGlContext.DrawElements(PrimitiveType.Triangles, (uint)_davydovoMapSegment.Mesh.Indices.Count, DrawElementsType.UnsignedInt, null);
        }
        
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
        var cameraH = RendererConstants.SurfaceRunModeCameraOrbitHeight + _demProvider.GetSurfaceAltitude(_camera.Lat, _camera.Lon, _zoomService.ZoomLevel);
        //var cameraH = GeoConstants.EarthRadius + RendererConstants.SurfaceRunModeCameraOrbitHeight;
        _camera.H = cameraH;

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
        targetVector = targetVector.RotateAround(nadirVector, _surfaceRunLonViewAngle);

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
                
                _currentMapsSurfaceAltitudeIncrement = RendererConstants.MapsAltitudeIncrementSurfaceRunMode;
                _davydovoMapSegment = null;
            }
            else
            {
                _camera.H = RendererConstants.CameraOrbitHeight;
                _camera.Target = GeoConstants.EarthCenter.AsPlanarPoint3D();
                _camera.Up = Vector<double>.Build.DenseOfArray(new double[] { 0.0, -1.0, 0.0 });
                
                _currentMapsSurfaceAltitudeIncrement = RendererConstants.MapsAltitudeIncrementSatelliteMode;
                _davydovoMapSegment = null;

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

        return _sphereCoordinatesProvider.Planar3DToGeo(closestIntersection);
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
                _earthSegments
                    .Where(es => es.GeoSegment.IsCoveredBy(args.Segment))
                    .ToList()
                    .ForEach(s => s.MarkToRegeneration());    
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
            .TakeLast(RendererConstants.MaxSegmentsPerFrameRegeneration)
            .ToList();

        // Regenerating meshes
        toRegenerateInThisFrame
            .ForEach(es => _earthGenerator.GenerateMeshForSegment(es));
        
        // Regenerating buffers
        toRegenerateInThisFrame
            .ForEach(es => es.Mesh.GenerateBuffers(silkGl));
        
        // Done
        toRegenerateInThisFrame
            .ForEach(es => es.MarkAsRegenerated());
    }

    private unsafe void DrawEarth(GL silkGl)
    {
        _earthTexture.Bind();
        
        // For now we treat all segments as visible
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

            if (c1.IsPointInCullingViewport() || c2.IsPointInCullingViewport() || c3.IsPointInCullingViewport() || c4.IsPointInCullingViewport() || viewportSegment.IsCullingViewpointCoveredBySegment())
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
        InitiateEarthMeshesRegeneration();
    }

    /// <summary>
    /// Mark Earth-related meshes for regeneration
    /// </summary>
    private void InitiateEarthMeshesRegeneration()
    {
        DisposeEarthSegments();
        GenerateEarthSegments();

        _davydovoMapSegment = null;
    }
}