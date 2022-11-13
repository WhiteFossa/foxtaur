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

        var earthIndices = new List<uint>();

        uint verticesCounter = 0;
        
        for (var lat = 0.0f; lat < (float)Math.PI / 2.0f; lat += (float)Math.PI / 90.0f)
        {
            var latNorther = lat + (float)Math.PI / 90.0f;
            
            for (var lon = (float)Math.PI; lon > -1.0f * (float)Math.PI; lon -= (float)Math.PI / 90.0f)
            {
                var lonEaster = lon - (float)Math.PI / 90.0f;
                
                var geoPoint0 = new GeoPoint() { Lat = lat, Lon = lon, H = 1.0f };
                var geoPoint1 = new GeoPoint() { Lat = latNorther, Lon = lon, H = 1.0f };
                var geoPoint2 = new GeoPoint() { Lat = latNorther, Lon = lonEaster, H = 1.0f };
                var geoPoint3 = new GeoPoint() { Lat = lat, Lon = lonEaster, H = 1.0f };

                var p3D0 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint0);
                var t2D0 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint0);
                
                var p3D1 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint1);
                var t2D1 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint1);
                
                var p3D2 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint2);
                var t2D2 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint2);
                
                var p3D3 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint3);
                var t2D3 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint3);

                var i3D0 = verticesCounter;
                earthVertices.Add(p3D0.X);
                earthVertices.Add(p3D0.Y);
                earthVertices.Add(p3D0.Z);
                earthVertices.Add(t2D0.Y);
                earthVertices.Add(t2D0.X);
                verticesCounter++;
                
                var i3D1 = verticesCounter;
                earthVertices.Add(p3D1.X);
                earthVertices.Add(p3D1.Y);
                earthVertices.Add(p3D1.Z);
                earthVertices.Add(t2D1.Y);
                earthVertices.Add(t2D1.X);
                verticesCounter++;
                
                var i3D2 = verticesCounter;
                earthVertices.Add(p3D2.X);
                earthVertices.Add(p3D2.Y);
                earthVertices.Add(p3D2.Z);
                earthVertices.Add(t2D2.Y);
                earthVertices.Add(t2D2.X);
                verticesCounter++;
                
                var i3D3 = verticesCounter;
                earthVertices.Add(p3D3.X);
                earthVertices.Add(p3D3.Y);
                earthVertices.Add(p3D3.Z);
                earthVertices.Add(t2D3.Y);
                earthVertices.Add(t2D3.X);
                verticesCounter++;
                
                earthIndices.Add(i3D0);
                earthIndices.Add(i3D1);
                earthIndices.Add(i3D2);
                
                earthIndices.Add(i3D2);
                earthIndices.Add(i3D3);
                earthIndices.Add(i3D0);
            }
        }
        
        _vertices = earthVertices.ToArray();
        _indices = earthIndices.ToArray();
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

        //_silkGLContext.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

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
}