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

        result[1, 1] = num1;
        result[1, 2] = num2;
        result[2, 1] = -num2;
        result[2, 2] = num1;

        return result;
    }
    
    public static Matrix<double> CreateRotationYMatrix(double radians)
    {
        var result = Matrix<double>.Build.DenseIdentity(4, 4);
        
        var num1 = Math.Cos(radians);
        var num2 = Math.Sin(radians);

        result[0, 0] = num1;
        result[0, 2] = -num2;
        result[2, 0] = num2;
        result[2, 2] = num1;

        return result;
    }
    
    public static Matrix<double> CreateRotationZMatrix(double radians)
    {
        var result = Matrix<double>.Build.DenseIdentity(4, 4);
        
        var num1 = Math.Cos(radians);
        var num2 = Math.Sin(radians);

        result[0, 0] = num1;
        result[0, 1] = num2;
        result[1, 0] = -num2;
        result[1, 1] = num1;

        return result;
    }   
    
    public static Matrix<double> CreateLookAt(MathNet.Numerics.LinearAlgebra.Vector<double> cameraPosition,
        MathNet.Numerics.LinearAlgebra.Vector<double> cameraTarget,
        MathNet.Numerics.LinearAlgebra.Vector<double> cameraUpVector)
    {
        var vector3_1 = (cameraPosition - cameraTarget).Normalize(1);
        var vector3_2 = (Cross3(cameraUpVector, vector3_1)).Normalize(1);
        var vector1 = Cross3(vector3_1, vector3_2);
        
        var result = Matrix<double>.Build.DenseIdentity(4, 4);

        result[0, 0] = vector3_2[0];
        result[0, 1] = vector1[0];
        result[0, 2] = vector3_1[0];
        result[1, 0] = vector3_2[1];
        result[1, 1] = vector1[1];
        result[1, 2] = vector3_1[1];
        result[2, 0] = vector3_2[2];
        result[2, 1] = vector1[2];
        result[2, 2] = vector3_1[2];
        result[3, 0] = -1 * vector3_2.DotProduct(cameraPosition);
        result[3, 1] = -1 * vector1.DotProduct(cameraPosition);
        result[3, 2] = -1 * vector3_1.DotProduct(cameraPosition);
        
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

        result[0, 0] = num2;
        result[0, 1] = 0;
        result[0, 2] = 0;
        result[0, 3] = 0;

        result[1, 1] = num1;
        result[1, 0] = 0;
        result[1, 2] = 0;
        result[1, 3] = 0;
        
        result[2, 0] = 0;
        result[2, 1] = 0;
        
        var num3 = double.IsPositiveInfinity(farPlaneDistance) ? -1f : farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
        result[2, 2] = num3;
        result[2, 3] = -1;
        
        result[3, 2] = nearPlaneDistance * num3;
        result[3, 0] = 0;
        result[3, 1] = 0;
        result[3, 3] = 0;

        return result;
    }

    public static Vector4 Transform(Vector4 vector, Matrix<double> matrix)
    {
        return new Vector4
        (
            (float)(vector.X * matrix[0, 0] + vector.Y * matrix[1, 0] + vector.Z * matrix[2, 0] + vector.W * matrix[3, 0]),
            (float)(vector.X * matrix[0, 1] + vector.Y * matrix[1, 1] + vector.Z * matrix[2, 1] + vector.W * matrix[3, 1]),
            (float)(vector.X * matrix[0, 2] + vector.Y * matrix[1, 2] + vector.Z * matrix[2, 2] + vector.W * matrix[3, 2]),
            (float)(vector.X * matrix[0, 3] + vector.Y * matrix[1, 3] + vector.Z * matrix[2, 3] + vector.W * matrix[3, 3])
        );
    }
    
    public static Vector3 TransformPerspectively(this Matrix<double> matrix, Vector3 vector)
    {
        var transformedVector = Transform(new Vector4(vector, 1.0f), matrix);
        return transformedVector.ToVector3() / transformedVector.W;
    }

    public static Matrix4x4 ToMatrix4x4(this Matrix<double> matrix)
    {
        return new Matrix4x4
        (
            (float)matrix[0, 0], (float)matrix[0, 1], (float)matrix[0, 2], (float)matrix[0, 3],
            (float)matrix[1, 0], (float)matrix[1, 1], (float)matrix[1, 2], (float)matrix[1, 3],
            (float)matrix[2, 0], (float)matrix[2, 1], (float)matrix[2, 2], (float)matrix[2, 3],
            (float)matrix[3, 0], (float)matrix[3, 1], (float)matrix[3, 2], (float)matrix[3, 3]
        );
    }

    public static MathNet.Numerics.LinearAlgebra.Vector<double> Cross3(MathNet.Numerics.LinearAlgebra.Vector<double> vector1, MathNet.Numerics.LinearAlgebra.Vector<double> vector2)
    {
        return MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray
        (
            new double[]
            {
                vector1[1] * vector2[2] - vector1[2] * vector2[1],
                vector1[2] * vector2[0] - vector1[0] * vector2[2],
                vector1[0] * vector2[1] - vector1[1] * vector2[0]   
            }
        );
    }
}