using Foxtaur.LibResources.Constants;
using Foxtaur.LibResources.Enums;
using Foxtaur.LibResources.Services.Abstractions.Readers;
using Foxtaur.LibResources.Services.Implementations.Readers;
using NLog;

namespace Foxtaur.LibResources.Models;

/// <summary>
/// Digital Elevation Map fragment
/// </summary>
public class DemFragment : ZoomedFragmentedResourceBase
{
    private Logger _logger = LogManager.GetCurrentClassLogger();
    
    private IGeoTiffReader _reader;

    private bool _isLoading;
    
    private bool _isLoaded;
    
    /// <summary>
    /// Is fragment loaded?
    /// </summary>
    public bool IsLoaded
    {
        get
        {
            return _isLoaded;
        }
    }

    private static object _processingLock = new object();

    public DemFragment(float northLat, float southLat, float westLon, float eastLon, List<ZoomLevel> zoomLevels, string resourceName, bool isLocal)
        : base(northLat, southLat, westLon, eastLon, zoomLevels, resourceName, isLocal)
    {
    }

    public override async Task DownloadAsync(OnFragmentedResourceLoaded onLoad)
    {
        OnLoad = onLoad ?? throw new ArgumentNullException(nameof(onLoad));
        
        lock (this)
        {
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
        }
        
        _logger.Info($"Loading { ResourceName }...");

        try
        {
            if (!IsLocal)
            {
                // Do we have already downloaded file?
                var localPath = GetResourceLocalPath(ResourceName);
                if (!File.Exists(localPath))
                {
                    await LoadFromUrlToFileAsync(ResourceName);    
                }
            }

            // Decompressing
            _logger.Info($"Decompressing { ResourceName }...");
            lock(_processingLock)
            {
                using (var decompressedStream = LoadZstdFile(GetLocalPath()))
                {
                    // Processing
                    _logger.Info($"Processing { ResourceName }...");
                    
                    _reader = new GeoTiffReader();
                    _reader.Open(decompressedStream);
                    
                    _logger.Info($"Processed { ResourceName }...");
                }
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
        OnLoad(this);
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