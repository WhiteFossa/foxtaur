using System.Numerics;
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
    public static Ray CreateByScreenRaycasting(ICamera camera, float screenX, float screenY, float viewportWidth,
        float viewportHeight)
    {
        var normalizedDeviceX = screenX / viewportWidth * 2.0f - 1.0f;
        var normalizedDeviceY = screenY / viewportHeight * 2.0f - 1.0f;
        
        var nearPoint = camera.Position3D.AsVector3();
        var farPoint = camera.BackProjectionMatrix.TransformPerspectively(new Vector3(normalizedDeviceX, normalizedDeviceY, 1.0f));

        return new Ray(nearPoint.AsPlanarPoint3D(), farPoint.AsPlanarPoint3D());
    }

    /// <summary>
    /// Intersect ray with the sphere
    /// </summary>
    public List<PlanarPoint3D> Intersect(Sphere sphere)
    {
        var p0 = Begin.AsVector3();
        var rayDirection = Vector3.Normalize(End.AsVector3() - p0);
        
        var k = p0 - sphere.Center.AsVector3();

        var b = Vector3.Dot(k, rayDirection);
        var c = Vector3.Dot(k, k) - (float)Math.Pow(sphere.Radius, 2);
        var d = (float)Math.Pow(b, 2) - c; // a = 1 because rayDirection is normalized
        
        if (d < 0)
        {
            return new List<PlanarPoint3D>();
        }

        var dSqRt = (float)Math.Sqrt(d);

        var t0 = -1.0f * b + dSqRt;
        var t1 = -1.0f * b - dSqRt;

        var i0 = p0 + rayDirection * t0;
        var i1 = p0 + rayDirection * t1;

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
        var p0 = Begin.AsVector3();
        var u = Vector3.Normalize(End.AsVector3() - p0);

        var t = (point.AsVector3() - p0) / u;

        var txy = Math.Abs(t.X - t.Y);
        var tyz = Math.Abs(t.Y - t.Z);

        if (txy > RendererConstants.TestIsPointOnRayPrecision || tyz > RendererConstants.TestIsPointOnRayPrecision)
        {
            // Point is not on line, part of what ray is
            return false;
        }
        
        return t.X >= 0;
    }
}