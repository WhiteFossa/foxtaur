namespace Foxtaur.LibRenderer.Models;

/// <summary>
///     Point with planar coordinates
/// </summary>
public class PlanarPoint3D : PlanarPoint2D
{
    /// <summary>
    ///     Planar Z
    /// </summary>
    public float Z { get; }

    public PlanarPoint3D(float x, float y, float z) : base(x, y)
    {
        Z = z;
    }

    /// <summary>
    /// Calculate distance from this point to a given point
    /// </summary>
    public float DistanceTo(PlanarPoint3D point)
    {
        _ = point ?? throw new ArgumentNullException(nameof(point));

        return (float)Math.Sqrt((float)Math.Pow(point.X - X, 2) + (float)Math.Pow(point.Y - Y, 2) +
                                (float)Math.Pow(point.Z - Z, 2));
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
        var minDistance = float.MaxValue;

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