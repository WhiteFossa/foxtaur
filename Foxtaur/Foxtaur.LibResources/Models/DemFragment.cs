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

    /// <summary>
    /// Is fragment data loaded?
    /// </summary>
    public bool IsLoaded { get; private set; }

    /// <summary>
    /// Last time, when height was get from fragment
    /// </summary>
    public DateTime LastAccessTime { get; private set; }

    /// <summary>
    /// Size of cache, associated with fragment. 0 if fragment is not loaded
    /// </summary>
    public long CacheSize
    {
        get
        {
            return IsLoaded ? _reader.GetDataSize() : 0;
        }
    }
    
    public DemFragment(float northLat, float southLat, float westLon, float eastLon, List<ZoomLevel> zoomLevels, string resourceName, bool isLocal)
        : base(northLat, southLat, westLon, eastLon, zoomLevels, resourceName, isLocal)
    {
        LastAccessTime = DateTime.UtcNow;
    }

    public override async Task DownloadAsync(OnFragmentedResourceLoaded onLoad)
    {
        OnLoad = onLoad ?? throw new ArgumentNullException(nameof(onLoad));
        
        lock (this)
        {
            if (IsLoaded)
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
        IsLoaded = true;

        _logger.Info($"{ ResourceName } is ready.");
        OnLoad(this);
    }

    /// <summary>
    /// Unload fragment from memory
    /// </summary>
    public void Unload()
    {
        lock (this)
        {
            if (!IsLoaded)
            {
                _logger.Info($"Attempt to unload not loaded fragment: { ResourceName }!");   
                throw new InvalidOperationException($"Attempt to unload not loaded fragment: { ResourceName }!");
            }
            
            _logger.Info($"Unloading { ResourceName }...");

            IsLoaded = false;
            _reader = null;
            
            _logger.Info($"Unloaded { ResourceName }...");
        }
    }
    
    /// <summary>
    /// Get height by geocoodrinates
    /// </summary>
    public float? GetHeight(float lat, float lon)
    {
        LastAccessTime = DateTime.UtcNow;
        
        if (!IsLoaded)
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