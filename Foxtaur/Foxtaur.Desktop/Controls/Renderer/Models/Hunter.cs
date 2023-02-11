using System;
using Foxtaur.Helpers;
using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Helpers;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibGeo.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Helpers;
using Foxtaur.LibRenderer.Services.Abstractions.Camera;
using ImageMagick;
using MathNet.Numerics.LinearAlgebra;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer.Models;

/// <summary>
/// Displayable hunter
/// </summary>
public class Hunter
{
    private readonly ICamera _camera;
    
    private ICoordinatesProvider _sphereCoordinatesProvider;
    private Mesh _mesh;
    private Texture _texture;

    /// <summary>
    /// Texture to use in flat UI
    /// </summary>
    public Texture UiTexture { get; private set; }

    /// <summary>
    /// Hunter position
    /// </summary>
    public GeoPoint Position { get; set; }

    public Hunter(ICoordinatesProvider sphereCoordinatesProvider,
        ICamera camera)
    {
        _sphereCoordinatesProvider = sphereCoordinatesProvider;
        _camera = camera;
    }

    /// <summary>
    /// Prepare hunter's texture
    /// </summary>
    public void PrepareTextures(GL glContext)
    {
        // 3D UI texture
        if (_texture != null)
        {
            _texture.Dispose();
        }
        
        _texture = new Texture(glContext, @"Resources/Textures/hunter_marker.png");
        
        // Flat UI texture
        if (UiTexture != null)
        {
            UiTexture.Dispose();
        }
        
        UiTexture = new Texture(glContext, @"Resources/Textures/hunter_marker.png");
    }
    
        /// <summary>
    /// Draw the hunter
    /// </summary>
    public unsafe void Draw(GL glContext)
    {
        // 1) 3D position
        var hunterPosition3DLower = _sphereCoordinatesProvider.GeoToPlanar3D(Position);
        var hunterPosition3DHigher = _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(Position.Lat, Position.Lon, Position.H + 0.000005)); // TODO: Specify Y size here
        
        // 2) Vector from camera position to hunter position (normalized)
        var vectorFromCameraLower = (hunterPosition3DLower.AsVector() - _camera.Position3D.AsVector()).Normalize() * 0.000005; // TODO: Specify X size here
        var vectorFromCameraHigher = (hunterPosition3DHigher.AsVector() - _camera.Position3D.AsVector()).Normalize() * 0.000005;
        
        // 3) Nadir vector, we will rotate around it
        var nadirVectorLower = GeoConstants.EarthCenter - hunterPosition3DLower.AsVector();
        var nadirVectorHigher = GeoConstants.EarthCenter - hunterPosition3DHigher.AsVector();
        
        // 4) Corners
        var leftBottom = vectorFromCameraLower.RotateAround(nadirVectorLower, -90.0.ToRadians()) + hunterPosition3DLower.AsVector();
        var rightBottom = vectorFromCameraLower.RotateAround(nadirVectorLower, 90.0.ToRadians()) + hunterPosition3DLower.AsVector();
        
        var leftTop = vectorFromCameraHigher.RotateAround(nadirVectorHigher, -90.0.ToRadians()) + hunterPosition3DHigher.AsVector();
        var rightTop = vectorFromCameraHigher.RotateAround(nadirVectorHigher, 90.0.ToRadians()) + hunterPosition3DHigher.AsVector();
        
        // Mesh
        if (_mesh != null)
        {
            _mesh.Dispose();
        }

        _mesh = new Mesh();
        
        var leftBottomIndex = _mesh.AddVertex(leftBottom.AsPlanarPoint3D(), new PlanarPoint2D(0, 1));
        _mesh.AddIndex(leftBottomIndex);
        
        var leftTopIndex = _mesh.AddVertex(leftTop.AsPlanarPoint3D(), new PlanarPoint2D(0, 0));
        _mesh.AddIndex(leftTopIndex);
        
        var rightBottomIndex = _mesh.AddVertex(rightBottom.AsPlanarPoint3D(), new PlanarPoint2D(1, 1));
        _mesh.AddIndex(rightBottomIndex);
        
        var rightTopIndex = _mesh.AddVertex(rightTop.AsPlanarPoint3D(), new PlanarPoint2D(1, 0));
        
