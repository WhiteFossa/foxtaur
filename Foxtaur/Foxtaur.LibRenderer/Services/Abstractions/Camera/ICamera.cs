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
    /// Camera position for given geo coordinates
    /// </summary>
    PlanarPoint3D Position3D { get; }
    
    /// <summary>
    /// Point where camera looking
    /// </summary>
    PlanarPoint3D Target { get; set; }

    /// <summary>
    /// Camera zoom (in radians)
    /// </summary>
    float Zoom { get; set; }

    /// <summary>
    /// Model matrix
    /// </summary>
    Matrix4x4 ModelMatrix { get; }

    /// <summary>
    /// View matrix
    /// </summary>
    Matrix4x4 ViewMatrix { get; }

    /// <summary>
    /// Projection matrix
    /// </summary>
    Matrix4x4 ProjectionMatrix { get; }

    /// <summary>
    /// Matrix for raycasting
    /// </summary>
    Matrix4x4 BackProjectionMatrix { get; }

    /// <summary>
    /// Screen aspect ratio
    /// </summary>
    float AspectRatio { get; set; }

    /// <summary>
    /// Zoom in
    /// </summary>
    void ZoomIn(float steps);

    /// <summary>
    /// Zoom out
    /// </summary>
    void ZoomOut(float steps);
}