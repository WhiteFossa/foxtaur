using System.Numerics;
using Foxtaur.LibRenderer.Models;

namespace Foxtaur.LibRenderer.Services.Abstractions.Camera;

/// <summary>
/// Camera
/// </summary>
public interface ICamera
{
    /// <summary>
    /// Camera latitude
    /// </summary>
    float Lat { get; set; }
    
    /// <summary>
    /// Camera longitude
    /// </summary>
    float Lon { get; set; }

    /// <summary>
    /// Camera height (over the center of Earth)
    /// </summary>
    float H { get; set; }

    /// <summary>
    /// Camera zoom (in radians)
    /// </summary>
    float Zoom { get; set; }

    /// <summary>
    /// Get camera position in 3D space
    /// </summary>
    PlanarPoint3D GetCameraPosition();

    /// <summary>
    /// Get camera position in 3D space (as Vector3)
    /// </summary>
    /// <returns></returns>
    Vector3 GetCameraPositionAsVector();

    /// <summary>
    /// Zoom in
    /// </summary>
    void ZoomIn(float steps);

    /// <summary>
    /// Zoom out
    /// </summary>
    void ZoomOut(float steps);
}