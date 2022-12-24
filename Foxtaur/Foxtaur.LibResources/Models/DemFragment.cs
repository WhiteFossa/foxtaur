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

    private Mutex _downloadLock = new Mutex();

    /// <summary>
    /// Is fragment data loading in progress?
    /// </summary>
    public bool IsLoading { get; private set; }
    
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
    
    public DemFragment(double northLat, double southLat, double westLon, double eastLon, ZoomLevel[] zoomLevels, string resourceName, bool isLocal)
        : base(northLat, southLat, westLon, eastLon, zoomLevels, resourceName, isLocal)
    {
        LastAccessTime = DateTime.UtcNow;
    }

    public override void Download(OnFragmentedResourceLoaded onLoad)
    {
        _downloadLock.WaitOne();

        try
        {
            OnLoad = onLoad ?? throw new ArgumentNullException(nameof(onLoad));

            if (IsLoaded)
            {
                // Fragment already downloaded
                return;
            }

            if (IsLoading)
            {
                // Loading in progress
                return;
            }

            IsLoading = true;

            _logger.Info($"Loading {ResourceName}...");

            if (!IsLocal)
            {
                // Do we have already downloaded file?
                var localPath = GetResourceLocalPath(ResourceName);
                if (!File.Exists(localPath))
                {
                    LoadFromUrlToFile(ResourceName);
                }
            }

            // Decompressing
            _logger.Info($"Decompressing {ResourceName}...");
            using (var decompressedStream = LoadZstdFile(GetLocalPath()))
            {
                // Processing
                _logger.Info($"Processing {ResourceName}...");

                _reader = new GeoTiffReader();
                _reader.Open(decompressedStream);
            }

            IsLoading = false;
            IsLoaded = true;

            _logger.Info($"{ResourceName} is ready.");
            OnLoad(this);

        }
        catch (Exception ex)
        {
            // Something went wrong, most probably network error during download
            _logger.Error($"Error during {ResourceName} download. Exception: { ex.Message }");
            
            IsLoading = false;
            IsLoaded = false;
        }
        finally
        {
            _downloadLock.ReleaseMutex();
        }
    }

    /// <summary>
    /// Unload fragment from memory
    /// </summary>
    public void Unload()
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
    
    /// <summary>
    /// Get height by geocoodrinates
    /// </summary>
    public double? GetHeight(double lat, double lon)
    {
        LastAccessTime = DateTime.UtcNow;

        if (!IsLoaded)
        {
            return ResourcesConstants.DemSeaLevel;
        }

        var height = _reader.GetPixelByGeoCoords(ResourcesConstants.DemBand, lat, lon);

        // If we have "no data", then it's sea level
        if (height < ResourcesConstants.DemNoData)
        {
            height = ResourcesConstants.DemSeaLevel;
        }

        return height;
    }
}