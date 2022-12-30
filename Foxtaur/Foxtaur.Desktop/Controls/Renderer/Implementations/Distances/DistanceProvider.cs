using System;
using System.Threading;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Distances;
using Foxtaur.Desktop.Controls.Renderer.Abstractions.Generators;
using Foxtaur.Desktop.Controls.Renderer.Models;
using Foxtaur.LibResources.Models;
using Foxtaur.LibResources.Models.HighResMap;
using Foxtaur.LibWebClient.Models;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer.Implementations.Distances;

public class DistanceProvider : IDistanceProvider
{
    private readonly IMapSegmentGenerator _mapSegmentGenerator;
    
    private Distance _activeDistance;
    private HighResMapFragment _mapFragment;
    private HighResMap _map;
    private MapSegment _mapSegment;
    private bool _isDistanceLoaded;

    public DistanceProvider(IMapSegmentGenerator mapSegmentGenerator)
    {
        _mapSegmentGenerator = mapSegmentGenerator;
    }
    
    public void SetActiveDistance(Distance distance)
    {
        // Disposing stuff
        _isDistanceLoaded = false;

        _activeDistance = distance ?? throw new ArgumentNullException(nameof(distance));

        _mapFragment = new HighResMapFragment(distance.Map.NorthLat,
            distance.Map.SouthLat,
            distance.Map.WestLon,
            distance.Map.EastLon,
            distance.Map.Url,
            false);
        
        _map = new HighResMap(distance.Map.Id, distance.Map.Name, _mapFragment);
        
        // Starting to load map
        var mapLoadThread = new Thread(() => _mapFragment.Download(OnMapLoaded));
        mapLoadThread.Start();
    }

    private void OnMapLoaded(FragmentedResourceBase fragment)
    {
        _isDistanceLoaded = true;
    }

    public bool IsDistanceSelected()
    {
        return _activeDistance != null;
    }

    public bool IsDistanceLoaded()
    {
        return _isDistanceLoaded;
    }

    public HighResMap GetMap()
    {
        return _map;
    }

    public void GenerateDistanceSegment(GL silkGlContext, double altitudeIncrement)
    {
        if (!_isDistanceLoaded)
        {
            // Not ready yet
            return;
        }
        
        if (_mapSegment == null)
        {
            _mapSegment = _mapSegmentGenerator.Generate(_map, altitudeIncrement);
            _mapSegment.UpdateTexture(new Texture(silkGlContext, _mapSegment.Map.Fragment.GetImage()));
            _mapSegment.Mesh.GenerateBuffers(silkGlContext);
        }
    }

    public void DisposeDistanceSegment()
    {
        if (_mapSegment == null)
        {
            return;
        }
        
        _mapSegment.Mesh.Dispose();
        _mapSegment.Texture.Dispose();

        _mapSegment = null;
    }

    public unsafe void DrawDistance(GL silkGlContext)
    {
        if (_mapSegment != null)
        {
            _mapSegment.Texture.Bind();
            _mapSegment.Mesh.BindBuffers(silkGlContext);
            
            silkGlContext.DrawElements(PrimitiveType.Triangles, (uint)_mapSegment.Mesh.Indices.Count, DrawElementsType.UnsignedInt, null);
        }
    }
}