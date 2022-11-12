using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
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

    private Texture _textureObject;

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
        _vertexSize = Marshal.SizeOf<Vertex>();
        
        // Loading points
        _vertices = new float[]
        {
            // X    Y       Z       Tx     Ty
            0.5f,   0.5f,   0.0f,   1.0f,  1.0f,
            0.5f,   -0.5f,  0.0f,   1.0f,  0.0f,
            -0.5f,  -0.5f,  0.0f,   0.0f,  0.0f,
            -0.5f,  0.5f,   0.0f,   0.0f,  1.0f,
        };
        
        _indices = new uint[]
        {
            0, 1, 3,
            1, 2, 3
        };
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
        _verticesArrayObject.VertexAttributePointer(VerticesPositionLocation, 3, VertexAttribPointerType.Float, 5, 0);
        _verticesArrayObject.VertexAttributePointer(VerticesTextureCoordsLocation, 2, VertexAttribPointerType.Float, 5, 3);

        // Loading shaders
        _shader = new Shader(_silkGLContext, @"Resources/Shaders/shader.vert", @"Resources/Shaders/shader.frag");

        // Loading texture
        _textureObject = new Texture(_silkGLContext, @"Resources/davydovo.png");
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
        _shader.SetUniform1i("ourTexture", 0);
        
        _textureObject.Bind();

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
        _textureObject.Dispose();
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