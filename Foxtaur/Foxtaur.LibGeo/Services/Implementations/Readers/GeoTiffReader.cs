using Foxtaur.LibGeo.Models;
using Foxtaur.LibGeo.Services.Abstractions.Readers;
using OSGeo.GDAL;

namespace Foxtaur.LibGeo.Services.Implementations.Readers;

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
    /// Raster data
    /// </summary>
    private byte[][] _rasters;
    
    public GeoTiffReader()
    {
        Gdal.AllRegister(); // Registering GDAL drivers
    }
    
    public void Open(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException(nameof(path));
        }
        
        _dataset = Gdal.Open(path, Access.GA_ReadOnly);
        _ = _dataset ?? throw new InvalidOperationException($"Can't open { path }");
        
        var driver = _dataset.GetDriver();
        _ = driver ?? throw new InvalidOperationException($"Can't get driver for { path }");
        
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
            throw new NotSupportedException("Only byte and int16 datatypes are supported");
        }
        
        _rasterSize = _dataset.RasterXSize * _dataset.RasterYSize * _bytesPerPixel;

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
                _dataset.RasterXSize,
                _dataset.RasterYSize,
                (IntPtr)bufferPtr,
                _dataset.RasterXSize,
                _dataset.RasterYSize,
                DataType.GDT_Int16,
                0,
                0);    
        }
        
        if (result != CPLErr.CE_None)
        {
            throw new InvalidOperationException($"Failed to read band { band }");
        }
    }

    public float GetPixel(int band, int x, int y)
    {
        _ = _dataset ?? throw new InvalidOperationException("File not opened!");

        if (band < 0 || band > _dataset.RasterCount)
        {
            throw new ArgumentException(nameof(band));
        }

        if (x < 0 || x >= _dataset.RasterXSize || y < 0 || y > _dataset.RasterYSize)
        {
            throw new ArgumentException("Incorrect coordinates");
        }
        
        var bandIndex = band - 1;
        if (_bytesPerPixel == 1)
        {
            return _rasters[bandIndex][y * _dataset.RasterXSize + x] / (float)byte.MaxValue;
        }
        else if (_bytesPerPixel == 2)
        {
            var baseIndex = 2 * (y * _dataset.RasterXSize + x);
            
            var higherByte = _rasters[bandIndex][baseIndex + 1];
            var lowerByte = _rasters[bandIndex][baseIndex];

            var result = BitConverter.ToInt16(new byte[] { lowerByte, higherByte }, 0) / (float)Int16.MaxValue;

            return result;
        }
        else
        {
            throw new NotSupportedException("Only byte and int16 datatypes are supported");
        }
    }

    public int GetWidth()
    {
        _ = _dataset ?? throw new InvalidOperationException("File not opened!");
        return _dataset.RasterXSize;
    }

    public int GetHeight()
    {
        _ = _dataset ?? throw new InvalidOperationException("File not opened!");
        return _dataset.RasterYSize;
    }

    public float GetPixel(int band, GeoPoint coords)
    {
        throw new NotImplementedException();
    }
}