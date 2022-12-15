using System.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Foxtaur.LibRenderer.Helpers;

/// <summary>
/// Stuff like matrices and vectors with double precision
/// </summary>
public static class DoubleHelper
{
    public static Matrix<double> CreateRotationXMatrix(double radians)
    {
        var result = Matrix<double>.Build.DenseIdentity(4, 4);
        
        var num1 = Math.Cos(radians);
        var num2 = Math.Sin(radians);

        result[2, 2] = num1;
        result[2, 3] = num2;
        result[3, 2] = -num2;
        result[3, 3] = num1;

        return result;
    }
    
    public static Matrix<double> CreateRotationYMatrix(double radians)
    {
        var result = Matrix<double>.Build.DenseIdentity(4, 4);
        
        var num1 = Math.Cos(radians);
        var num2 = Math.Sin(radians);

        result[1, 1] = num1;
        result[1, 3] = -num2;
        result[3, 1] = num2;
        result[3, 3] = num1;

        return result;
    }
    
    public static Matrix<double> CreateRotationZMatrix(double radians)
    {
        var result = Matrix<double>.Build.DenseIdentity(4, 4);
        
        var num1 = Math.Cos(radians);
        var num2 = Math.Sin(radians);

        result[1, 1] = num1;
        result[1, 2] = num2;
        result[2, 1] = -num2;
        result[2, 2] = num1;

        return result;
    }   
    
    public static Matrix<double> CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
    {
        Vector3 vector3_1 = Vector3.Normalize(cameraPosition - cameraTarget);
        Vector3 vector3_2 = Vector3.Normalize(Vector3.Cross(cameraUpVector, vector3_1));
        Vector3 vector1 = Vector3.Cross(vector3_1, vector3_2);
        
        var result = Matrix<double>.Build.DenseIdentity(4, 4);

        result[1, 1] = vector3_2.X;
        result[1, 2] = vector1.X;
        result[1, 3] = vector3_1.X;
        result[2, 1] = vector3_2.Y;
        result[2, 2] = vector1.Y;
        result[2, 3] = vector3_1.Y;
        result[3, 1] = vector3_2.Z;
        result[3, 2] = vector1.Z;
        result[3, 3] = vector3_1.Z;
        result[4, 1] = -Vector3.Dot(vector3_2, cameraPosition);
        result[4, 2] = -Vector3.Dot(vector1, cameraPosition);
        result[4, 3] = -Vector3.Dot(vector3_1, cameraPosition);
        
        return result;
    }
    
    public static Matrix<double> CreatePerspectiveFieldOfView(double fieldOfView, double aspectRatio, double nearPlaneDistance, double farPlaneDistance)
    {
        if (fieldOfView <= 0.0 || fieldOfView >= Math.PI)
        {
            throw new ArgumentOutOfRangeException(nameof (fieldOfView));
        }

        if (nearPlaneDistance <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));
        }

        if (farPlaneDistance <= 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(farPlaneDistance));
        }

        if (nearPlaneDistance >= farPlaneDistance)
        {
            throw new ArgumentOutOfRangeException(nameof(nearPlaneDistance));
        }

        var num1 = 1 / Math.Tan(fieldOfView * 0.5);
        var num2 = num1 / aspectRatio;
        
        var result = Matrix<double>.Build.DenseIdentity(4, 4);

        result[1, 1] = num2;
        result[1, 2] = 0;
        result[1, 3] = 0;
        result[1, 4] = 0;

        result[2, 2] = num1;
        result[2, 1] = 0;
        result[2, 3] = 0;
        result[2, 4] = 0;
        
        result[3, 1] = 0;
        result[3, 2] = 0;
        
        var num3 = double.IsPositiveInfinity(farPlaneDistance) ? -1f : farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
        result[3, 3] = num3;
        result[3, 4] = -1;
        
        result[4, 3] = nearPlaneDistance * num3;
        result[4, 1] = 0;
        result[4, 2] = 0;
        result[4, 4] = 0;

        return result;
    }

    public static Vector4 Transform(Vector4 vector, Matrix<double> matrix)
    {
        return new Vector4
        (
            (float)(vector.X * matrix[1, 1] + vector.Y * matrix[2, 1] + vector.Z * matrix[3, 1] + vector.W * matrix[4, 1]),
            (float)(vector.X * matrix[1, 2] + vector.Y * matrix[2, 2] + vector.Z * matrix[3, 2] + vector.W * matrix[4, 2]),
            (float)(vector.X * matrix[1, 3] + vector.Y * matrix[2, 3] + vector.Z * matrix[3, 3] + vector.W * matrix[4, 3]),
            (float)(vector.X * matrix[1, 4] + vector.Y * matrix[2, 4] + vector.Z * matrix[3, 4] + vector.W * matrix[4, 4])
        );
    }
}