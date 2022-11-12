using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Foxtaur.LibRenderer.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer;

public class DesktopRendererControl : OpenGlControlBase
{
    /// <summary>
    /// Vertexes positions
    /// </summary>
    private const int VerticesPositionLocation = 0;

    /// <summary>
    /// Vertexes texture coords
    /// </summary>
    private const int VerticesTextureCoordsLocation = 1;

    /// <summary>
    /// Silk.NET OpenGL context
    /// </summary>
    private GL _silkGLContext;
    
    private readonly ITexturesLoader _texturesLoader;

    /// <summary>
    /// Vertex size in bytes
    /// </summary>
    private readonly int _vertexSize;

    private Logger _logger = LogManager.GetCurrentClassLogger();
    
    /// <summary>
    /// Vertices
    /// </summary>
    private readonly float[] _vertices;

    /// <summary>
    /// Indices
    /// </summary>
    private  readonly uint[] _indices;
    
    private BufferObject<float> _verticesBufferObject;
    private BufferObject<uint> _elementsBufferObject;
    private VertexArrayObject<float, uint> _verticesArrayObject;

    private Shader _shader;

    //private readonly Foxtaur.LibRenderer.Models.Texture _texture;
    
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
        _texturesLoader = Program.Di.GetService<ITexturesLoader>();
        
        _vertexSize = Marshal.SizeOf<Vertex>();
        
        // Loading points
        _vertices = new float[]
        {
            // X    Y       Z       R   G   B   A
            0.5f,   0.5f,   0.0f,   1,  0,  0,  1,
            0.5f,   -0.5f,  0.0f,   0,  0,  0,  1,
            -0.5f,  -0.5f,  0.0f,   0,  0,  1,  1,
            -0.5f,  0.5f,   0.5f,   0,  0,  0,  1
        };
        
        _indices = new uint[]
        {
            0, 1, 3,
            1, 2, 3
        };
        
        // Loading texture
        //_texture = _texturesLoader.LoadTextureFromFile(@"Resources/davydovo_res.png");

        /*_texture = new Foxtaur.LibRenderer.Models.Texture()
        {
            Width = 1024,
            Height = 1024,
            Data = new byte[1024 * 1024 * 4]
        };

        for (var i = 0; i < _texture.Data.Length; i++)
        {
            _texture.Data[i] = (byte)(i % 256);
        }*/
    }

    /// <summary>
    /// OpenGL initialization
    /// </summary>
    protected unsafe override void OnOpenGlInit(GlInterface gl, int fb)
    {
        base.OnOpenGlInit(gl, fb);
            
        _silkGLContext = GL.GetApi(gl.GetProcAddress);

        // Object for vertices
        _verticesBufferObject = new BufferObject<float>(_silkGLContext, _vertices, BufferTargetARB.ArrayBuffer);
        
        // Object for indices
        _elementsBufferObject = new BufferObject<uint>(_silkGLContext, _indices, BufferTargetARB.ElementArrayBuffer);
        
        // Vertices array
        _verticesArrayObject = new VertexArrayObject<float, uint>(_silkGLContext, _verticesBufferObject, _elementsBufferObject);

        //Telling the VAO object how to lay out the attribute pointers
        _verticesArrayObject.VertexAttributePointer(VerticesPositionLocation, 3, VertexAttribPointerType.Float, 7, 0);
        _verticesArrayObject.VertexAttributePointer(VerticesTextureCoordsLocation, 4, VertexAttribPointerType.Float, 7, 3);

        // Loading shaders
        _shader = new Shader(_silkGLContext, @"Controls/Renderer/Shaders/shader.vert", @"Controls/Renderer/Shaders/shader.frag");
        
        /*// Texture
        _textureObject = GL.GenTexture();
        GL.BindTexture(GL_TEXTURE_2D, _textureObject);
        CheckAndLogOpenGLErrors(GL);

        fixed (void* tdata = _texture.Data)
        {
            GL.TexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, _texture.Width, _texture.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, new IntPtr(tdata));
        }
        CheckAndLogOpenGLErrors(GL);
        
        GL.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
        GL.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
        CheckAndLogOpenGLErrors(GL);*/
    }
    
    /// <summary>
    /// Called when frame have to be rendered
    /// </summary>
    protected unsafe override void OnOpenGlRender(GlInterface gl, int fb)
    {
        _silkGLContext.ClearColor(Color.Blue);
        _silkGLContext.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        _silkGLContext.Enable(EnableCap.DepthTest);
        _silkGLContext.Viewport(0,0, (uint)Bounds.Width, (uint)Bounds.Height);
            
        _elementsBufferObject.Bind();
        _verticesBufferObject.Bind();
        _verticesArrayObject.Bind();
        
        _shader.Use();
        //_shader.SetUniform1i("uBlue", (float) Math.Sin(DateTime.Now.Millisecond / 1000f * Math.PI));

        _silkGLContext.DrawElements(PrimitiveType.Triangles, (uint)_indices.Length, DrawElementsType.UnsignedInt, null);
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    /// <summary>
    /// OpenGL de-initialization
    /// </summary>
    protected override void OnOpenGlDeinit(GlInterface gl, int fb)
    {
        _verticesBufferObject.Dispose();
        _elementsBufferObject.Dispose();
        _verticesArrayObject.Dispose();
        _shader.Dispose();
        base.OnOpenGlDeinit(gl, fb);
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