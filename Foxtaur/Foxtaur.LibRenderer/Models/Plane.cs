using Foxtaur.LibGeo.Models;
using MathNet.Numerics.LinearAlgebra;

namespace Foxtaur.LibRenderer.Models;

/// <summary>
/// Plane in 3D space
/// </summary>
public class Plane
{
    /// <summary>
    /// Plane equation's A
    /// </summary>
    public double A { get; private set; }

    /// <summary>
    /// Plane equation's B
    /// </summary>
    public double B { get; private set; }

    /// <summary>
    /// Plane equation's C
    /// </summary>
    public double C { get; private set; }

    /// <summary>
    /// Construct plane by 3 points on it
    /// </summary>
    public Plane(PlanarPoint3D p1, PlanarPoint3D p2, PlanarPoint3D p3)
    {
        _ = p1 ?? throw new ArgumentNullException(nameof(p1));
        _ = p2 ?? throw new ArgumentNullException(nameof(p2));
        _ = p3 ?? throw new ArgumentNullException(nameof(p3));

        // Solving plane equations
        var p = Matrix<double>.Build.DenseOfArray(new double[,]
        {
            { p1.X, p1.Y, p1.Z },
            { p2.X, p2.Y, p2.Z },
            { p3.X, p3.Y, p3.Z }
        });

        var r = Vector<double>.Build.Dense(new double[] { -1.0, -1.0, -1.0 });

        var s = p.Solve(r);

        A = s[0];
        B = s[1];
        C = s[2];
    }
}