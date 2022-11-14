using System.Numerics;
using Foxtaur.LibRenderer.Models;
using NLog;

namespace Foxtaur.LibRenderer.Helpers;

/// <summary>
/// Useful stuff for renderer
/// </summary>
public static class RendererHelper
{
    /// <summary>
    /// Log fatal error and throw an exception
    /// </summary>
    public static void LogAndThrowFatalError(Logger logger, string message)
    {
        logger.Fatal(message);
        throw new InvalidOperationException(message);
    }

    /// <summary>
    /// Throw 4th vector component out
    /// </summary>
    public static Vector3 To3(this Vector4 vec)
    {
        return new Vector3(vec.X, vec.Y, vec.Z);
    }

    /// <summary>
    /// Transform matrix perspectively
    /// </summary>
    public static Vector3 TransformPerspectively(this Matrix4x4 matrix, Vector3 vector)
    {
        var transformedVector = Vector4.Transform(new Vector4(vector, 1.0f), matrix);
        return transformedVector.To3() / transformedVector.W;
    }

    /// <summary>
    /// Vector3 to PlanarPorint3D
    /// </summary>
    public static PlanarPoint3D ToPlanarPoint3D(this Vector3 vector)
    {
        return new PlanarPoint3D(vector.X, vector.Y, vector.Z);
    }
}