using Foxtaur.LibResources.Enums;

namespace Foxtaur.LibResources.Models;

/// <summary>
/// As fragmented resource base, but with zoom level
/// </summary>
public class ZoomedFragmentedResourceBase : FragmentedResourceBase
{
    /// <summary>
    /// Fragment provides data for those zoom levels
    /// </summary>
    public List<ZoomLevel> ZoomLevels { get; private set; }

    public ZoomedFragmentedResourceBase(double northLat,
        double southLat,
        double westLon,
        double eastLon,
        List<ZoomLevel> zoomLevels,
        string resourceName,
        bool isLocal) : base(northLat, southLat, westLon, eastLon, resourceName, isLocal)
    {
        ZoomLevels = zoomLevels;
    }

    public override Task DownloadAsync(OnFragmentedResourceLoaded onLoad)
    {
        throw new NotImplementedException("Implement me in children!");
    }
}