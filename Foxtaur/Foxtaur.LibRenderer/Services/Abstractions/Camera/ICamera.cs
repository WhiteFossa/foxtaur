using System.Numerics;
using Foxtaur.LibGeo.Models;

namespace Foxtaur.LibRenderer.Services.Abstractions.Camera;

public class OnZoomChangedArgs
{
    public double Zoom { get; private set; }

    public OnZoomChangedArgs(double zoom)
    {
        Zoom = zoom;
    }
}

/// <summary>
/// Camera
/// </summary>
public interface ICamera
{
    /// <summary>
    /// Camera latitude
    /// </summary>
    double Lat { get; set; }

    /// <summary>
    /// Camera longitude
    /// </summary>
    double Lon { get; set; }

    /// <summary>
    /// Camera height (over the center of Earth)
    /// </summary>
    double H { get; set; }

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
    double Zoom { get; set; }

    /// <summary>
    /// If true, then we are in surface run mode, so special limits for zoom are applied
    /// </summary>
    bool IsSurfaceRunMode { get; set; }

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
    /// Forward projection matrix (for projection from 3D space to screen)
    /// </summary>
    Matrix4x4 ForwardProjectionMatrix { get; }

    /// <summary>
    /// Matrix for raycasting
    /// </summary>
    Matrix4x4 BackProjectionMatrix { get; }

    /// <summary>
    /// Screen aspect ratio
    /// </summary>
    double AspectRatio { get; set; }

    /// <summary>
    /// Camera up vector
    /// </summary>
    Vector3 Up { get; set; }
    
    /// <summary>
    /// Called when zoom changed
    /// </summary>
    delegate void OnZoomChangedHandler(object sender, OnZoomChangedArgs args);
    
    /// <summary>
    /// Event for zoom change
    /// </summary>
    event OnZoomChangedHandler OnZoomChanged;

    /// <summary>
    /// Zoom in
    /// </summary>
    void ZoomIn(double steps);

    /// <summary>
    /// Zoom out
    /// </summary>
    void ZoomOut(double steps);

    /// <summary>
    /// Project point from 3D space to viewport coordinates [-1; 1]
    /// </summary>
    PlanarPoint2D ProjectPointToViewport(PlanarPoint3D point);

    /// <summary>
    /// Project geo segment to planar segment (in viewport coordinates)
    /// </summary>
    PlanarSegment ProjectSegmentToViewport(GeoSegment segment);

    /// <summary>
    /// Is point on camera's side of Earth (for culling). Earth divided by the plane, passing through
    /// underground point
    /// </summary>
    bool IsPointOnCameraSideOfEarth(PlanarPoint3D undergroundPoint, PlanarPoint3D point);
}