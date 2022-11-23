using Foxtaur.LibGeo.Models;

namespace Foxtaur.LibGeo.Services.Abstractions.Readers;

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
    /// Get pixel by planar coordinates. Result is normalized to [0; 1]
    /// </summary>
    float GetPixel(int band, int x, int y);

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
    /// Get pixel by geocoordinates. Height is ignored
    /// </summary>
    float GetPixel(int band, GeoPoint coords);
}