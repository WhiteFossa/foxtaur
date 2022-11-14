using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Foxtaur.Desktop.Controls.Renderer.Abstractions;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Services.Abstractions.CoordinateProviders;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Silk.NET.OpenGL;
using System;
using System.Drawing;
using System.Numerics;
using Foxtaur.LibRenderer.Helpers;
using Foxtaur.LibRenderer.Models;

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
    /// Camera
    /// </summary>
    private readonly Camera _camera;

    /// <summary>
    /// Camera position
    /// </summary>
    private Vector3 _cameraPosition;

    /// <summary>
    /// Camera target
    /// </summary>
    private Vector3 _cameraTarget;

    /// <summary>
    /// Camera direction
    /// </summary>
    private Vector3 _cameraDirection;

    /// <summary>
    /// Camera up direction
    /// </summary>
    private Vector3 _cameraUp;

    /// <summary>
    /// Model matrix
    /// </summary>
    private Matrix4x4 _modelMatrix;

    /// <summary>
    /// View matrix
    /// </summary>
    private Matrix4x4 _viewMatrix;

    /// <summary>
    /// Projection matrix
    /// </summary>
    private Matrix4x4 _projectionMatrix;

    /// <summary>
    /// Earth
    /// </summary>
    private Mesh _earthMesh;
    private Texture _earthTexture;

    private Texture _redDebugTexture;

    /// <summary>
    /// True if mouse left button pressed and not released yet
    /// </summary>
    private bool _isMouseLeftButtonPressed = false;

    private PlanarPoint3D _rayStart;
    private PlanarPoint3D _rayEnd;
    
    // DI stuff
    private ICoordinatesProvider _sphereCoordinatesProvider = Program.Di.GetService<ISphereCoordinatesProvider>();
    private IEarthGenerator _earthGenerator = Program.Di.GetService<IEarthGenerator>();

    /// <summary>
    /// Static constructor
    /// </summary>
    static DesktopRendererControl()
    {
        // Change of this fields require re-rendering
        //AffectsRender<DesktopRendererControl>(YawProperty, PitchProperty, RollProperty, DiscoProperty);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public DesktopRendererControl()
    {
        // Generating the Earth
        _earthMesh = _earthGenerator.GenerateFullEarth((float)Math.PI / 90.0f);
        
        // Creating camera
        _camera = new Camera
        {
            Lat = 0.0f,
            Lon = -0.5f,
            H = RendererConstants.CameraOrbitHeight,
            Zoom = RendererConstants.CameraMaxZoom
        };

        // Targetting camera
        _cameraTarget = new Vector3(0.0f, 0.0f, 0.0f);
        _cameraUp = new Vector3(0.0f, -1.0f, 0.0f);

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

        _silkGLContext.Viewport(0, 0, (uint)_viewportWidth, (uint)_viewportHeight); // TODO: Move me to constants

        _shader.Use();

        // Projections
        _cameraPosition = _camera.GetCameraPositionAsVector();
        _cameraDirection = Vector3.Normalize(_cameraPosition - _cameraTarget);

        _modelMatrix = Matrix4x4.CreateRotationZ(0.0f) * Matrix4x4.CreateRotationY(0.0f) * Matrix4x4.CreateRotationX(0.0f); // Rotation
        _viewMatrix = Matrix4x4.CreateLookAt(_cameraPosition, _cameraDirection, _cameraUp); // Camera position
        _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(_camera.Zoom, _aspectRatio, 0.1f, 100.0f); // Zoom

        // Setting shader parameters (common)
        _shader.SetUniform2f("resolution", new Vector2(_viewportWidth, _viewportHeight));

        // Setting shader parameters (vertices)
        _shader.SetUniform4f("uModel", _modelMatrix);
        _shader.SetUniform4f("uView", _viewMatrix);
        _shader.SetUniform4f("uProjection", _projectionMatrix);

        // Setting shader parameters (fragments)
        _shader.SetUniform1i("ourTexture", 0);
        _earthTexture.Bind();

        //_silkGLContext.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

        // Earth
        _earthMesh.GenerateBuffers(_silkGLContext);
        _earthMesh.BindBuffers();
        _silkGLContext.DrawElements(PrimitiveType.Triangles, (uint)_earthMesh.Indices.Count, DrawElementsType.UnsignedInt, null);
        _earthMesh.Dispose();

        // Debug vector
        if (_rayStart != null && _rayEnd != null)
        {
            DrawDebugVector(_silkGLContext, _rayStart, _rayEnd);
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

        _aspectRatio = _viewportWidth / _viewportHeight;
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

        var ray = CastRayFromScreen(x, y);

        _rayStart = ray.Item1;
        _rayEnd = ray.Item2;
    }

    /// <summary>
    /// Cast ray from the screen
    /// </summary>
    private Tuple<PlanarPoint3D, PlanarPoint3D> CastRayFromScreen(float x, float y)
    {
        var viewProjection = _viewMatrix * _projectionMatrix;
        Matrix4x4 invertedViewProjection;
        if (!Matrix4x4.Invert(viewProjection, out invertedViewProjection))
        {
            throw new InvalidOperationException("Failed to invert view projection!");
        }

        var normalizedDeviceX = x / _viewportWidth * 2.0f - 1.0f;
        var normalizedDeviceY = y / _viewportHeight * 2.0f - 1.0f;
        
        var nearPoint = invertedViewProjection.TransformPerspectively(new Vector3(normalizedDeviceX, normalizedDeviceY, 0.0f));
        var farPoint = invertedViewProjection.TransformPerspectively(new Vector3(normalizedDeviceX, normalizedDeviceY, 1.0f));

        return new Tuple<PlanarPoint3D, PlanarPoint3D>(nearPoint.ToPlanarPoint3D(), farPoint.ToPlanarPoint3D());
    }

    /// <summary>
    /// Draw debug vector
    /// </summary>
    private unsafe void DrawDebugVector(GL silkGlContext, PlanarPoint3D startPoint, PlanarPoint3D endPoint)
    {
        var mesh = new Mesh();
        mesh.AddVertex(new PlanarPoint3D(startPoint.X - 0.1f, startPoint.Y, startPoint.Z), new PlanarPoint2D(0, 0));
        mesh.AddVertex(new PlanarPoint3D(startPoint.X + 0.1f, startPoint.Y, startPoint.Z), new PlanarPoint2D(0, 1));
        
        mesh.AddVertex(new PlanarPoint3D(endPoint.X - 0.1f, endPoint.Y, endPoint.Z), new PlanarPoint2D(1, 0));
        mesh.AddVertex(new PlanarPoint3D(endPoint.X + 0.1f, endPoint.Y, endPoint.Z), new PlanarPoint2D(1, 1));
        
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