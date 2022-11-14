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
        // Creating camera
        _camera = new Camera
        {
            Lat = 0.0f,
            Lon = -0.5f,
            H = RendererConstants.CameraOrbitHeight,
            Zoom = RendererConstants.CameraMaxZoom
        };

        // Generating the Earth
        _earthMesh = _earthGenerator.GenerateFullEarth((float)Math.PI / 90.0f);

        // Setting-up input events
        PointerWheelChanged += OnWheel;
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

        // Earth
        _earthMesh.GenerateBuffers(_silkGLContext);

        // Loading texture
        _textureObject = new Texture(_silkGLContext, @"Resources/Textures/Basemaps/HYP_50M_SR_W_SMALL.jpeg");
        //_textureObject = new Texture(_silkGLContext, @"Resources/davydovo.png");
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
        Vector3 cameraPosition = _camera.GetCameraPositionAsVector();
        var cameraTarget = new Vector3(0.0f, 0.0f, 0.0f);
        var cameraDirection = Vector3.Normalize(cameraPosition - cameraTarget);

        var cameraUp = new Vector3(0.0f, -1.0f, 0.0f);

        var model = Matrix4x4.CreateRotationZ(0.0f) * Matrix4x4.CreateRotationY(0.0f) * Matrix4x4.CreateRotationX(0.0f); // Rotation
        var view = Matrix4x4.CreateLookAt(cameraPosition, cameraDirection, cameraUp); // Camera position
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(_camera.Zoom, _aspectRatio, 0.1f, 100.0f); // Zoom

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
        _silkGLContext.DrawElements(PrimitiveType.Triangles, (uint)_earthMesh.Indices.Count,
            DrawElementsType.UnsignedInt, null);

        // Rotate camera (debug)
        _camera.Lon += (float)Math.PI / 200.0f;
        if (_camera.Lon > Math.PI)
        {
            _camera.Lon -= 2.0f * (float)Math.PI;
        }

        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    /// <summary>
    /// OpenGL de-initialization
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

    /// <summary>
    /// Mouse wheel event
    /// </summary>
    private void OnWheel(object sender, PointerWheelEventArgs e)
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

    private void OnMouseMoved(object sender, PointerEventArgs e)
    {
    }
}