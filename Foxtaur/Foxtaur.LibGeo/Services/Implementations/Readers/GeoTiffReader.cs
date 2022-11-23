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
        var firstBandPixelDataType = _dataset.GetRasterBand(1).DataType;
        for (var band = 2; band <= _dataset.RasterCount; band++)
        {
            if (_dataset.GetRasterBand(band).DataType != firstBandPixelDataType)
            {
                throw new NotSupportedException("All bands have to be of the same type");
            }
        }

        if (firstBandPixelDataType == DataType.GDT_Byte)
        {
            _bytesPerPixel = 1;
        }
        else if (firstBandPixelDataType == DataType.GDT_Int16)
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

    private void LoadBand(int band)
    {
        if (band < 0 || band > _dataset.RasterCount)
        {
            throw new ArgumentException(nameof(band));
        }
        
        var dataBand = _dataset.GetRasterBand(band);
        var effectiveBand = band - 1;
        var result = dataBand.ReadRaster(0, 0, _dataset.RasterXSize, _dataset.RasterYSize, _rasters[effectiveBand], _dataset.RasterXSize, _dataset.RasterYSize, 0, 0);
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
            return _rasters[bandIndex][y * _dataset.RasterXSize + x] / 255.0f;
        }
        else if (_bytesPerPixel == 2)
        {
            var baseIndex = y * _dataset.RasterXSize + x;
            
            var higherByte = _rasters[bandIndex][baseIndex + _dataset.RasterXSize];
            var lowerByte = _rasters[bandIndex][baseIndex + x];

            return (higherByte * 256 + lowerByte) / 65535.0f;
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