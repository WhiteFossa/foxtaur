using System;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibGeo.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibGeo.Services.Abstractions.DemProviders;
using Foxtaur.LibRenderer.Models;

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

    public Mesh GenerateFullEarth(float gridStep)
    {
        var earthMesh = new Mesh();

        for (var lat = (float)Math.PI / -2.0f; lat < (float)Math.PI / 2.0f; lat += gridStep)
        {
            var latNorther = lat + gridStep;

            // First two points of a stripe
            var firstPair = GenerateAndAddPointsPair(earthMesh, lat, (float)Math.PI, latNorther, (float)Math.PI);
            var i3D0 = firstPair.Item1;
            var i3D1 = firstPair.Item2;

            // Intermediate triangles
            for (var lon = (float)Math.PI - gridStep; lon > -1.0f * (float)Math.PI; lon -= gridStep)
            {
                var intermediatePair = GenerateAndAddPointsPair(earthMesh, lat, lon, latNorther, lon);
                var i3D2 = intermediatePair.Item1;
                var i3D3 = intermediatePair.Item2;

                earthMesh.AddIndex(i3D0);
                earthMesh.AddIndex(i3D1);
                earthMesh.AddIndex(i3D3);

                earthMesh.AddIndex(i3D3);
                earthMesh.AddIndex(i3D2);
                earthMesh.AddIndex(i3D0);

                i3D0 = i3D2;
                i3D1 = i3D3;
            }

            // Two last triangles of a stripe
            var lastPair = GenerateAndAddPointsPair(earthMesh, lat, -1.0f * (float)Math.PI, latNorther,
                -1.0f * (float)Math.PI);
            var i3last0 = lastPair.Item1;
            var i3last1 = lastPair.Item2;

            earthMesh.AddIndex(i3D0);
            earthMesh.AddIndex(i3D1);
            earthMesh.AddIndex(i3last1);

            earthMesh.AddIndex(i3last1);
            earthMesh.AddIndex(i3last0);
            earthMesh.AddIndex(i3D0);
        }

        return earthMesh;
    }

    public Sphere GenerateEarthSphere()
    {
        return new Sphere(new PlanarPoint3D(0.0f, 0.0f, 0.0f), GeoConstants.EarthRadius);
    }

    /// <summary>
    /// Generates a pair of points (geo), converts them to planars and add them to mesh.
    /// Returns pair of generated points indices 
    /// </summary>
    private ValueTuple<uint, uint> GenerateAndAddPointsPair(Mesh mesh, float p0Lat, float p0Lon, float p1Lat,
        float p1Lon)
    {
        _ = mesh ?? throw new ArgumentNullException(nameof(mesh));

        // Geopoints
        var altitude0 = _demProvider.GetSurfaceAltitude(p0Lat, p0Lon);
        var geoPoint0 = new GeoPoint(p0Lat, p0Lon, altitude0);
        
        var altitude1 = _demProvider.GetSurfaceAltitude(p1Lat, p1Lon);
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