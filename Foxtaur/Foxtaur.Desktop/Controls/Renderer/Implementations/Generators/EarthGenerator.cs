using System;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
using Foxtaur.Desktop.Controls.Renderer.Models;
using Foxtaur.Helpers;
using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibGeo.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibGeo.Services.Abstractions.DemProviders;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibResources.Enums;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer.Implementations.Generators;

public class EarthGenerator : IEarthGenerator
{
    private readonly ISphereCoordinatesProvider _sphereCoordinatesProvider;
    private readonly IDemProvider _demProvider;

    public EarthGenerator(ISphereCoordinatesProvider sphereCoordinatesProvider,
        IDemProvider demProvider)
    {
        _sphereCoordinatesProvider = sphereCoordinatesProvider;
        _demProvider = demProvider;
    }
    
    public EarthSegment GenerateEarthSegment(GeoSegment segment, float gridStep)
    {
        return new EarthSegment(segment, gridStep);
    }

    public void GenerateMeshForSegment(EarthSegment segment, ZoomLevel desiredZoomLevel)
    {
        _ = segment ?? throw new ArgumentNullException(nameof(segment));
        
        var segmentMesh = new Mesh();
        
        for (var lat = segment.GeoSegment.SouthLat; lat < segment.GeoSegment.NorthLat; lat += segment.GridStep)
        {
            var latNorther = lat + segment.GridStep;

            // First two points of a stripe
            var firstPair = GenerateAndAddPointsPair(segmentMesh, lat, segment.GeoSegment.WestLon, latNorther, segment.GeoSegment.WestLon, desiredZoomLevel);
            var i3D0 = firstPair.Item1;
            var i3D1 = firstPair.Item2;

            // Intermediate triangles
            for (var lon = segment.GeoSegment.WestLon - segment.GridStep; lon > segment.GeoSegment.EastLon; lon -= segment.GridStep)
            {
                var intermediatePair = GenerateAndAddPointsPair(segmentMesh, lat, lon, latNorther, lon, desiredZoomLevel);
                var i3D2 = intermediatePair.Item1;
                var i3D3 = intermediatePair.Item2;

                segmentMesh.AddIndex(i3D0);
                segmentMesh.AddIndex(i3D1);
                segmentMesh.AddIndex(i3D3);

                segmentMesh.AddIndex(i3D3);
                segmentMesh.AddIndex(i3D2);
                segmentMesh.AddIndex(i3D0);

                i3D0 = i3D2;
                i3D1 = i3D3;
            }
            
            // Two last triangles of a stripe
            var lastPair = GenerateAndAddPointsPair(segmentMesh, lat, segment.GeoSegment.EastLon, latNorther, segment.GeoSegment.EastLon, desiredZoomLevel);
            var i3last0 = lastPair.Item1;
            var i3last1 = lastPair.Item2;

            segmentMesh.AddIndex(i3D0);
            segmentMesh.AddIndex(i3D1);
            segmentMesh.AddIndex(i3last1);

            segmentMesh.AddIndex(i3last1);
            segmentMesh.AddIndex(i3last0);
            segmentMesh.AddIndex(i3D0);
        }
        
        segment.UpdateMesh(segmentMesh);
    }

    public Sphere GenerateEarthSphere()
    {
        return new Sphere(new PlanarPoint3D(0.0f, 0.0f, 0.0f), GeoConstants.EarthRadius);
    }

    /// <summary>
    /// Generates a pair of points (geo), converts them to planars and add them to mesh.
    /// Returns pair of generated points indices 
    /// </summary>
    private ValueTuple<uint, uint> GenerateAndAddPointsPair(Mesh mesh,
        float p0Lat,
        float p0Lon,
        float p1Lat,
        float p1Lon,
        ZoomLevel desiredZoomLevel)
    {
        _ = mesh ?? throw new ArgumentNullException(nameof(mesh));

        // Geopoints
        var altitude0 = _demProvider.GetSurfaceAltitude(p0Lat, p0Lon, desiredZoomLevel);
        var geoPoint0 = new GeoPoint(p0Lat, p0Lon, altitude0);

        var altitude1 = _demProvider.GetSurfaceAltitude(p1Lat, p1Lon, desiredZoomLevel);
        var geoPoint1 = new GeoPoint(p1Lat, p1Lon, altitude1);

        // Planar coordinates (3D + Texture)
        var p3D0 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint0);
        var t2D0 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint0);

        var p3D1 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint1);
        var t2D1 = _sphereCoordinatesProvider.GeoToPlanar2D(geoPoint1);

        // Adding vertices (not indices!)
        var i3D0 = mesh.AddVertex(p3D0, t2D0);
        var i3D1 = mesh.AddVertex(p3D1, t2D1);

        return (i3D0, i3D1);
    }
}