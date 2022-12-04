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
    public float MinZoom { get; }

    /// <summary>
    /// Maximal zoom for this level
    /// </summary>
    public float MaxZoom { get; }

    /// <summary>
    /// Earth segment size
    /// </summary>
    public float SegmentSize { get; }

    /// <summary>
    /// Each Earth segments will have SegmentSteps ^ 2 vertices
    /// </summary>
    public float SegmentSteps { get; }

    public ZoomLevel(LibResources.Enums.ZoomLevel level, float minZoom, float maxZoom, float segmentSize, float segmentSteps)
    {
        Level = level;
        MinZoom = minZoom;
        MaxZoom = maxZoom;
        SegmentSize = segmentSize;
        SegmentSteps = segmentSteps;
    }
}