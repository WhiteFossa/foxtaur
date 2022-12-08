using Foxtaur.LibResources.Services.Abstractions.Readers;
using Foxtaur.LibResources.Services.Implementations.Readers;
using ImageMagick;
using NLog;

namespace Foxtaur.LibResources.Models.HighResMap;

/// <summary>
/// High resolution map fragment (high resolution map always contains one fragment)
/// </summary>
public class HighResMapFragment : FragmentedResourceBase
{
    private Logger _logger = LogManager.GetCurrentClassLogger();
    
    private IGeoTiffReader _reader;

    private bool _isLoading;

    /// <summary>
    /// Is fragment data loaded?
    /// </summary>
    public bool IsLoaded { get; private set; }

    public HighResMapFragment(float northLat, float southLat, float westLon, float eastLon, string resourceName, bool isLocal)
        : base(northLat, southLat, westLon, eastLon, resourceName, isLocal)
    {
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
        
        _logger.Info($"Loading map { ResourceName }...");

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
            _logger.Info($"Decompressing map { ResourceName }...");
            using (var decompressedStream = LoadZstdFile(GetLocalPath()))
            {
                // Processing
                _logger.Info($"Processing map { ResourceName }...");
                
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

        _logger.Info($"Map { ResourceName } is ready.");
        OnLoad(this);
    }

    /// <summary>
    /// Get map as image (e.g. for use as a texture)
    /// </summary>
    public MagickImage GetImage()
    {
        if (!IsLoaded)
        {
            return null;
        }
        
        var image = new MagickImage(new MagickColor(0x00, 0x00, 0x00, 0x00), _reader.GetWidth(), _reader.GetHeight());
        
        // Loading image
        var pixels = image.GetPixels();
        for (var y = 0; y < _reader.GetHeight(); y++)
        {
            for (var x = 0; x < _reader.GetWidth(); x++)
            {
                var r = (byte)(_reader.GetPixel(0, x, y) * 255);
                var g = (byte)(_reader.GetPixel(1, x, y) * 255);
                var b = (byte)(_reader.GetPixel(2, x, y) * 255);
                
                pixels.SetPixel(x, y, new byte[] { r, g, b, 0xFF });
            }
        }

        return image;
    }
}