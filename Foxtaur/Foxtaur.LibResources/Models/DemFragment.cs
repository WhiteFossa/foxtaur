using Foxtaur.LibResources.Constants;
using Foxtaur.LibResources.Services.Abstractions.Readers;
using Foxtaur.LibResources.Services.Implementations.Readers;

namespace Foxtaur.LibResources.Models;

/// <summary>
/// Digital Elevation Map fragment
/// </summary>
public class DemFragment : FragmentedResourceBase
{
    private IGeoTiffReader _reader;

    private bool _isDownloaded;

    public DemFragment(float northLat, float southLat, float westLon, float eastLon, string resourceName, bool isLocal)
        : base(northLat, southLat, westLon, eastLon, resourceName, isLocal)
    {
    }

    public override string GetLocalPath()
    {
        return ResourcesConstants.LocalDemFragmentsDirectory + Path.DirectorySeparatorChar + ResourceName;
    }

    public override async Task DownloadAsync()
    {
        if (_isDownloaded)
        {
            // Fragment already downloaded
            return;
        }

        if (!IsLocal)
        {
            // TODO: Put download code here
        }

        // Now file is downloaded, we are ready to process
        _reader = new GeoTiffReader();
        _reader.Open(GetLocalPath());

        _isDownloaded = true;
    }

    /// <summary>
    /// Get height by geocoodrinates
    /// </summary>
    public float? GetHeight(float lat, float lon)
    {
        if (!_isDownloaded)
        {
            throw new InvalidOperationException($"Download {ResourceName} first!");
        }

        var height =  _reader.GetPixelByGeoCoords(ResourcesConstants.DemBand, lat, lon);
        
        // If we have "no data", then it's sea level
        if (height < ResourcesConstants.DemNoData)
        {
            height = ResourcesConstants.DemSeaLevel;
        }

        return height;
    }
}