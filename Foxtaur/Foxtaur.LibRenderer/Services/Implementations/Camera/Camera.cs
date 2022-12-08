using System.Numerics;
using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibGeo.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibGeo.Services.Implementations.CoordinateProviders;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Helpers;
using Foxtaur.LibRenderer.Services.Abstractions.Camera;

namespace Foxtaur.LibRenderer.Services.Implementations.Camera;

public class Camera : ICamera
{
    private ICoordinatesProvider _sphereCoordinatesProvider = new SphereCoordinatesProvider();

    /// <summary>
    /// Camera latitude
    /// </summary>
    private float _lat;

    /// <summary>
    /// Camera longitude
    /// </summary>
    private float _lon;

    /// <summary>
    /// Camera height
    /// </summary>
    private float _h;

    /// <summary>
    /// Camera position in 3D space
    /// </summary>
    private PlanarPoint3D _position = new PlanarPoint3D(0, 0, 0);

    /// <summary>
    /// Camera target
    /// </summary>
    private PlanarPoint3D _target = new PlanarPoint3D(0, 0, 0);

    /// <summary>
    /// Camera zoom
    /// </summary>
    private float _zoom;

    /// <summary>
    /// Camera up vector
    /// </summary>
    private Vector3 _up;

    /// <summary>
    /// Camera aspect ratio
    /// </summary>
    private float _aspectRatio;

    public float Lat
    {
        get { return _lat; }

        set
        {
            _lat = value;

            CalculateCameraPosition();
        }
    }

    public float Lon
    {
        get { return _lon; }

        set
        {
            _lon = value;

            CalculateCameraPosition();
        }
    }

    public float H
    {
        get { return _h; }

        set
        {
            if (value < GeoConstants.EarthRadius)
            {
                _h = GeoConstants.EarthRadius;
            }

            _h = value;

            CalculateCameraPosition();
        }
    }

    public PlanarPoint3D Position3D
    {
        get { return _position; }

        set
        {
            _position = value;

            CalculateMatrices();
        }
    }

    public PlanarPoint3D Target
    {
        get { return _target; }

        set
        {
            _target = value;

            CalculateMatrices();
        }
    }

    public float Zoom
    {
        get { return _zoom; }

        set
        {
            if (value < RendererConstants.CameraMaxZoom)
            {
                _zoom = RendererConstants.CameraMaxZoom;
            }
            else if (value > RendererConstants.CameraMinZoom)
            {
                _zoom = RendererConstants.CameraMinZoom;
            }
            else
            {
                _zoom = value;
            }

            CalculateMatrices();
            
            OnZoomChanged?.Invoke(this, new OnZoomChangedArgs(_zoom));
        }
    }

    public Matrix4x4 ModelMatrix { get; private set; } = new Matrix4x4();

    public Matrix4x4 ViewMatrix { get; private set; } = new Matrix4x4();

    public Matrix4x4 ProjectionMatrix { get; private set; } = new Matrix4x4();

    public Matrix4x4 ForwardProjectionMatrix { get; private set; } = new Matrix4x4();
    
    public Matrix4x4 BackProjectionMatrix { get; private set; } = new Matrix4x4();

    public float AspectRatio
    {
        get { return _aspectRatio; }

        set
        {
            _aspectRatio = value;

            CalculateMatrices();
        }
    }

    public Vector3 Up
    {
        get { return _up; }

        set
        {
            _up = value;

            CalculateMatrices();
        }
    }

    public Camera(ISphereCoordinatesProvider sphereCoordinatesProvider)
    {
        _sphereCoordinatesProvider = sphereCoordinatesProvider;

        _zoom = RendererConstants.CameraMinZoom; // To avoid errors during initialization

        AspectRatio = 1.0f;
        Up = new Vector3(0.0f, -1.0f, 0.0f);
        Zoom = RendererConstants.CameraMinZoom;

        CalculateCameraPosition();
        CalculateMatrices();
    }

