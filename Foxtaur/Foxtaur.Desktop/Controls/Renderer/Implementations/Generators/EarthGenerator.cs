using System;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Services.Abstractions.CoordinateProviders;

namespace Foxtaur.Desktop.Controls.Renderer.Implementations.Generators;

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
            var lastPair = GenerateAndAddPointsPair(earthMesh, lat, -1.0f * (float)Math.PI, latNorther, -1.0f * (float)Math.PI);
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
        return new Sphere(new PlanarPoint3D(0.0f, 0.0f, 0.0f), RendererConstants.EarthRadius);
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
        var geoPoint0 = new GeoPoint(p0Lat, p0Lon, RendererConstants.EarthRadius);
        var geoPoint1 = new GeoPoint(p1Lat, p1Lon, RendererConstants.EarthRadius);

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