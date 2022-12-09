using Foxtaur.Helpers;
using Foxtaur.LibResources.Services.Abstractions.Readers;
using OSGeo.GDAL;

namespace Foxtaur.LibResources.Services.Implementations.Readers;

/// <summary>
///  GeoTIFF reader
/// </summary>
public class GeoTiffReader : IGeoTiffReader
{
    /// <summary>
    /// GeoTIFF dataset
    /// </summary>
    private Dataset _dataset;

    /// <summary>
    /// BYTES per pixel
    /// </summary>
    private int _bytesPerPixel;

    /// <summary>
    /// Pixel type
    /// </summary>
    private DataType _pixelType;

    /// <summary>
    /// Size of one raster in bytes
    /// </summary>
    private int _rasterSize;

    /// <summary>
    /// Total size of all rasters
    /// </summary>
    private long _totalRastersSize;
    
    /// <summary>
    /// Raster data
    /// </summary>
    private byte[][] _rasters;

    /// <summary>
    /// Coefficients for geolocation
    /// </summary>
    private double[] _geoCoefficients = new double[6];

    private double _geoK1;
    private double _geoK2;
    private double _geoK3;
    private double _geoK4;
    private double _geoK5;
    
    /// <summary>
    /// Raster width
    /// </summary>
    private int _width;

    /// <summary>
    /// Raster height
    /// </summary>
    private int _height;

    /// <summary>
    /// It seems that GDAL is not thread safe
    /// </summary>
    private static object _gdalLock = new object();

    static GeoTiffReader()
    {
        Gdal.AllRegister(); // Registering GDAL drivers
    }

    public void Open(string path)
    {
        lock (_gdalLock)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }

            _dataset = Gdal.Open(path, Access.GA_ReadOnly);
            _ = _dataset ?? throw new InvalidOperationException($"Can't open {path}");

            var driver = _dataset.GetDriver();
            _ = driver ?? throw new InvalidOperationException($"Can't get driver for {path}");

            // Allocating buffer spaces
            _pixelType = _dataset.GetRasterBand(1).DataType;
            for (var band = 2; band <= _dataset.RasterCount; band++)
            {
                if (_dataset.GetRasterBand(band).DataType != _pixelType)
                {
                    throw new NotSupportedException("All bands have to be of the same type");
                }
            }

            if (_pixelType == DataType.GDT_Byte)
            {
                _bytesPerPixel = 1;
            }
            else if (_pixelType == DataType.GDT_Int16)
            {
                _bytesPerPixel = 2;
            }
            else
            {
                throw new NotSupportedException("Open file: Only byte and int16 datatypes are supported");
            }

            _width = _dataset.RasterXSize;
            _height = _dataset.RasterYSize;
            
            _rasterSize = _width * _height * _bytesPerPixel;

            _totalRastersSize = _rasterSize * _dataset.RasterCount;
            
            _rasters = new byte[_dataset.RasterCount][];
            for (var band = 1; band <= _dataset.RasterCount; band++)
            {
                var bandIndex = band - 1; // Bands are counted from 1
                _rasters[bandIndex] = new byte[_rasterSize];
            }

            // Loading the rasters from all bands
            for (var band = 1; band <= _dataset.RasterCount; band++)
            {
                LoadBand(band);
            }

            // Loading geolocation
            _dataset.GetGeoTransform(_geoCoefficients);

