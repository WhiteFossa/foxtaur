using System.Numerics;
using Foxtaur.LibGeo.Helpers;
using Foxtaur.LibGeo.Models;
using Foxtaur.LibRenderer.Constants;
using Foxtaur.LibRenderer.Helpers;
using Foxtaur.LibRenderer.Services.Abstractions.Camera;

namespace Foxtaur.LibRenderer.Models;

/// <summary>
/// Ray in 3D space
/// </summary>
public class Ray
{
    /// <summary>
    /// Ray begin point
    /// </summary>
    public PlanarPoint3D Begin { get; private set; }

    /// <summary>
    /// Ray end point
    /// </summary>
    public PlanarPoint3D End { get; private set; }

    public Ray(PlanarPoint3D begin, PlanarPoint3D end)
    {
        Begin = begin;
        End = end;
    }

    /// <summary>
    /// Casts ray from screen to 3D space
    /// </summary>
    public static Ray CreateByScreenRaycasting(ICamera camera, double screenX, double screenY, double viewportWidth, double viewportHeight)
    {
        var normalizedDeviceX = screenX / viewportWidth * 2.0 - 1.0;
        var normalizedDeviceY = screenY / viewportHeight * 2.0 - 1.0;

        var nearPoint = camera.Position3D.AsVector();
        var farPoint = camera.BackProjectionMatrix.TransformPerspectively(MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray(new double[] { normalizedDeviceX, normalizedDeviceY, 1.0 }));

        return new Ray(nearPoint.AsPlanarPoint3D(), farPoint.AsPlanarPoint3D());
    }

    /// <summary>
    /// Intersect ray with the sphere
    /// </summary>
    public List<PlanarPoint3D> Intersect(Sphere sphere)
    {
        var p0 = Begin.AsVector();
        var rayDirection = (End.AsVector() - p0).Normalize();

        var k = p0 - sphere.Center.AsVector();

        var b = k.DotProduct(rayDirection);
        var c = k.DotProduct(k) - Math.Pow(sphere.Radius, 2);
        var d = Math.Pow(b, 2) - c; // a = 1 because rayDirection is normalized

        if (d < 0)
        {
            return new List<PlanarPoint3D>();
        }

        var dSqRt = Math.Sqrt(d);

        var t0 = -1.0 * b + dSqRt;
        var t1 = -1.0 * b - dSqRt;

        var i0 = p0 + rayDirection * (float)t0;
        var i1 = p0 + rayDirection * (float)t1;

        return new List<PlanarPoint3D>()
        {
            i0.AsPlanarPoint3D(),
            i1.AsPlanarPoint3D()
        };
    }

    /// <summary>
    /// Is point on the End side of line, specified by ray
    /// </summary>
    public bool IsPointOnEndSide(PlanarPoint3D point)
    {
        var p0 = Begin.AsVector();
        var u = (End.AsVector() - p0).Normalize();

        var t = (point.AsVector() - p0) / u;

        var txy = Math.Abs(t[0] - t[1]);
        var tyz = Math.Abs(t[1] - t[2]);

        if (txy > RendererConstants.TestIsPointOnRayPrecision || tyz > RendererConstants.TestIsPointOnRayPrecision)
        {
            // Point is not on line, part of what ray is
            return false;
        }

        return t[0] >= 0;
    }
}