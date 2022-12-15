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

    public HighResMapFragment(double northLat, double southLat, double westLon, double eastLon, string resourceName, bool isLocal)
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
                
                // Combined raster
                var width = _reader.GetWidth();
                var height = _reader.GetHeight();
                var raster = new byte[width * height * 4]; // 4 bytes per pixel - RGBA

                var pixelBaseIndex = 0;
                
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        raster[pixelBaseIndex + 0] = (byte)(_reader.GetPixel(1, x, y) * 255);
                        raster[pixelBaseIndex + 1] = (byte)(_reader.GetPixel(2, x, y) * 255);
                        raster[pixelBaseIndex + 2] = (byte)(_reader.GetPixel(3, x, y) * 255);
                        raster[pixelBaseIndex + 3] = 255; // No transparency

                        pixelBaseIndex += 4;
                    }
                }
                
                var readSettings = new MagickReadSettings();
                readSettings.ColorType = ColorType.TrueColorAlpha;
                readSettings.Width = width;
                readSettings.Height = height;
                readSettings.Format = MagickFormat.Rgba;
                
                _image = new MagickImage(raster, readSettings);
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

    /// <summary>
    /// Get texture coordinates by geo coordinates. May return null if coordinates are outside the map
    /// </summary>
    public Tuple<double, double> GetTextureCoordinates(double lat, double lon)
    {
        if (!IsLoaded)
        {
            return null;
        }

        var planarCoords = _reader.GetPixelCoordsByGeoCoords(lat, lon);
        if (planarCoords == null)
        {
            return null;
        }

        var x = planarCoords.Item1 / _reader.GetWidth();
        var y = planarCoords.Item2 / _reader.GetHeight();

        return new Tuple<double, double>(x, y);
    }
}