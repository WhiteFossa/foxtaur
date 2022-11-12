using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using NLog;
using static Avalonia.OpenGL.GlConsts;

namespace Foxtaur.Desktop.Controls.Renderer;

public partial class DesktopRenderer : UserControl
{
    public DesktopRenderer()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

public class DesktopRendererControl : OpenGlControlBase
{
    /// <summary>
    /// Vertexes positions
    /// </summary>
    private const int VertexesPositionLocation = 0;

    /// <summary>
    /// Vertex size in bytes
    /// </summary>
    private readonly int _vertexSize;

    private Logger _logger = LogManager.GetCurrentClassLogger();
    
    private int _vertexBufferObject;
    private int _vertexArrayObject;
    
    private readonly Vertex[] _points;

    private int _defaultVertexShader;
    private int _defaultFragmentShader;
    private int _defaultShaderProgram;
    
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
        _vertexSize = Marshal.SizeOf<Vertex>();
        
        // Loading triangle
        _points = new Vertex[3];
        _points[0] = new Vertex() { Position = new Vector3(-0.5f, -0.5f, 0.0f) };
        _points[1] = new Vertex() { Position = new Vector3(0.5f, -0.5f, 0.0f) };
        _points[2] = new Vertex() { Position = new Vector3(0.0f, 0.5f, 0.0f) };
    }

    /// <summary>
    /// OpenGL initialization
    /// </summary>
    protected unsafe override void OnOpenGlInit(GlInterface GL, int fb)
    {
        CheckAndLogOpenGLErrors(GL);

        _logger.Info($"Renderer: { GL.GetString(GL_RENDERER) } Version: { GL.GetString(GL_VERSION) }");
        
        // Load the source of the vertex shader and compile it
        _defaultVertexShader = GL.CreateShader(GL_VERTEX_SHADER);
        _logger.Info(GL.CompileShaderAndGetError(_defaultVertexShader, Shaders.DefaultVertexesShader));

        // Load the source of the fragment shader and compile it
        _defaultFragmentShader = GL.CreateShader(GL_FRAGMENT_SHADER);
        _logger.Info(GL.CompileShaderAndGetError(_defaultFragmentShader, Shaders.DefaultFragmentsShader));
        
        // Create the shader program, attach the vertex and fragment shaders and link the program.
        _defaultShaderProgram = GL.CreateProgram();
        GL.AttachShader(_defaultShaderProgram, _defaultVertexShader);
        GL.AttachShader(_defaultShaderProgram, _defaultFragmentShader);
        
        GL.BindAttribLocationString(_defaultShaderProgram, VertexesPositionLocation, "vPos");
        _logger.Info(GL.LinkProgramAndGetError(_defaultShaderProgram));
        CheckAndLogOpenGLErrors(GL);
        
        // Object for vertices
        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
        CheckAndLogOpenGLErrors(GL);
        
        // Loading verices
        fixed (void* pdata = _points)
        {
            GL.BufferData(GL_ARRAY_BUFFER, new IntPtr(_points.Length * _vertexSize), new IntPtr(pdata), GL_STATIC_DRAW);
        }

        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);

        GL.VertexAttribPointer(VertexesPositionLocation, 3, GL_FLOAT, 0, _vertexSize, IntPtr.Zero);
        GL.EnableVertexAttribArray(VertexesPositionLocation);
        CheckAndLogOpenGLErrors(GL);
    }
    
    /// <summary>
    /// Called when frame have to be rendered
    /// </summary>
    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        unsafe
        {
            // Preparing scene
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            gl.Enable(GL_DEPTH_TEST);
            gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
            
            gl.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
            gl.BindVertexArray(_vertexArrayObject);
            gl.UseProgram(_defaultShaderProgram);
            CheckAndLogOpenGLErrors(gl);

            gl.DrawArrays(GL_TRIANGLES, 0, new IntPtr(3));
            CheckAndLogOpenGLErrors(gl);
        }
    }

    /// <summary>
    /// OpenGL de-initialization
    /// </summary>
    protected override void OnOpenGlDeinit(GlInterface GL, int fb)
    {
        // Unbind everything
        GL.BindBuffer(GL_ARRAY_BUFFER, 0);
        GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);

        // Delete all resources.
        GL.DeleteBuffer(_vertexBufferObject);
        GL.DeleteProgram(_defaultShaderProgram);
        GL.DeleteShader(_defaultFragmentShader);
        GL.DeleteShader(_defaultVertexShader);
    }
    
    /// <summary>
    /// Check and log OpenGL errors
    /// </summary>
    private void CheckAndLogOpenGLErrors(GlInterface gl)
    {
        int error;
        while ((error = gl.GetError()) != GlConsts.GL_NO_ERROR)
        {
            _logger.Error($"OpenGL error: { error }");   
        }
    }
}