namespace Foxtaur.LibGeo.Models;

/// <summary>
///     Point with planar coordinates
/// </summary>
public class PlanarPoint3D : PlanarPoint2D
{
    /// <summary>
    ///     Planar Z
    /// </summary>
    public double Z { get; }

    public PlanarPoint3D(double x, double y, double z) : base(x, y)
    {
        Z = z;
    }

    /// <summary>
    /// Calculate distance from this point to a given point
    /// </summary>
    public double DistanceTo(PlanarPoint3D point)
    {
        _ = point ?? throw new ArgumentNullException(nameof(point));

        return Math.Sqrt(Math.Pow(point.X - X, 2) + Math.Pow(point.Y - Y, 2) + Math.Pow(point.Z - Z, 2));
    }

    /// <summary>
    /// Calculate distance from this point to a given point (coordinates overload)
    /// </summary>
    public double DistanceTo(double x, double y, double z)
    {
        return Math.Sqrt(Math.Pow(x - X, 2) + Math.Pow(y - Y, 2) + Math.Pow(z - Z, 2));
    }

    /// <summary>
    /// Returns closest point
    /// </summary>
    public PlanarPoint3D GetClosesPoint(IReadOnlyCollection<PlanarPoint3D> points)
    {
        _ = points ?? throw new ArgumentNullException(nameof(points));

        if (!points.Any())
        {
            throw new ArgumentException(nameof(points));
        }

        PlanarPoint3D closestPoint = null;
        var minDistance = double.MaxValue;

        foreach (var point in points)
        {
            var distance = DistanceTo(point);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = point;
            }
        }

        return closestPoint;
    }
}