    public event ICamera.OnZoomChangedHandler OnZoomChanged;

    public void ZoomIn(float steps)
    {
        Zoom = Zoom * (float)Math.Pow(RendererConstants.CameraZoomInMultiplier, steps);
    }

    public void ZoomOut(float steps)
    {
        Zoom = Zoom * (float)Math.Pow(RendererConstants.CameraZoomOutMultiplier, steps);
    }

    public PlanarPoint2D ProjectPointToViewport(PlanarPoint3D point)
    {
        _ = point ?? throw new ArgumentNullException(nameof(point));

        var projectedPoint = Vector4.Transform(new Vector4(point.X, point.Y, point.Z, 1.0f), ForwardProjectionMatrix);
        
        return new PlanarPoint2D(projectedPoint.X, projectedPoint.Y);
    }

    public PlanarSegment ProjectSegmentToViewport(GeoSegment segment)
    {
        _ = segment ?? throw new ArgumentNullException(nameof(segment));
        
        var gp1 = new GeoPoint(segment.NorthLat, segment.WestLon, GeoConstants.EarthRadius);
        var gp3 = new GeoPoint(segment.SouthLat, segment.EastLon, GeoConstants.EarthRadius);
        
        // To 3D
        var p1 = _sphereCoordinatesProvider.GeoToPlanar3D(gp1);
        var p3 = _sphereCoordinatesProvider.GeoToPlanar3D(gp3);
        
        // To viewport
        var vp1 = ProjectPointToViewport(p1);
        var vp3 = ProjectPointToViewport(p3);

        return new PlanarSegment(Math.Max(vp1.Y, vp3.Y), Math.Min(vp1.X, vp3.X), Math.Min(vp1.Y, vp3.Y), Math.Max(vp1.X, vp3.X));
    }

    public bool IsPointOnCameraSideOfEarth(PlanarPoint3D undergroundPoint, PlanarPoint3D point)
    {
        _ = point ?? throw new ArgumentNullException(nameof(point));
        _ = undergroundPoint ?? throw new ArgumentNullException(nameof(undergroundPoint));

        var normalizedCameraVector = Vector3.Normalize(new Vector3(Position3D.X, Position3D.Y, Position3D.Z));

        var planeEquationSolution = normalizedCameraVector.X * (point.X - undergroundPoint.X)
                            + normalizedCameraVector.Y * (point.Y - undergroundPoint.Y)
                            + normalizedCameraVector.Z * (point.Z - undergroundPoint.Z);

        return planeEquationSolution > 0;
    }

    private void CalculateCameraPosition()
    {
        Position3D = _sphereCoordinatesProvider.GeoToPlanar3D(new GeoPoint(Lat, Lon, H));
    }

    /// <summary>
    /// Call on next changes:
    /// - Position3D
    /// - Target
    /// - Up
    /// - Zoom
    /// - AspectRatio
    /// </summary>
    private void CalculateMatrices()
    {
        ModelMatrix = Matrix4x4.CreateRotationZ(0.0f) * Matrix4x4.CreateRotationY(0.0f) *
                      Matrix4x4.CreateRotationX(0.0f); // Rotation
        ViewMatrix = Matrix4x4.CreateLookAt(Position3D.AsVector3(), Target.AsVector3(), Up); // Camera position
        ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(Zoom, AspectRatio, 0.01f, 100.0f); // Zoom
        
        ForwardProjectionMatrix = ModelMatrix * ViewMatrix * ProjectionMatrix;
        
        // Back-projection matrix (for raycasting)
        var forwardProjection = ViewMatrix * ProjectionMatrix;
        
        Matrix4x4 backProjection;
        if (!Matrix4x4.Invert(forwardProjection, out backProjection))
        {
            throw new InvalidOperationException("Failed to invert forward projection!");
        }

        BackProjectionMatrix = backProjection;
    }
}