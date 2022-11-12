using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibRenderer.Services.Implementations.CoordinateProviders;
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

    /// <summary>
    /// Sphere coordinates provider (will be moved to a service)
    /// </summary>
    private ICoordinatesProvider _sphereCoordinatesProvider = new SphereCoordinatesProvider();

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
        // Loading points
        var earthVertices = new List<float>();
        
        for (var lat = -1.0f * (float)Math.PI / 2.0f; lat < (float)Math.PI / 2.0f; lat += (float)Math.PI / 10.0f)
        {
            for (var lon = (float)Math.PI; lon > -1.0f * (float)Math.PI; lon -= (float)Math.PI / 10.0f)
            {
                var geoPoint = new GeoPoint() { Lat = lat, Lon = lon, H = 0.5f };

                var p3D = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint);
                
                // 3D coords
                earthVertices.Add(p3D.X);
                earthVertices.Add(p3D.Y);
                earthVertices.Add(p3D.Z);
                
                // Texture coords
                earthVertices.Add(0.0f);
                earthVertices.Add(0.0f);
            }
        }
        
        _vertices = earthVertices.ToArray();

        var indicesCount = _vertices.Length - 2;
        _indices = new uint[indicesCount * 3];
        
        uint index = 0;
        for (var i = 0; i < indicesCount; i++)
        {
            _indices[3 * i + 0] = index;
            _indices[3 * i + 1] = index + 1;
            _indices[3 * i + 2] = index + 2;

            index++;
        }

        /*_vertices = new float[]
        {
            // X    Y       Z       Tx     Ty
            1.0f,   1.0f,   0.0f,   1.0f,  1.0f,
            1.0f,   -1.0f,  0.0f,   1.0f,  0.0f,
            -1.0f,  -1.0f,  0.0f,   0.0f,  0.0f,
            -1.0f,  1.0f,   0.0f,   0.0f,  1.0f,
        };*/

        /*_indices = new uint[]
        {
            0, 1, 3,
            1, 2, 3
        };*/
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
        
        //_textureObject.Bind();

        _silkGLContext.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

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