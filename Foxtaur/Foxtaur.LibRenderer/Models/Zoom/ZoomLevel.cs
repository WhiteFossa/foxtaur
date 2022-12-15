namespace Foxtaur.LibRenderer.Models.Zoom;

/// <summary>
/// Information about zoom level
/// </summary>
public class ZoomLevel
{
    /// <summary>
    /// Level
    /// </summary>
    public LibResources.Enums.ZoomLevel Level { get; }

    /// <summary>
    /// Minimal zoom for this level
    /// </summary>
    public double MinZoom { get; }

    /// <summary>
    /// Maximal zoom for this level
    /// </summary>
    public double MaxZoom { get; }

    /// <summary>
    /// Earth segment size
    /// </summary>
    public double SegmentSize { get; }

    /// <summary>
    /// All meshes MUST use this step (to provide consistent DEM)
    /// </summary>
    public double MeshesStep { get; }

    public ZoomLevel(LibResources.Enums.ZoomLevel level, double minZoom, double maxZoom, double segmentSize, double meshesStep)
    {
        Level = level;
        MinZoom = minZoom;
        MaxZoom = maxZoom;
        SegmentSize = segmentSize;
        MeshesStep = meshesStep;
    }
}