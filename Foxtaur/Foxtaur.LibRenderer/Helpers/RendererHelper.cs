using Foxtaur.LibGeo.Models;
using Foxtaur.LibRenderer.Constants;
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
    /// PlanarPoint3D to Vector
    /// </summary>
    public static Vector<double> AsVector(this PlanarPoint3D point)
    {
        return Vector<double>.Build.DenseOfArray(new double[] { point.X, point.Y, point.Z });
    }

    /// <summary>
    /// Rotate around direction to angle
    /// </summary>
    public static Vector<double> RotateAround(this Vector<double> toRotate, Vector<double> direction, double a)
    {
        var nd = direction.Normalize();

        var cosa = Math.Cos(a);
        var oneMinusCosa = 1 - cosa;
        var sina = Math.Sin(a);

        var m11 = cosa + oneMinusCosa * Math.Pow(nd[0], 2);
        var m12 = oneMinusCosa * nd[0] * nd[1] - sina * nd[2];
        var m13 = oneMinusCosa * nd[0] * nd[2] + sina * nd[1];

        var m21 = oneMinusCosa * nd[1] * nd[0] + sina * nd[2];
        var m22 = cosa + oneMinusCosa * Math.Pow(nd[1], 2);
        var m23 = oneMinusCosa * nd[1] * nd[2] - sina * nd[0];

        var m31 = oneMinusCosa * nd[2] * nd[0] - sina * nd[1];
        var m32 = oneMinusCosa * nd[2] * nd[1] + sina * nd[0];
        var m33 = cosa + oneMinusCosa * Math.Pow(nd[2], 2);

        var rotation = Matrix<double>.Build.DenseOfArray(
            new double[,]
            {
                { m11, m12, m13 },
                { m21, m22, m23 },
                { m31, m32, m33 }
            });

        var toRotateMathNet = Vector<double>.Build.Dense(new double[] { toRotate[0], toRotate[1], toRotate[2] });

        return rotation * toRotateMathNet;
    }

    /// <summary>
    /// Is point in culling viewport?
    /// </summary>
    public static bool IsPointInCullingViewport(this PlanarPoint2D point)
    {
        return point.X >= -1.0 * RendererConstants.CullingViewportSize
               &&
               point.X <= RendererConstants.CullingViewportSize
               &&
               point.Y >= -1.0 * RendererConstants.CullingViewportSize
               &&
               point.Y <= RendererConstants.CullingViewportSize;
    }

    /// <summary>
    /// Is culling viewport covered by given segment?
    /// </summary>
    public static bool IsCullingViewpointCoveredBySegment(this PlanarSegment segment)
    {
        return (segment.Left <= -1.0 * RendererConstants.CullingViewportSize && segment.Right >= RendererConstants.CullingViewportSize)
               ||
               (segment.Top >= RendererConstants.CullingViewportSize && segment.Bottom <= -1.0 * RendererConstants.CullingViewportSize);
    }
}