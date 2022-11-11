using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using NLog;
using static Avalonia.OpenGL.GlConsts;

namespace Foxtaur.Desktop.Controls;

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
    /// OpenGL Vertex
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
    }

    /// <summary>
    /// Vertex size in bytes
    /// </summary>
    private readonly int _vertexSize;

    private Logger _logger = LogManager.GetCurrentClassLogger();
    
    private int _vertexBufferObject;
    private int _indexBufferObject;
    private int _vertexArrayObject;
    
    private readonly Vertex[] _points;
    private readonly ushort[] _indices;
    private readonly float _minY;
    private readonly float _maxY;
    
    
    private int _vertexShader;
    private int _fragmentShader;
    private int _shaderProgram;
    
    private string VertexShaderSource => GetShader(false, @"
    attribute vec3 aPos;
    attribute vec3 aNormal;
    uniform mat4 uModel;
    uniform mat4 uProjection;
    uniform mat4 uView;

    varying vec3 Normal;

    void main()
    {
        gl_Position = uProjection * uView * uModel * vec4(aPos, 1.0);
        Normal = normalize(vec3(uModel * vec4(aNormal, 1.0)));
    }
");

    private string FragmentShaderSource = @"#version 330 core
out vec4 color;
void main(){
  color = vec4(0, 0, 1, 1);
}";
    
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
        _points[0] = new Vertex() { Position = new Vector3(-10.0f, -10.0f, 1.0f), Normal = new Vector3(0.0f, 0.0f, 0.0f) };
        _points[1] = new Vertex() { Position = new Vector3(10.0f, -10.0f, 1.0f), Normal = new Vector3(0.0f, 0.0f, 0.0f) };
        _points[2] = new Vertex() { Position = new Vector3(0.0f, 10.0f, 1.0f), Normal = new Vector3(0.0f, 0.0f, 0.0f) };

        _indices = new ushort[3] { 0, 1, 2 };
        
        for (int i = 0; i < _indices.Length; i += 3)
        {
            Vector3 a = _points[_indices[i]].Position;
            Vector3 b = _points[_indices[i + 1]].Position;
            Vector3 c = _points[_indices[i + 2]].Position;
            var normal = Vector3.Normalize(Vector3.Cross(c - b, a - b));

            _points[_indices[i]].Normal += normal;
            _points[_indices[i + 1]].Normal += normal;
            _points[_indices[i + 2]].Normal += normal;
        }
        
        for (int i = 0; i < _points.Length; i++)
        {
            _points[i].Normal = Vector3.Normalize(_points[i].Normal);
            _maxY = Math.Max(_maxY, _points[i].Position.Y);
            _minY = Math.Min(_minY, _points[i].Position.Y);
        }
    }

    /// <summary>
    /// OpenGL initialization
    /// </summary>
    protected unsafe override void OnOpenGlInit(GlInterface GL, int fb)
    {
        CheckAndLogOpenGLErrors(GL);

        _logger.Info($"Renderer: { GL.GetString(GL_RENDERER) } Version: { GL.GetString(GL_VERSION) }");
        
        // Load the source of the vertex shader and compile it.
        _vertexShader = GL.CreateShader(GL_VERTEX_SHADER);
        _logger.Info(GL.CompileShaderAndGetError(_vertexShader, VertexShaderSource));

        // Load the source of the fragment shader and compile it.
        _fragmentShader = GL.CreateShader(GL_FRAGMENT_SHADER);
        _logger.Info(GL.CompileShaderAndGetError(_fragmentShader, FragmentShaderSource));
        
        // Create the shader program, attach the vertex and fragment shaders and link the program.
        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, _vertexShader);
        GL.AttachShader(_shaderProgram, _fragmentShader);
        const int positionLocation = 0;
        const int normalLocation = 1;
        GL.BindAttribLocationString(_shaderProgram, positionLocation, "aPos");
        GL.BindAttribLocationString(_shaderProgram, normalLocation, "aNormal");
        _logger.Info(GL.LinkProgramAndGetError(_shaderProgram));
        CheckAndLogOpenGLErrors(GL);
        
        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
        CheckAndLogOpenGLErrors(GL);
        
        fixed (void* pdata = _points)
        {
            GL.BufferData(GL_ARRAY_BUFFER, new IntPtr(_points.Length * _vertexSize), new IntPtr(pdata), GL_STATIC_DRAW);
        }
        
        _indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indexBufferObject);
        CheckAndLogOpenGLErrors(GL);

        fixed (void* pdata = _indices)
        {
            GL.BufferData(GL_ELEMENT_ARRAY_BUFFER, new IntPtr(_indices.Length * sizeof(ushort)), new IntPtr(pdata), GL_STATIC_DRAW);
        }

        CheckAndLogOpenGLErrors(GL);
        
        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);
        CheckAndLogOpenGLErrors(GL);
        
        GL.VertexAttribPointer(positionLocation, 3, GL_FLOAT, 0, _vertexSize, IntPtr.Zero);
        GL.VertexAttribPointer(normalLocation, 3, GL_FLOAT, 0, _vertexSize, new IntPtr(12));
        GL.EnableVertexAttribArray(positionLocation);
        GL.EnableVertexAttribArray(normalLocation);
        CheckAndLogOpenGLErrors(GL);
    }
    
    /// <summary>
    /// Called when frame have to be rendered
    /// </summary>
    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        unsafe
        {
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
            gl.Enable(GL_DEPTH_TEST);
            gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
            var GL = gl;

            GL.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
            GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indexBufferObject);
            GL.BindVertexArray(_vertexArrayObject);
            GL.UseProgram(_shaderProgram);
            CheckAndLogOpenGLErrors(GL);
            var projection =
                Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)(Bounds.Width / Bounds.Height), 0.01f, 1000);
            
            var view = Matrix4x4.CreateLookAt(new Vector3(25, 25, 25), new Vector3(), new Vector3(0, 1, 0));
            var model = Matrix4x4.CreateFromYawPitchRoll(0, 0, 0);
            
            var modelLoc = GL.GetUniformLocationString(_shaderProgram, "uModel");
            var viewLoc = GL.GetUniformLocationString(_shaderProgram, "uView");
            var projectionLoc = GL.GetUniformLocationString(_shaderProgram, "uProjection");
            var maxYLoc = GL.GetUniformLocationString(_shaderProgram, "uMaxY");
            var minYLoc = GL.GetUniformLocationString(_shaderProgram, "uMinY");
            GL.UniformMatrix4fv(modelLoc, 1, false, &model);
            GL.UniformMatrix4fv(viewLoc, 1, false, &view);
            GL.UniformMatrix4fv(projectionLoc, 1, false, &projection);
            GL.Uniform1f(maxYLoc, _maxY);
            GL.Uniform1f(minYLoc, _minY);
            CheckAndLogOpenGLErrors(GL);
            
            GL.DrawElements(GL_TRIANGLES, _indices.Length, GL_UNSIGNED_SHORT, IntPtr.Zero);

            CheckAndLogOpenGLErrors(GL);
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
        GL.DeleteProgram(_shaderProgram);
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);
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
    
    private string GetShader(bool fragment, string shader)
    {
        var version = (GlVersion.Type == GlProfileType.OpenGL ?
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 150 : 120 :
            100);
        var data = "#version " + version + "\n";
        if (GlVersion.Type == GlProfileType.OpenGLES)
            data += "precision mediump float;\n";
        if (version >= 150)
        {
            shader = shader.Replace("attribute", "in");
            if (fragment)
                shader = shader
                    .Replace("varying", "in")
                    .Replace("//DECLAREGLFRAG", "out vec4 outFragColor;")
                    .Replace("gl_FragColor", "outFragColor");
            else
                shader = shader.Replace("varying", "out");
        }

        data += shader;

        return data;
    }
}