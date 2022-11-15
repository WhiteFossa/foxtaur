using System.Numerics;
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
    public static Ray CreateByScreenRaycasting(ICamera camera, float screenX, float screenY, float viewportWidth, float viewportHeight)
    {
        var normalizedDeviceX = screenX / viewportWidth * 2.0f - 1.0f;
        var normalizedDeviceY = screenY / viewportHeight * 2.0f - 1.0f;
        
        var nearPoint = camera.BackProjectionMatrix.TransformPerspectively(new Vector3(normalizedDeviceX, normalizedDeviceY, 0.0f));
        var farPoint = camera.BackProjectionMatrix.TransformPerspectively(new Vector3(normalizedDeviceX, normalizedDeviceY, 1.0f));

        return new Ray(nearPoint.AsPlanarPoint3D(), farPoint.AsPlanarPoint3D());
    }

    public List<PlanarPoint3D> Intersect(Sphere sphere)
    {
        var bxex = Begin.X - End.X;
        var byey = Begin.Y - End.Y;
        var bzez = Begin.Z - End.Z;
        
        var a = 1.0f + (float)Math.Pow(byey / bxex, 2) + (float)Math.Pow(bzez / bxex, 2);

        var b = 2 * sphere.Center.X
                + 2 * (byey / bxex) * ((Begin.X * End.Y - End.X * Begin.Y) / bxex - sphere.Center.Y)
                + 2 * (bzez / bxex) * ((Begin.X * End.Z - End.X * Begin.Z) / bxex - sphere.Center.Z);

        var c = (float)Math.Pow(sphere.Center.X, 2)
                + (float)Math.Pow(((Begin.X * End.Y - End.X * Begin.Y) / bxex - sphere.Center.Y), 2)
                + (float)Math.Pow(((Begin.X * End.Z - End.X * Begin.Z) / bxex - sphere.Center.Z), 2)
                - sphere.Radius;

        var d = (float)Math.Pow(b, 2) - 4 * a * c;

        if (d < 0)
        {
            return new List<PlanarPoint3D>();
        }

        var x1 = (-1.0f * b + (float)Math.Sqrt(d)) / (2.0f * a);
        var x2 = (-1.0f * b - (float)Math.Sqrt(d)) / (2.0f * a);

        var y1 = GetYForSphereIntersection(x1, bxex, byey);
        var y2 = GetYForSphereIntersection(x2, bxex, byey);
        
        var z1 = GetZForSphereIntersection(x1, bxex, bzez);
        var z2 = GetZForSphereIntersection(x2, bxex, bzez);

        return new List<PlanarPoint3D>()
        {
            new(x1, y1, z1),
            new(x2, y2, z2)
        };
    }

    private float GetYForSphereIntersection(float x, float bxex, float byey)
    {
        return byey / bxex * x + (Begin.X * End.Y - End.X * Begin.Y) / bxex;
    }
    
    private float GetZForSphereIntersection(float x, float bxex, float bzez)
    {
        return bzez / bxex * x + (Begin.X * End.Z - End.X * Begin.Z) / bxex;
    }
}