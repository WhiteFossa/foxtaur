using System;
using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Helpers;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibGeo.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Helpers;
using ImageMagick;
using MathNet.Numerics.LinearAlgebra;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer.Models;

/// <summary>
/// Displayable hunter
/// </summary>
public class Hunter
{
    private ICoordinatesProvider _sphereCoordinatesProvider;
    private Mesh _mesh;
    private Texture _texture;
    
    /// <summary>
    /// Hunter position
    /// </summary>
    public GeoPoint Position { get; set; }

    public Hunter(ICoordinatesProvider sphereCoordinatesProvider)
    {
        _sphereCoordinatesProvider = sphereCoordinatesProvider;
    }

    /// <summary>
    /// Prepare hunter's texture
    /// </summary>
    public void PrepareTexture(GL glContext)
    {
        // Texture
        if (_texture != null)
        {
            _texture.Dispose();
        }
        
        using (var hunterTextureImage = new MagickImage(new MagickColor(255, 0, 0, 255), 100, 100))
        {
            _texture = new Texture(glContext, hunterTextureImage);
        }
    }
    
    /// <summary>
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
    }

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