using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Matrix
{
    public static double[,] MultiScalarMatrix(double scalar, double[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int columns = matrix.GetLength(1);

        double[,] result = new double[rows, columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                result[i, j] = scalar * matrix[i, j];
            }
        }

        return result;
    }

    public static double[] MultiScalarArray(double scalar, double[] array)
    {
        return array.Select(r => r * scalar).ToArray();
    }

    public static double[] AddArrays(params double[][] arrays)
    {
        if (arrays.Length == 0)
        {
            throw new ArgumentException("At least one array must be provided.");
        }

        int length = arrays[0].Length;
        for (int i = 1; i < arrays.Length; i++)
        {
            if (arrays[i].Length != length)
            {
                throw new ArgumentException("Array lengths must be equal.");
            }
        }

        double[] result = new double[length];
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < arrays.Length; j++)
            {
                result[i] += arrays[j][i];
            }
        }
        return result;
    }
    public static double[] SubtractArrays(params double[][] arrays)
    {
        if (arrays.Length == 0)
        {
            throw new ArgumentException("At least one array must be provided.");
        }

        int length = arrays[0].Length;
        for (int i = 1; i < arrays.Length; i++)
        {
            if (arrays[i].Length != length)
            {
                throw new ArgumentException("Array lengths must be equal.");
            }
        }

        double[] result = new double[length];
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < arrays.Length; j++)
            {
                result[i] -= arrays[j][i];
            }
        }
        return result;
    }

    public static double[,] ConcatMatrices(params double[][,] matrices)
    {
        int size = matrices[0].GetLength(0);
        int n = matrices.Length;
        //Debug.Log("n = " + n + " size = " + size);
        int sqrtN = (int)Mathf.Sqrt(n);
        int newSize = size * sqrtN;

        // Check if n is a perfect square
        if (sqrtN * sqrtN != n)
        {
            throw new ArgumentException("Number of matrices must be a perfect square.");
        }

        // Create the resulting matrix
        double[,] result = new double[newSize, newSize];

        // Concatenate the matrices
        for (int i = 0; i < n; i++)
        {
            int rowOffset = (i / sqrtN) * size;
            int colOffset = (i % sqrtN) * size;

            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    result[rowOffset + j, colOffset + k] = matrices[i][j,k];
                }
            }
        }
        return result;
    }

    public static double[] Vector3ToArray(Vector3 vector)
    {
        double[] array = new double[3];
        array[0] = vector.x;
        array[1] = vector.y;
        array[2] = vector.z;
        return array;
    }

    public static Vector3[] ArrayToVector3(double[] array)
    {
        int numElements = array.Length;
        int numVectors = numElements / 3;

        if (numElements % 3 != 0)
        {
            Debug.LogError("Invalid array length. Expected a length divisible by 3.");
            return null;
        }

        Vector3[] vectorArray = new Vector3[numVectors];

        for (int i = 0; i < numVectors; i++)
        {
            int startIndex = i * 3;
            float x = (float)array[startIndex];
            float y = (float)array[startIndex + 1];
            float z = (float)array[startIndex + 2];

            vectorArray[i] = new Vector3(x, y, z);
        }

        return vectorArray;
    }

    public static double[] MultiMatrixArray(double[,] matrix, double[] array)
    {
        int numRows = matrix.GetLength(0);
        int numCols = matrix.GetLength(1);

        if (numCols != array.Length)
        {
            throw new ArgumentException("Matrix and array dimensions are incompatible for multiplication.");
        }

        double[] result = new double[numRows];

        for (int i = 0; i < numRows; i++)
        {
            double sum = 0;

            for (int j = 0; j < numCols; j++)
            {
                sum += matrix[i, j] * array[j];
            }

            result[i] = sum;
        }

        return result;
    }

    // Only works if "matrix" is diagonal
    public static double[,] DiagonalInverse(double[,] matrix)
    {
        int size = matrix.GetLength(0);

        if (size != matrix.GetLength(1))
        {
            throw new ArgumentException("Matrix is not square.");
        }

        double[,] result = new double[size, size];

        for (int i = 0; i < size; i++)
        {
            double value = matrix[i, i];

            if (Math.Abs(value) < double.Epsilon)
            {
                throw new InvalidOperationException("Matrix is not invertible.");
            }

            result[i, i] = 1.0 / value;
        }

        return result;
    }

    public static double[,] Eye(int n)
    {
        double[,] matrix = new double[n,n];

        for (int i = 0; i < n; i++)
        {
            matrix[i,i] = 1f;
        }

        return matrix;
    }

    public static double[,] Zeroes(int n)
    {
        double[,] matrix = new double[n,n];

        for (int i = 0; i < n; i++)
        {
            for (int j=0; j<n;j++)
            {
                matrix[i,j] = 0;    
            }
        }

        return matrix;
    }

    public static double[,] ConcatMatricesInLine(params double[][,] matrices)
    {
        int numRows = matrices[0].GetLength(0);
        int numCols = 0;

        foreach (var matrix in matrices)
        {
            //Debug.Log(matrix.GetLength(0));
            if (matrix.GetLength(0) != numRows)
            {
                throw new ArgumentException("The matrices must have the same number of rows.");
            }

            numCols += matrix.GetLength(1);
        }

        double[,] concatenatedMatrix = new double[numRows, numCols];
        int colIndex = 0;

        foreach (var matrix in matrices)
        {
            int matrixCols = matrix.GetLength(1);

            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < matrixCols; col++)
                {
                    concatenatedMatrix[row, colIndex + col] = matrix[row, col];
                }
            }

            colIndex += matrixCols;
        }

        return concatenatedMatrix;
    }


    public static T[] CombineArrays<T>(T[] array1, T[] array2)
    {
        if (array1.Length != array2.Length)
        {
            throw new ArgumentException("The arrays must have the same length.");
        }

        T[] combinedArray = new T[array1.Length * 2];

        for (int i = 0; i < array1.Length; i++)
        {
            combinedArray[i * 2] = array1[i];
            combinedArray[i * 2 + 1] = array2[i];
        }

        return combinedArray;
    }

    public static double[,] ArrayToMatrix(double[] elements)
    {
        int n = elements.Length;
        int size = (int)Math.Sqrt(n);

        if (size * size != n)
        {
            throw new ArgumentException("The number of elements is not a perfect square.");
        }

        double[,] matrix = new double[size, size];

        int index = 0;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                matrix[i, j] = elements[index++];
            }
        }

        return matrix;
    }


    public static double[,] Multiply(double[,] matrixA, double[,] matrixB)
    {
        int rowsA = matrixA.GetLength(0);
        int colsA = matrixA.GetLength(1);
        int rowsB = matrixB.GetLength(0);
        int colsB = matrixB.GetLength(1);

        if (colsA != rowsB)
        {
            throw new ArgumentException("The number of columns in matrixA must be equal to the number of rows in matrixB for matrix multiplication.");
        }

        double[,] result = new double[rowsA, colsB];

        for (int i = 0; i < rowsA; i++)
        {
            for (int j = 0; j < colsB; j++)
            {
                double sum = 0.0;

                for (int k = 0; k < colsA; k++)
                {
                    sum += matrixA[i, k] * matrixB[k, j];
                }

                result[i, j] = sum;
            }
        }

        return result;
    }


    public static double[,] JaggedToMultidimensional(double[][] jaggedArray)
    {
        int rows = jaggedArray.Length;
        int cols = jaggedArray[0].Length;

        double[,] multidimensionalArray = new double[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                multidimensionalArray[i, j] = jaggedArray[i][j];
            }
        }

        return multidimensionalArray;
    }

    public static double[][] MultidimensionalToJagged(double[,] multidimensionalArray)
    {
        int rows = multidimensionalArray.GetLength(0);
        int cols = multidimensionalArray.GetLength(1);

        double[][] jaggedArray = new double[rows][];

        for (int i = 0; i < rows; i++)
        {
            jaggedArray[i] = new double[cols];

            for (int j = 0; j < cols; j++)
            {
                jaggedArray[i][j] = multidimensionalArray[i, j];
            }
        }

        return jaggedArray;
    }

    public static void PrintMatrix(string message, double[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int columns = matrix.GetLength(1);
        message += "[ ";
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                message += matrix[i, j].ToString() + "  ";
            }
            message += "; ";
        }
        message += "].";
        Debug.Log(message);
    }

    public static void PrintArray(string message, double[] array)
    {
        int n = array.Length;
        message += "[ ";
        for (int i = 0; i < n; i++)
        {
            message += array[i].ToString() + "  ";
        }
        message += "].";
        Debug.Log(message);
    }
}
