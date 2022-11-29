using Foxtaur.LibResources.Constants;
using Foxtaur.LibResources.Services.Abstractions.Readers;
using Foxtaur.LibResources.Services.Implementations.Readers;
using NLog;

namespace Foxtaur.LibResources.Models;

/// <summary>
/// Digital Elevation Map fragment
/// </summary>
public class DemFragment : FragmentedResourceBase
{
    /// <summary>
    /// Logger
    /// </summary>
    private Logger _logger = LogManager.GetCurrentClassLogger();
    
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

    public override async Task DownloadAsync(OnFragmentedResourceLoaded onLoad)
    {
        _onLoad = onLoad ?? throw new ArgumentNullException(nameof(onLoad));
        
        if (_isDownloaded)
        {
            // Fragment already downloaded
            return;
        }
        
        _logger.Info($"Loading { ResourceName }...");

        if (!IsLocal)
        {
            // TODO: Put download code here
        }

        // Now file is downloaded, we are ready to process
        _logger.Info($"Processing { ResourceName }...");
        _reader = new GeoTiffReader();
        _reader.Open(GetLocalPath());

        _isDownloaded = true;

        _logger.Info($"{ ResourceName } is ready.");
        _onLoad(this);
    }

    /// <summary>
    /// Get height by geocoodrinates
    /// </summary>
    public float? GetHeight(float lat, float lon)
    {
        if (!_isDownloaded)
        {
            return ResourcesConstants.DemSeaLevel;
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