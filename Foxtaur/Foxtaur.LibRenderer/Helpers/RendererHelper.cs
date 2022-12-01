using System.Numerics;
using Foxtaur.LibGeo.Models;
using MathNet.Numerics.LinearAlgebra;
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
    public static Vector3 ToVector3(this Vector4 vec)
    {
        return new Vector3(vec.X, vec.Y, vec.Z);
    }

    /// <summary>
    /// Transform matrix perspectively
    /// </summary>
    public static Vector3 TransformPerspectively(this Matrix4x4 matrix, Vector3 vector)
    {
        var transformedVector = Vector4.Transform(new Vector4(vector, 1.0f), matrix);
        return transformedVector.ToVector3() / transformedVector.W;
    }

    /// <summary>
    /// PlanarPoint3D to Vector3
    /// </summary>
    public static Vector3 AsVector3(this PlanarPoint3D point)
    {
        return new Vector3(point.X, point.Y, point.Z);
    }

    /// <summary>
    /// Rotate around direction to angle
    /// </summary>
    public static Vector3 RotateAround(this Vector3 toRotate, Vector3 direction, float a)
    {
        var nd = Vector3.Normalize(direction);

        var cosa = Math.Cos(a);
        var oneMinusCosa = 1 - cosa;
        var sina = Math.Sin(a);

        var m11 = (float)(cosa + oneMinusCosa * Math.Pow(nd.X, 2));
        var m12 = (float)(oneMinusCosa * nd.X * nd.Y - sina * nd.Z);
        var m13 = (float)(oneMinusCosa * nd.X * nd.Z + sina * nd.Y);

        var m21 = (float)(oneMinusCosa * nd.Y * nd.X + sina * nd.Z);
        var m22 = (float)(cosa + oneMinusCosa * Math.Pow(nd.Y, 2));
        var m23 = (float)(oneMinusCosa * nd.Y * nd.Z - sina * nd.X);

        var m31 = (float)(oneMinusCosa * nd.Z * nd.X - sina * nd.Y);
        var m32 = (float)(oneMinusCosa * nd.Z * nd.Y + sina * nd.X);
        var m33 = (float)(cosa + oneMinusCosa * Math.Pow(nd.Z, 2));

        var rotation = Matrix<float>.Build.DenseOfArray(
            new float[,]
            {
                { m11, m12, m13 },
                { m21, m22, m23 },
                { m31, m32, m33 }
            });

        var toRotateMathNet =
            MathNet.Numerics.LinearAlgebra.Vector<float>.Build.Dense(new float[]
                { toRotate.X, toRotate.Y, toRotate.Z });

        var rotated = rotation * toRotateMathNet;

        return new Vector3(rotated[0], rotated[1], rotated[2]);
    }

    /// <summary>
    /// Multiply Matrix4x4 by Vector
    /// </summary>
    public static Vector4 MultiplyMatrix4x4ByVector4(Matrix4x4 matrix, Vector4 vector)
    {
        var mnMatrix = Matrix<float>.Build.DenseOfArray(
            new float[,]
            {
                { matrix.M11, matrix.M12, matrix.M13, matrix.M14 },
                { matrix.M21, matrix.M22, matrix.M23, matrix.M24 },
                { matrix.M31, matrix.M32, matrix.M33, matrix.M34 },
                { matrix.M41, matrix.M42, matrix.M43, matrix.M44 }
            });

        var mnVector = MathNet.Numerics.LinearAlgebra.Vector<float>.Build.Dense(new float[] { vector.X, vector.Y, vector.Z, vector.W });

        var result = mnMatrix * mnVector;

        return new Vector4(result[0], result[1], result[2], result[3]);
    }
}