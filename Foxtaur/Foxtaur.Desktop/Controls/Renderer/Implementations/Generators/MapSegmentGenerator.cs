using System;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
using Foxtaur.Desktop.Controls.Renderer.Helpers;
using Foxtaur.Desktop.Controls.Renderer.Models;
using Foxtaur.Helpers;
using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibGeo.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibGeo.Services.Abstractions.DemProviders;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Services.Abstractions.Zoom;
using Foxtaur.LibResources.Enums;
using Foxtaur.LibResources.Models.HighResMap;

namespace Foxtaur.Desktop.Controls.Renderer.Implementations.Generators;

public class MapSegmentGenerator : IMapSegmentGenerator
{
    private readonly IDemProvider _demProvider;
    private readonly ISphereCoordinatesProvider _sphereCoordinatesProvider;
    private readonly IZoomService _zoomService;

    public MapSegmentGenerator(IDemProvider demProvider,
        ISphereCoordinatesProvider speSphereCoordinatesProvider,
        IZoomService zoomService)
    {
        _demProvider = demProvider;
        _sphereCoordinatesProvider = speSphereCoordinatesProvider;
        _zoomService = zoomService;
    }
    
    public MapSegment Generate(HighResMap map)
    {
        _ = map ?? throw new ArgumentNullException(nameof(map));
        
        var geoSegment = new GeoSegment(map.Fragment.NorthLat, map.Fragment.WestLon, map.Fragment.SouthLat, map.Fragment.EastLon);
        
        var segmentMesh = new Mesh();
        
        for (var lat = geoSegment.SouthLat.GetClosestLatNode(_zoomService.ZoomLevelData.MeshesStep); lat < geoSegment.NorthLat; lat += _zoomService.ZoomLevelData.MeshesStep)
        {
            var latNorther = lat + _zoomService.ZoomLevelData.MeshesStep;

            var granulatedWestLon = geoSegment.WestLon.GetClosestLonNodeWest(_zoomService.ZoomLevelData.MeshesStep);

            // First two points of a stripe
            var firstPair = GenerateAndAddPointsPair(segmentMesh, lat, granulatedWestLon, latNorther, granulatedWestLon, _zoomService.ZoomLevelData.Level, map.Fragment);
            var i3D0 = firstPair.Item1;
            var i3D1 = firstPair.Item2;

            // Intermediate triangles
            var granulatedEastLon = geoSegment.EastLon.GetClosestLonNodeEast(_zoomService.ZoomLevelData.MeshesStep);
            
            for (var lon = granulatedWestLon - _zoomService.ZoomLevelData.MeshesStep; lon > granulatedEastLon; lon -= _zoomService.ZoomLevelData.MeshesStep)
            {
                var intermediatePair = GenerateAndAddPointsPair(segmentMesh, lat, lon, latNorther, lon, _zoomService.ZoomLevelData.Level, map.Fragment);
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
            var lastPair = GenerateAndAddPointsPair(segmentMesh, lat, granulatedEastLon, latNorther, granulatedEastLon, _zoomService.ZoomLevelData.Level, map.Fragment);
            var i3last0 = lastPair.Item1;
            var i3last1 = lastPair.Item2;

            segmentMesh.AddIndex(i3D0);
            segmentMesh.AddIndex(i3D1);
            segmentMesh.AddIndex(i3last1);

            segmentMesh.AddIndex(i3last1);
            segmentMesh.AddIndex(i3last0);
            segmentMesh.AddIndex(i3D0);
        }

        return new MapSegment(geoSegment, segmentMesh, map);
    }
    
    /// <summary>
    /// Generates a pair of points (geo), converts them to planars and add them to mesh.
    /// Returns pair of generated points indices 
    /// </summary>
    private ValueTuple<uint, uint> GenerateAndAddPointsPair(Mesh mesh,
        double p0Lat,
        double p0Lon,
        double p1Lat,
        double p1Lon,
        ZoomLevel desiredZoomLevel,
        HighResMapFragment fragment)
    {
        _ = mesh ?? throw new ArgumentNullException(nameof(mesh));

        // Geopoints
        var altitude0 = _demProvider.GetSurfaceAltitude(p0Lat, p0Lon, desiredZoomLevel) + RendererConstants.MapsAltitudeIncrement;
        var geoPoint0 = new GeoPoint(p0Lat, p0Lon, altitude0);

        var altitude1 = _demProvider.GetSurfaceAltitude(p1Lat, p1Lon, desiredZoomLevel) + RendererConstants.MapsAltitudeIncrement;
        var geoPoint1 = new GeoPoint(p1Lat, p1Lon, altitude1);

        // Planar coordinates (3D + Texture)
        var p3D0 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint0);
        var texCoords0 = fragment.GetTextureCoordinates(geoPoint0.Lat, geoPoint0.Lon);
        var t2D0 = new PlanarPoint2D(texCoords0.Item1, texCoords0.Item2);

        var p3D1 = _sphereCoordinatesProvider.GeoToPlanar3D(geoPoint1);
        var texCoords1 = fragment.GetTextureCoordinates(geoPoint1.Lat, geoPoint1.Lon);
        var t2D1 = new PlanarPoint2D(texCoords1.Item1, texCoords1.Item2);

        // Adding vertices (not indices!)
        var i3D0 = mesh.AddVertex(p3D0, t2D0);
        var i3D1 = mesh.AddVertex(p3D1, t2D1);

        return (i3D0, i3D1);
    }
}