        _mesh.AddIndex(rightBottomIndex);
        _mesh.AddIndex(leftTopIndex);
        _mesh.AddIndex(rightTopIndex);

        // Actual drawing
        _mesh.GenerateBuffers(glContext);
        
        _texture.Bind();
        _mesh.BindBuffers(glContext);
        glContext.DrawElements(PrimitiveType.Triangles, (uint)_mesh.Indices.Count, DrawElementsType.UnsignedInt, null);
    }
    
    /*/// <summary>
    /// Draw the hunter
    /// </summary>
    public unsafe void Draw(GL glContext)
    {
        // 1) Vector from the current position to the Earth center
        var position3D = _sphereCoordinatesProvider.GeoToPlanar3D(Position);
        var nadirVector = GeoConstants.EarthCenter - position3D.AsVector();
        
        // 2) Transforming to zenith vector
        var toNorthVector = _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(Math.PI / 2.0, 0, Position.H)).AsVector() - position3D.AsVector();
        var nadirNorthPerpVector = DoubleHelper.Cross3(nadirVector, toNorthVector); // Perpendicular to nadir vector and north vector
        
        var zenithVector = nadirVector.RotateAround(nadirNorthPerpVector, Math.PI);
        var hunterHeight = zenithVector.Normalize() * RendererConstants.HunterHeight;
        hunterHeight += zenithVector; // On top of zenith vector
        
        // 3) Horizontal vector, we will rotate it later
        var hunterRadius = nadirNorthPerpVector.Normalize() * RendererConstants.HunterRadius;
        hunterRadius += hunterHeight;

        // Mesh
        if (_mesh != null)
        {
            _mesh.Dispose();
        }

        _mesh = new Mesh();

        // 4) Generating a cone
        var hunterHeight3D = hunterHeight.AsPlanarPoint3D();
        var rotatedHunterRadiusOld = hunterRadius;
        Vector<double> rotatedHunterRadiusNew = null;
        
        for (double angle = 0; angle < Math.PI * 2.0; angle += 0.1)
        {
            rotatedHunterRadiusNew = hunterRadius.RotateAround(hunterHeight, angle);
            
            AddHunterTriangles(position3D, rotatedHunterRadiusOld.AsPlanarPoint3D(), rotatedHunterRadiusNew.AsPlanarPoint3D(), hunterHeight3D);

            rotatedHunterRadiusOld = rotatedHunterRadiusNew;
        }
        
        // Last triangles
        AddHunterTriangles(position3D, rotatedHunterRadiusOld.AsPlanarPoint3D(), hunterRadius.AsPlanarPoint3D(), hunterHeight3D);

        // Actual drawing
        _mesh.GenerateBuffers(glContext);
        
        _texture.Bind();
        _mesh.BindBuffers(glContext);
        glContext.DrawElements(PrimitiveType.Triangles, (uint)_mesh.Indices.Count, DrawElementsType.UnsignedInt, null);
    }*/

    private void AddHunterTriangles(PlanarPoint3D hunterPosition, PlanarPoint3D hunterRadiusOld, PlanarPoint3D hunterRadiusNew, PlanarPoint3D hunterHeight)
    {
        // Cone side
        var sideIndex1 = _mesh.AddVertex(hunterPosition, new PlanarPoint2D(0, 0));
        var sideIndex2 = _mesh.AddVertex(hunterRadiusOld, new PlanarPoint2D(0, 1));
        var sideIndex3 = _mesh.AddVertex(hunterRadiusNew, new PlanarPoint2D(1, 1));
        
        _mesh.AddIndex(sideIndex1);
        _mesh.AddIndex(sideIndex2);
        _mesh.AddIndex(sideIndex3);
            
        // Top disk
        var diskIndex1 = _mesh.AddVertex(hunterHeight, new PlanarPoint2D(0, 0));
        var diskIndex2 = _mesh.AddVertex(hunterRadiusOld, new PlanarPoint2D(0, 1));
        var diskIndex3 = _mesh.AddVertex(hunterRadiusNew, new PlanarPoint2D(1, 1));

        _mesh.AddIndex(diskIndex1);
        _mesh.AddIndex(diskIndex2);
        _mesh.AddIndex(diskIndex3);
    }
}