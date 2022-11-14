using System;
using Foxtaur.Desktop.Controls.Renderer.Abstractions;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Services.Abstractions.CoordinateProviders;

namespace Foxtaur.Desktop.Controls.Renderer.Implementations;

public class EarthGenerator : IEarthGenerator
{
    private readonly ISphereCoordinatesProvider _sphereCoordinatesProvider;

    public EarthGenerator(ISphereCoordinatesProvider sphereCoordinatesProvider)
    {
        _sphereCoordinatesProvider = sphereCoordinatesProvider;
    }
    
    public Mesh GenerateFullEarth(float gridStep)
    {
        var earthMesh = new Mesh();
        
        for (var lat = (float)Math.PI / -2.0f; lat < (float)Math.PI / 2.0f; lat += gridStep)
        {
            var latNorther = lat + gridStep;

            // First two points of a stripe
            var geoPoint0 = new GeoPoint(lat, (float)Math.PI, RendererConstants.EarthRadius);
            var geoPoint1 = new GeoPoint(latNorther, (float)Math.PI, RendererConstants.EarthRadius);
            
            // Planar coordinates of the two first points
            var p3D0 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint0);
            var t2D0 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint0);
            
            var p3D1 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint1);
            var t2D1 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint1);
            
            // Adding vertexes (not indexes!)
            var i3D0 = earthMesh.AddVertex(p3D0, t2D0);
            var i3D1 = earthMesh.AddVertex(p3D1, t2D1);
            
            for (var lon = (float)Math.PI - gridStep; lon > -1.0f * (float)Math.PI; lon -= gridStep)
            {
                geoPoint0 = new GeoPoint(lat, lon, RendererConstants.EarthRadius);
                geoPoint1 = new GeoPoint(latNorther, lon, RendererConstants.EarthRadius);
                
                p3D0 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint0);
                t2D0 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint0);
            
                p3D1 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint1);
                t2D1 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint1);
                
                var i3D2 = earthMesh.AddVertex(p3D0, t2D0);
                var i3D3 = earthMesh.AddVertex(p3D1, t2D1);
                
                earthMesh.AddIndex(i3D0);
                earthMesh.AddIndex(i3D1);
                earthMesh.AddIndex(i3D3);
                
                earthMesh.AddIndex(i3D3);
                earthMesh.AddIndex(i3D2);
                earthMesh.AddIndex(i3D0);

                i3D0 = i3D2;
                i3D1 = i3D3;
            }
            
            // Two final triangles of a stripe
            geoPoint0 = new GeoPoint(lat, -1.0f * (float)Math.PI, RendererConstants.EarthRadius);
            geoPoint1 = new GeoPoint(latNorther, -1.0f * (float)Math.PI, RendererConstants.EarthRadius);
                
            p3D0 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint0);
            t2D0 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint0);
            
            p3D1 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint1);
            t2D1 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint1);
                
            var i3final0 = earthMesh.AddVertex(p3D0, t2D0);
            var i3final1 = earthMesh.AddVertex(p3D1, t2D1);
            
            earthMesh.AddIndex(i3D0);
            earthMesh.AddIndex(i3D1);
            earthMesh.AddIndex(i3final1);
                
            earthMesh.AddIndex(i3final1);
            earthMesh.AddIndex(i3final0);
            earthMesh.AddIndex(i3D0);
        }

        return earthMesh;
    }
}