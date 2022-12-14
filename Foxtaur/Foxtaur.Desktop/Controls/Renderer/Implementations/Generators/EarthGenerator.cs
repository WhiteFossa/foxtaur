using System;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
using Foxtaur.Desktop.Controls.Renderer.Helpers;
using Foxtaur.Desktop.Controls.Renderer.Models;
using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Helpers;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibGeo.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibGeo.Services.Abstractions.DemProviders;
using Foxtaur.LibRenderer.Models;
using Foxtaur.LibRenderer.Services.Abstractions.Zoom;
using Foxtaur.LibResources.Enums;
using Foxtaur.LibSettings.Services.Abstractions;

namespace Foxtaur.Desktop.Controls.Renderer.Implementations.Generators;

public class EarthGenerator : IEarthGenerator
{
    private readonly ISphereCoordinatesProvider _sphereCoordinatesProvider;
    private readonly IDemProvider _demProvider;
    private readonly IZoomService _zoomService;
    private readonly ISettingsService _settingsService;

    public EarthGenerator(ISphereCoordinatesProvider sphereCoordinatesProvider,
        IDemProvider demProvider,
        IZoomService zoomService,
        ISettingsService settingsService)
    {
        _sphereCoordinatesProvider = sphereCoordinatesProvider;
        _demProvider = demProvider;
        _zoomService = zoomService;
        _settingsService = settingsService;
    }
    
    public EarthSegment GenerateEarthSegment(GeoSegment segment)
    {
        return new EarthSegment(segment);
    }

    public void GenerateMeshForSegment(EarthSegment segment)
    {
        _ = segment ?? throw new ArgumentNullException(nameof(segment));
        
        var altitudeScalingFactor = _settingsService.GetDemScale();
        
        segment.NewMesh = new Mesh();
        
        for (var lat = segment.GeoSegment.SouthLat.GetClosestLatNode(_zoomService.ZoomLevelData.MeshesStep); lat < segment.GeoSegment.NorthLat; lat += _zoomService.ZoomLevelData.MeshesStep)
        {
            var latNorther = lat + _zoomService.ZoomLevelData.MeshesStep;
            
            var granulatedWestLon = segment.GeoSegment.WestLon.GetClosestLonNodeWest(_zoomService.ZoomLevelData.MeshesStep);

            // First two points of a stripe
            var firstPair = GenerateAndAddPointsPair(segment.NewMesh, lat, granulatedWestLon, latNorther, granulatedWestLon, _zoomService.ZoomLevel, altitudeScalingFactor);
            var i3D0 = firstPair.Item1;
            var i3D1 = firstPair.Item2;

            // Intermediate triangles
            var granulatedEastLon = segment.GeoSegment.EastLon.GetClosestLonNodeEast(_zoomService.ZoomLevelData.MeshesStep);
            
            for (var lon = granulatedWestLon - _zoomService.ZoomLevelData.MeshesStep; lon > granulatedEastLon; lon -= _zoomService.ZoomLevelData.MeshesStep)
            {
                var intermediatePair = GenerateAndAddPointsPair(segment.NewMesh, lat, lon, latNorther, lon, _zoomService.ZoomLevel, altitudeScalingFactor);
                var i3D2 = intermediatePair.Item1;
                var i3D3 = intermediatePair.Item2;

                segment.NewMesh.AddIndex(i3D0);
                segment.NewMesh.AddIndex(i3D1);
                segment.NewMesh.AddIndex(i3D3);

                segment.NewMesh.AddIndex(i3D3);
                segment.NewMesh.AddIndex(i3D2);
                segment.NewMesh.AddIndex(i3D0);

                i3D0 = i3D2;
                i3D1 = i3D3;
            }
            
            // Two last triangles of a stripe
            var lastPair = GenerateAndAddPointsPair(segment.NewMesh, lat, granulatedEastLon, latNorther, granulatedEastLon, _zoomService.ZoomLevel, altitudeScalingFactor);
            var i3last0 = lastPair.Item1;
            var i3last1 = lastPair.Item2;

            segment.NewMesh.AddIndex(i3D0);
            segment.NewMesh.AddIndex(i3D1);
            segment.NewMesh.AddIndex(i3last1);

            segment.NewMesh.AddIndex(i3last1);
            segment.NewMesh.AddIndex(i3last0);
            segment.NewMesh.AddIndex(i3D0);
        }
    }

    public Sphere GenerateEarthSphere()
    {
        return new Sphere(new PlanarPoint3D(0.0, 0.0, 0.0), GeoConstants.EarthRadius);
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
        double altitudeScalingFactor)
    {
        _ = mesh ?? throw new ArgumentNullException(nameof(mesh));
        
        // Geopoints
        var altitude0 = _demProvider.GetSurfaceAltitude(p0Lat, p0Lon, desiredZoomLevel).ScaleAltitude(altitudeScalingFactor);
        var geoPoint0 = new GeoPoint(p0Lat, p0Lon, altitude0);

        var altitude1 = _demProvider.GetSurfaceAltitude(p1Lat, p1Lon, desiredZoomLevel).ScaleAltitude(altitudeScalingFactor);
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