            _geoK1 = _geoCoefficients[4] / _geoCoefficients[1];
            _geoK2 = _geoCoefficients[5] - _geoK1 * _geoCoefficients[2];
            _geoK3 = _geoK1 * _geoCoefficients[0];
            _geoK4 = _geoCoefficients[0] / _geoCoefficients[1];
            _geoK5 = _geoCoefficients[2] / _geoCoefficients[1];    
        }
    }

    public void Open(Stream stream)
    {
        _ = stream ?? throw new ArgumentNullException(nameof(stream));
        
        using(var memoryStream = new MemoryStream())
        {
            stream.CopyTo(memoryStream);
            var buffer = memoryStream.ToArray();
            
            // Virtual file
            var virtualFilename = $"/vsimem/{Guid.NewGuid()}.tif";
            Gdal.FileFromMemBuffer(virtualFilename, buffer);
            
            Open(virtualFilename);
            
            Gdal.Unlink(virtualFilename);
        }
    }

    private unsafe void LoadBand(int band)
    {
        if (band < 0 || band > _dataset.RasterCount)
        {
            throw new ArgumentException(nameof(band));
        }

        var dataBand = _dataset.GetRasterBand(band);
        var effectiveBand = band - 1;

        CPLErr result;
        fixed (void* bufferPtr = _rasters[effectiveBand])
        {
            result = dataBand.ReadRaster(0,
                0,
                _width,
                _height,
                (IntPtr)bufferPtr,
                _width,
                _height,
                _pixelType,
                0,
                0);
        }

        if (result != CPLErr.CE_None)
        {
            throw new InvalidOperationException($"Failed to read band {band}");
        }
    }

    public float GetPixel(int band, int x, int y)
    {
        // No params checks for speedup
        var bandIndex = band - 1;
        if (_bytesPerPixel == 1)
        {
            return _rasters[bandIndex][y * _width + x] / (float)byte.MaxValue;
        }
        else if (_bytesPerPixel == 2)
        {
            var baseIndex = 2 * (y * _width + x);

            var lowerByte = _rasters[bandIndex][baseIndex];
            var higherByte = _rasters[bandIndex][baseIndex + 1];

            return BitConverter.ToInt16(new byte[] { lowerByte, higherByte }, 0) / (float)UInt16.MaxValue + 0.5f;
        }
        else
        {
            throw new NotSupportedException("Get pixel: Only byte and int16 datatypes are supported");
        }
    }

    public float GetPixelWithInterpolation(int band, float x, float y)
    {
        var x1 = (int)x;
        var y1 = (int)y;

        if (x1 == _width - 1 || y1 == _height - 1)
        {
            // Edge pixel
            return GetPixel(band, x1, y1);
        }
        
        var x2 = x1 + 1;
        var y2 = y1 + 1;

        var p1 = GetPixel(band, x1, y1);
        var p2 = GetPixel(band, x2, y1);
        var p3 = GetPixel(band, x2, y2);
        var p4 = GetPixel(band, x1, y2);
        
        // y2 - y1 is always 1
        var k1 = y - y1;
        var k2 = y2 - y;

        var q1 = k1 * p4 + k2 * p1;
        var q2 = k1 * p3 + k2 * p2;
        
        // x2 - x1 is always 1
        var k3 = x - x1;
        var k4 = x2 - x;

        return q1 * k4 + q2 * k3;
    }

    public int GetWidth()
    {
        return _width;
    }

    public int GetHeight()
    {
        return _height;
    }

    public long GetDataSize()
    {
        return _totalRastersSize;
    }

    public Tuple<float, float> GetPixelCoordsByGeoCoords(float lat, float lon)
    {
        var latDegrees = lat.ToDegrees();
        var lonDegrees = -1.0f * lon.ToDegrees(); // GeoTIFF use negative west, we use negative east

        var y = (latDegrees - _geoCoefficients[3] + _geoK3 - _geoK1 * lonDegrees) / _geoK2;
        var x = lonDegrees / _geoCoefficients[1] - _geoK4 - _geoK5 * y;

        return new Tuple<float, float>((float)x, (float)y);
    }

    public float? GetPixelByGeoCoords(int band, float lat, float lon)
    {
        var planarCoords = GetPixelCoordsByGeoCoords(lat, lon);
        if (planarCoords == null)
        {
            return null;
        }

        return GetPixelWithInterpolation(band, planarCoords.Item1, planarCoords.Item2);
    }
}