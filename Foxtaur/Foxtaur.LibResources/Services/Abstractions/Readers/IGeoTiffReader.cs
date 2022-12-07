namespace Foxtaur.LibResources.Services.Abstractions.Readers;

/// <summary>
/// GeoTIFF reader
/// </summary>
public interface IGeoTiffReader
{
    /// <summary>
    /// Open file
    /// </summary>
    void Open(string path);

    /// <summary>
    /// Open stream
    /// </summary>
    void Open(Stream stream);

    /// <summary>
    /// Get pixel by planar coordinates. Result is normalized to [0; 1]
    /// </summary>
    float GetPixel(int band, int x, int y);

    /// <summary>
    /// Get pixel by planar coordinates (with bilinear interpolation). Result is normalized to [0; 1]
    /// </summary>
    float GetPixelWithInterpolation(int band, float x, float y); 

    /// <summary>
    /// Get image width
    /// </summary>
    int GetWidth();

    /// <summary>
    /// Get image height
    /// </summary>
    /// <returns></returns>
    int GetHeight();

    /// <summary>
    /// Get data size, occupied by reader (coarse)
    /// </summary>
    long GetDataSize();

    /// <summary>
    /// Get pixel by geocoordinates
    /// If geocoordinates are outside image will return null
    /// </summary>
    float? GetPixelByGeoCoords(int band, float lat, float lon);
}