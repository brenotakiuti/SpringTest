using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixConcatTest : MonoBehaviour
{
    // Works!
    void Start()
    {
        double[,] matrix1 = new double[,]
        {
            { 1, 1 },
            { 1, 1 }
        };
        double[,] matrix2 = new double[,]
        {
                { 2, 2 },
                { 2, 2 }
        };
        double[,] matrix3 = new double[,]
        {
            { 3, 3 },
            { 3, 3 }
        };

        double[,] matrix4 = new double[,]
        {
            { 4, 4 },
            { 4, 4 }
        };
        double[,] matrix5 = new double[,]
        {
            { 5, 5 },
            { 5, 5 }
        };
        double[][] matrix6 = new double[][]
        {
            new double[] { 6, 6 },
            new double[] { 6, 6 }
        };
        double[][] matrix7 = new double[][]
        {
            new double[] { 7, 7 },
            new double[] { 7, 7 }
        };

        double[,] M = new double[,]
        {
            { 0.5, 0 },
            { 0, 0.5}
        };double[,] C = new double[,]
        {
            { 0, 0 },
            { 0, 0 }
        };double[,] K = new double[,]
        {
            { 10, 0 },
            { 0, 20 }
        };
        double[,] matrix8 = Matrix.Eye(2);
        double[,] matrix9 = Matrix.Zeroes(2);
        int size = K.GetLength(0);
        //double[,] result = Matrix.ConcatMatricesInLine(matrix1, matrix2, matrix3, matrix4, matrix5);
        double[,] A1 = Matrix.ConcatMatrices(Matrix.Zeroes(size), Matrix.Eye(size),Matrix.MultiScalarMatrix(-1,Matrix.Multiply(Matrix.DiagonalInverse(M),K)),Matrix.MultiScalarMatrix(-1,Matrix.Multiply(Matrix.DiagonalInverse(M),C)));

        Matrix.PrintMatrix("Result: ",A1);
    }

}
