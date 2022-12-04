using Foxtaur.LibResources.Enums;

namespace Foxtaur.LibResources.Models;

/// <summary>
/// As fragmented resource base, but with zoom level
/// </summary>
public class ZoomedFragmentedResourceBase : FragmentedResourceBase
{
    /// <summary>
    /// Zoom level
    /// </summary>
    public ZoomLevel ZoomLevel { get; private set; }

    public ZoomedFragmentedResourceBase(float northLat,
        float southLat,
        float westLon,
        float eastLon,
        ZoomLevel zoomLevel,
        string resourceName,
        bool isLocal) : base(northLat, southLat, westLon, eastLon, resourceName, isLocal)
    {
        ZoomLevel = zoomLevel;
    }

    public override Task DownloadAsync(OnFragmentedResourceLoaded onLoad)
    {
        throw new NotImplementedException("Implement me in children!");
    }
}