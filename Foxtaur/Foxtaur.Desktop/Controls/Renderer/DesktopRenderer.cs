using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Foxtaur.LibRenderer.Constants;
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
    
    // private static Vector3 CameraDirection = Vector3.Zero;
    // private static float CameraYaw = -90f;
    // private static float CameraPitch = 0f;
    // private static float CameraZoom = 45f;

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
        /*var gp1 = new GeoPoint(0.0f, 0.0f, 0.3f);
        var gp2 = new GeoPoint(-0.5f, (float)Math.PI / -2.0f, 0.3f);
        
        var pp1 = _sphereCoordinatesProvider.GeoToPlanar3D(gp1);
        var pp2 = _sphereCoordinatesProvider.GeoToPlanar3D(gp2);*/
        
        // Loading points
        var earthVertices = new List<float>();

        var earthIndices = new List<uint>();

        uint verticesCounter = 0;

        var step = (float)Math.PI / 90.0f;
        
        for (var lat = 0.0f; lat < (float)Math.PI / 2.0f - step; lat += step)
        {
            var latNorther = lat + step;
            
            for (var lon = (float)Math.PI; lon > -1.0f * (float)Math.PI + step; lon -= step)
            {
                //_logger.Error($"Lat: { lat }, Lon: { lon }");
                
                var lonEaster = lon - step;

                var geoPoint0 = new GeoPoint(lat, lon, RendererConstants.EarthRadius);
                var geoPoint1 = new GeoPoint(latNorther, lon, RendererConstants.EarthRadius);
                var geoPoint2 = new GeoPoint(latNorther, lonEaster, RendererConstants.EarthRadius);
                var geoPoint3 = new GeoPoint(lat, lonEaster, RendererConstants.EarthRadius);

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
                earthVertices.Add(t2D0.X);
                earthVertices.Add(t2D0.Y);
                verticesCounter++;
                
                var i3D1 = verticesCounter;
                earthVertices.Add(p3D1.X);
                earthVertices.Add(p3D1.Y);
                earthVertices.Add(p3D1.Z);
                earthVertices.Add(t2D1.X);
                earthVertices.Add(t2D1.Y);
                verticesCounter++;
                
                var i3D2 = verticesCounter;
                earthVertices.Add(p3D2.X);
                earthVertices.Add(p3D2.Y);
                earthVertices.Add(p3D2.Z);
                earthVertices.Add(t2D2.X);
                earthVertices.Add(t2D2.Y);
                verticesCounter++;
                
                var i3D3 = verticesCounter;
                earthVertices.Add(p3D3.X);
                earthVertices.Add(p3D3.Y);
                earthVertices.Add(p3D3.Z);
                earthVertices.Add(t2D3.X);
                earthVertices.Add(t2D3.Y);
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
        _textureObject = new Texture(_silkGLContext, @"Resources/Textures/Basemaps/HYP_50M_SR_W.jpeg");
    }
    
    /// <summary>
    /// Called when frame have to be rendered
    /// </summary>
    protected unsafe override void OnOpenGlRender(GlInterface gl, int fb)
    {
        _silkGLContext.ClearColor(Color.Blue);
        _silkGLContext.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        _silkGLContext.Enable(EnableCap.DepthTest);
        _silkGLContext.Viewport(0,0, (uint)Bounds.Width, (uint)Bounds.Height); // TODO: Move me to constants

        _elementsBufferObject.Bind();
        _verticesBufferObject.Bind();
        _verticesArrayObject.Bind();
        
        _shader.Use();
        
        // Setting shader parameters
        _shader.SetUniform1i("ourTexture", 0);
        
        // Projections
        var cameraPosition = new Vector3(0.1f, 0.1f, 0.0f);
        var cameraTarget = new Vector3(0.0f, 0.0f, 0.0f);
        var cameraDirection = Vector3.Normalize(cameraPosition - cameraTarget);
        
        var cameraUp = Vector3.UnitY;
        
        var model = Matrix4x4.CreateRotationZ(0.0f) * Matrix4x4.CreateRotationY(0.0f) * Matrix4x4.CreateRotationX((float)Math.PI / -2.0f); // Rotation
        var view = Matrix4x4.CreateLookAt(cameraPosition, cameraDirection, cameraUp); // Camera position
        var projection = Matrix4x4.CreatePerspectiveFieldOfView(3.0f, (float)Bounds.Width / (float)Bounds.Height, 0.1f, 100.0f); // Zoom
        
        _shader.SetUniform4f("uModel", model);
        _shader.SetUniform4f("uView", view);
        _shader.SetUniform4f("uProjection", projection);
        
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