using Foxtaur.LibResources.Models.HighResMap;
using Foxtaur.LibWebClient.Models;
using Silk.NET.OpenGL;

namespace Foxtaur.Desktop.Controls.Renderer.Abstractions.Distances;

/// <summary>
/// Provides everything, related to distance
/// </summary>
public interface IDistanceProvider
{
    /// <summary>
    /// Set active distance
    /// </summary>
    void SetActiveDistance(Distance distance);

    /// <summary>
    /// True if we have active distance
    /// </summary>
    /// <returns></returns>
    bool IsDistanceSelected();

    /// <summary>
    /// True if distance successfully loaded
    /// </summary>
    bool IsDistanceLoaded();
    
    /// <summary>
    /// Get active distance map
    /// </summary>
    /// <returns></returns>
    HighResMap GetMap();

    /// <summary>
    /// If distance is loaded this method call will generate distance segment, ready for drawing
    /// </summary>
    void GenerateDistanceSegment(GL silkGlContext, double altitudeIncrement);

    /// <summary>
    /// Dispose segment (new one can be generated then). If segment is not generated, do nothing
    /// </summary>
    void DisposeDistanceSegment();
    
    /// <summary>
    /// If distance is loaded this method will draw distance
    /// </summary>
    void DrawDistance(GL silkGlContext);
}