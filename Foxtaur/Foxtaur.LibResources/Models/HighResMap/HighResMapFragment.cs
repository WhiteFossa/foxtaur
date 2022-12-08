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

    private MagickImage _image;

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
                
                _image = new MagickImage(new MagickColor(0x00, 0x00, 0x00, 0x00), _reader.GetWidth(), _reader.GetHeight());
        
                // Loading image
                var pixels = _image.GetPixels();
                var pixelData = new byte[4];
                pixelData[3] = 0xFF; // No transparency
        
                for (var y = 0; y < _reader.GetHeight(); y++)
                {
                    if (y % 100 == 0)
                    {
                        _logger.Info($"Processing map { ResourceName } line { y }...");    
                    }

                    for (var x = 0; x < _reader.GetWidth(); x++)
                    {
                        pixelData[0] = (byte)(_reader.GetPixel(1, x, y) * 255);
                        pixelData[1] = (byte)(_reader.GetPixel(2, x, y) * 255);
                        pixelData[2] = (byte)(_reader.GetPixel(3, x, y) * 255);
                
                        pixels.SetPixel(x, y, pixelData);
                    }
                }
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

        return _image;
    }
}