using Foxtaur.LibGeo.Constants;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibGeo.Services.Abstractions.CoordinateProviders;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Helpers;
using Foxtaur.LibRenderer.Services.Abstractions.Camera;
using MathNet.Numerics.LinearAlgebra;

namespace Foxtaur.LibRenderer.Services.Implementations.Camera;

public class Camera : ICamera
{
    private ICoordinatesProvider _sphereCoordinatesProvider;

    /// <summary>
    /// Camera latitude
    /// </summary>
    private double _lat;

    /// <summary>
    /// Camera longitude
    /// </summary>
    private double _lon;

    /// <summary>
    /// Camera height
    /// </summary>
    private double _h;

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
    private double _zoom;

    /// <summary>
    /// Camera up vector
    /// </summary>
    private Vector<double> _up = Vector<double>.Build.DenseOfArray(new double[] { 0, 1, 0 });

    /// <summary>
    /// Camera aspect ratio
    /// </summary>
    private double _aspectRatio;

    public double Lat
    {
        get { return _lat; }

        set
        {
            _lat = value;

            CalculateCameraPosition();
        }
    }

    public double Lon
    {
        get { return _lon; }

        set
        {
            _lon = value;

            CalculateCameraPosition();
        }
    }

    public double H
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

    public double Zoom
    {
        get { return _zoom; }

        set
        {
            if (value < RendererConstants.CameraMaxZoom)
            {
                _zoom = RendererConstants.CameraMaxZoom;
            }
            else if (!IsSurfaceRunMode && value > RendererConstants.CameraMinZoom)
            {
                _zoom = RendererConstants.CameraMinZoom;
            }
            else if (IsSurfaceRunMode && value > RendererConstants.SurfaceRunMinZoom)
            {
                _zoom = RendererConstants.SurfaceRunMinZoom;
            }
            else
            {
                _zoom = value;
            }

            CalculateMatrices();
            
            OnZoomChanged?.Invoke(this, new OnZoomChangedArgs(_zoom));
        }
    }

    public bool IsSurfaceRunMode { get; set; }

    public Matrix<double> ModelMatrix { get; private set; } = Matrix<double>.Build.DenseIdentity(4, 4);

    public Matrix<double> ViewMatrix { get; private set; } = Matrix<double>.Build.DenseIdentity(4, 4);

    public Matrix<double> ProjectionMatrix { get; private set; } = Matrix<double>.Build.DenseIdentity(4, 4);

    public Matrix<double> ForwardProjectionMatrix { get; private set; } = Matrix<double>.Build.DenseIdentity(4, 4);
    
    public Matrix<double> BackProjectionMatrix { get; private set; } = Matrix<double>.Build.DenseIdentity(4, 4);

    public double AspectRatio
    {
        get { return _aspectRatio; }

        set
        {
            _aspectRatio = value;

            CalculateMatrices();
        }
    }

    public Vector<double> Up
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

        AspectRatio = 1.0;
        Up = Vector<double>.Build.DenseOfArray(new double[] { 0.0, -1.0, 0.0 });
        Zoom = RendererConstants.CameraMinZoom;

        CalculateCameraPosition();
        CalculateMatrices();
    }

    public event ICamera.OnZoomChangedHandler OnZoomChanged;

    public void ZoomIn(double steps)
    {
        Zoom = Zoom * Math.Pow(RendererConstants.CameraZoomInMultiplier, steps);
    }

    public void ZoomOut(double steps)
    {
        Zoom = Zoom * Math.Pow(RendererConstants.CameraZoomOutMultiplier, steps);
    }

    public PlanarPoint2D ProjectPointToViewport(PlanarPoint3D point)
    {
        _ = point ?? throw new ArgumentNullException(nameof(point));
        
        var projectedPoint = DoubleHelper.Transform(Vector<double>.Build.DenseOfArray(new double[] { point.X, point.Y, point.Z, 1.0}), ForwardProjectionMatrix);
        
        return new PlanarPoint2D(projectedPoint[0], projectedPoint[1]);
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
        
        var normalizedCameraVector = Position3D.AsVector().Normalize();

        var planeEquationSolution = normalizedCameraVector[0] * (point.X - undergroundPoint.X)
                            + normalizedCameraVector[1] * (point.Y - undergroundPoint.Y)
                            + normalizedCameraVector[2] * (point.Z - undergroundPoint.Z);

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
        ModelMatrix = DoubleHelper.CreateRotationZMatrix(0) * DoubleHelper.CreateRotationYMatrix(0) * DoubleHelper.CreateRotationXMatrix(0); // Rotation
        ViewMatrix = DoubleHelper.CreateLookAt(Position3D.AsVector(), Target.AsVector(), Up); // Camera position
        ProjectionMatrix = DoubleHelper.CreatePerspectiveFieldOfView(Zoom, AspectRatio, 0.00001, 10.0); // Zoom
        
        ForwardProjectionMatrix = ModelMatrix * ViewMatrix * ProjectionMatrix;
        
        // Back-projection matrix (for raycasting)
        var forwardProjection = ViewMatrix * ProjectionMatrix;

        BackProjectionMatrix = forwardProjection.Inverse();
    }
}