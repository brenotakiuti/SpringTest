using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RKFTest_matrix : MonoBehaviour
{
    float deltaT = 20f;
    RKF45 solver = new RKF45();

    double[] initialState = new double[2] { 0.5, 0};
    double[] initialStateM = new double[4] { 0.5, 0, 0, 0 };
    double[] x = new double[4] { 2, 5, 11, 7 };
    double J;
    float r = 0.3f;
    double[,] M;
    double[,] C;
    double[,] K;
    double[,] A1;

    // Start is called before the first frame update
    void Start()
    {
        J = Mathf.PI*Mathf.Pow(r,4)/4;
        M = new double[,]
        {
            { 0.5, 0 },
            { 0, J}
        };
        C = new double[,]
        {
            { 0, 0 },
            { 0, 0 }
        };
        K = new double[,]
        {
            { 10, 0 },
            { 0, 20 }
        };

        int size = M.GetLength(0);

        A1 = Matrix.ConcatMatrices(Matrix.Zeroes(size), Matrix.Eye(size), Matrix.MultiScalarMatrix(-1, Matrix.Multiply(Matrix.DiagonalInverse(M), K)), Matrix.MultiScalarMatrix(-1, Matrix.Multiply(Matrix.DiagonalInverse(M), C)));

        Matrix.PrintMatrix("A1: ", A1);

        Result resultM = solver.Solve(springODE, 0, deltaT, initialState, 0.005, 1e-10);
        Result result = solver.Solve(springODEmatrix, 0, deltaT, initialStateM, 0.005, 1e-10);

        double[] multtest = Matrix.MultiMatrixArray(A1, x);
        Matrix.PrintArray("Multi: ", multtest);
        Matrix.PrintArray("t: " + result.t[1]+ " Result: ", result.y[1]);
        Matrix.PrintArray("t: " + result.t[7]+ " Result: ", result.y[7]);
        Matrix.PrintArray("t: " + result.t[41]+ " Result: ", result.y[41]);
        Matrix.PrintArray("t: " + result.t[69]+ " Result: ", result.y[69]);
        Matrix.PrintArray("t: " + result.t[114]+ " Result: ", result.y[114]);
        Matrix.PrintArray("t2: " + resultM.t[1]+ " Result: ", resultM.y[1]);
        Matrix.PrintArray("t2: " + resultM.t[7]+ " Result: ", resultM.y[7]);
        Matrix.PrintArray("t2: " + resultM.t[41]+ " Result: ", resultM.y[41]);
        Matrix.PrintArray("t2: " + resultM.t[69]+ " Result: ", resultM.y[69]);
        Matrix.PrintArray("t2: " + resultM.t[114]+ " Result: ", resultM.y[114]);
        // With this, rkf45 is proven to be correct even using matrices
    }

    private double[] springODEmatrix(double t, double[] x)
    {
        double[] y = (double[])x.Clone();

        y = Matrix.MultiMatrixArray(A1, x);
        return y;
    }

    private double[] springODE(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -0 / 0.5 * x[1] - 10 / 0.5 * x[0];
        return y;
    }

}
