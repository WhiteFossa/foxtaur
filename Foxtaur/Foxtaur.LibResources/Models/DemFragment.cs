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

    private bool _isLoading;
    
    private bool _isLoaded;

    public DemFragment(float northLat, float southLat, float westLon, float eastLon, string resourceName, bool isLocal)
        : base(northLat, southLat, westLon, eastLon, resourceName, isLocal)
    {
    }

    public override async Task DownloadAsync(OnFragmentedResourceLoaded onLoad)
    {
        _onLoad = onLoad ?? throw new ArgumentNullException(nameof(onLoad));
        
        if (_isLoaded)
        {
            // Fragment already downloaded
            return;
        }
        
        if (_isLoading)
        {
            // Loading in progress
            return;
        }

        _isLoading = true;
        
        _logger.Info($"Loading { ResourceName }...");

        try
        {
            if (!IsLocal)
            {
                await LoadFromUrlToFileAsync(ResourceName);
            }

            // Decompressing
            _logger.Info($"Decompressing { ResourceName }...");
            using (var decompressedStream = LoadZstdFile(GetLocalPath()))
            {
                // Processing
                _logger.Info($"Processing { ResourceName }...");
                _reader = new GeoTiffReader();
                _reader.Open(decompressedStream);    
            }
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            throw;
        }

        _isLoading = false;
        _isLoaded = true;

        _logger.Info($"{ ResourceName } is ready.");
        _onLoad(this);
    }

    /// <summary>
    /// Get height by geocoodrinates
    /// </summary>
    public float? GetHeight(float lat, float lon)
    {
        if (!_isLoaded)
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