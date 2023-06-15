//using System.Collections;
using System.Collections.Generic;
//using UnityEngine;
using System;
using System.Linq;

//using MathNet.Numerics.LinearAlgebra;

/// <summary>
/// Implements the Runge-Kutta-Fehlberg 4(5) numerical integration method. [1]Numerical methods using MATLAB/John H. Mathews, Kurtis D. Fink—4th ed.
/// </summary>
public class RKF45
{
    // Coefficients for the method REF[1]Eq(28)
    private double[] coef_A = { 0, 0.25, 0.375, 0.9231, 1, 0.5 };
    private double[,] coef_B = { { 0, 0, 0, 0, 0 }, { 0.25, 0, 0, 0, 0 }, { 0.093750, 0.28125, 0, 0, 0 }, { 0.879380974055530, -3.277196176604461, 3.320892125625853, 0, 0 }, { 2.032407407407407, -8, 7.173489278752436, -0.205896686159844, 0 }, { -0.296296296296296, 2, -1.381676413255361, 0.452972709551657, -0.275 } };
    private double[] coef_C = { 0.115740740740741, 0, 0.548927875243665, 0.535331384015595, -0.2, 0 };
    private double[] coef_CH = { 0.118518518518519, 0, 0.518986354775828, 0.506131490342017, -0.18, 0.0363636363636363636 };

    // state = {x,x'}
    // derivative = {x',x''}

    /// <summary>
    /// Solves an ordinary differential equation using the Runge-Kutta-Fehlberg 4(5) method.
    /// </summary>
    /// <param name="odeFunction">The function that defines the ordinary differential equation. It takes the current time and state as parameters and returns the derivative.</param>
    /// <param name="startTime">The start time of the integration.</param>
    /// <param name="endTime">The end time of the integration.</param>
    /// <param name="initialState">The initial state of the system.</param>
    /// <param name="stepSize">The initial step size for the integration.</param>
    /// <param name="tolerance">The desired tolerance for the integration error.</param>
    /// <returns>A Result object containing the time steps and corresponding states.</returns>
    public Result Solve(Func<double, double[], double[]> odeFunction, double startTime, double endTime, double[] initialState, double stepSize, double tolerance)
    {
        //odeFunction(time,state), return final state

        double[] state = (double[])initialState.Clone();    //Do NOT EVER assign an array with another array just to "copy it". In C# double[] state = initialState assigns the reference of initialState to state, making them essentially the same thing (literally the same position in the memory)
        int n = state.Length;
        List<double[]> finalState = new List<double[]>();
        //double[] yState = { 0, 0 };
        double[] yState = new double[n];
        //double[] zState = { 0, 0 };
        double[] zState = new double[n];
        double time = startTime;
        double step = stepSize;
        //double[] k = { 0, 0, 0, 0, 0, 0 };
        double[] k0, k1, k2, k3, k4, k5;
        double[] f;
        bool repeat = true;
        int iterationLimit = 10 ^ 5;
        int timeIndex = 1;
        int counter2 = 0;
        Result result = new Result();
        int multiplier = 100;

        //calculate the maximun number of time steps
        int maxTimeItation = (int)Math.Ceiling((endTime - startTime) / (stepSize * multiplier));

        //Start solving
        result.y.Add((double[])initialState.Clone());
        result.t.Add(startTime);
        while (time < endTime)
        {
            Array.Copy(result.y[timeIndex - 1], state, state.Length);
            while (repeat)
            {
                // Copy the values in state to yState and zState to RESET these 2
                Array.Copy(state, yState, state.Length);
                Array.Copy(state, zState, state.Length);

                // Reser newTime and newState. Those are used to calculate f(newTime, newState)
                double newTime = time + coef_A[0] * step;
                double[] newState = (double[])state.Clone();

                // Calculation of k_n REF[1]Eq(28)Page(508)
                //k1
                f = odeFunction(newTime, newState);
                //k0 =  step * f;
                k0 = f.Select(r => r * step).ToArray();

                //k2
                newTime = time + coef_A[1] * step;
                //newState[0] = newState[0] + coef_B[1, 0] * k[0];
                newState = Matrix.AddArrays(state, Matrix.MultiScalarArray(coef_B[1, 0], k0));
                f = odeFunction(newTime, newState);
                //k1 = step * f[0];
                k1 = f.Select(r => r * step).ToArray();

                //k3
                newTime = time + coef_A[2] * step;
                //newState[0] = state[0] + coef_B[2, 0] * k[0] + coef_B[2, 1] * k[1];
                newState = Matrix.AddArrays(state, Matrix.MultiScalarArray(coef_B[2, 0], k0), Matrix.MultiScalarArray(coef_B[2, 1], k1));
                f = odeFunction(newTime, newState);
                //k[2] = step * f[0];
                k2 = f.Select(r => r * step).ToArray();

                //k4
                newTime = time + coef_A[3] * step;
                //newState[0] = state[0] + coef_B[3, 0] * k[0] + coef_B[3, 1] * k[1] + coef_B[3, 2] * k[2];
                newState = Matrix.AddArrays(state, Matrix.MultiScalarArray(coef_B[3, 0], k0), Matrix.MultiScalarArray(coef_B[3, 1], k1), Matrix.MultiScalarArray(coef_B[3, 2], k2));
                f = odeFunction(newTime, newState);
                //k[3] = step * f[0];
                k3 = f.Select(r => r * step).ToArray();

                //k5
                newTime = time + coef_A[4] * step;
                //newState[0] = state[0] + coef_B[4, 0] * k[0] + coef_B[4, 1] * k[1] + coef_B[4, 2] * k[2] + coef_B[4, 3] * k[3];
                newState = Matrix.AddArrays(state, Matrix.MultiScalarArray(coef_B[4, 0], k0), Matrix.MultiScalarArray(coef_B[4, 1], k1), Matrix.MultiScalarArray(coef_B[4, 2], k2), Matrix.MultiScalarArray(coef_B[4, 3], k3));
                f = odeFunction(newTime, newState);
                //k[4] = step * f[0];
                k4 = f.Select(r => r * step).ToArray();

                //k6
                newTime = time + coef_A[5] * step;
                //newState[0] = state[0] + coef_B[5, 0] * k[0] + coef_B[5, 1] * k[1] + coef_B[5, 2] * k[2] + coef_B[5, 3] * k[3] + coef_B[5, 4] * k[4];
                newState = Matrix.AddArrays(state, Matrix.MultiScalarArray(coef_B[5, 0], k0), Matrix.MultiScalarArray(coef_B[5, 1], k1), Matrix.MultiScalarArray(coef_B[5, 2], k2), Matrix.MultiScalarArray(coef_B[5, 3], k3), Matrix.MultiScalarArray(coef_B[5, 4], k4));
                f = odeFunction(newTime, newState);
                //k[5] = step * f[0];
                k5 = f.Select(r => r * step).ToArray();

                // Calculation of y_k+1 and z_k+1 REF[1]Eq(29-30)Page(508) 
                yState = Matrix.AddArrays(state, Matrix.MultiScalarArray(coef_C[0], k0), Matrix.MultiScalarArray(coef_C[1], k1), Matrix.MultiScalarArray(coef_C[2], k2), Matrix.MultiScalarArray(coef_C[3], k3), Matrix.MultiScalarArray(coef_C[4], k4), Matrix.MultiScalarArray(coef_C[5], k5));
                zState = Matrix.AddArrays(state, Matrix.MultiScalarArray(coef_CH[0], k0), Matrix.MultiScalarArray(coef_CH[1], k1), Matrix.MultiScalarArray(coef_CH[2], k2), Matrix.MultiScalarArray(coef_CH[3], k3), Matrix.MultiScalarArray(coef_CH[4], k4), Matrix.MultiScalarArray(coef_CH[5], k5));

                // The error is the difference between the two aproximations
                double[] error = Matrix.SubtractArrays(zState, yState);

                bool errorBool = error.Any(element => element > tolerance);

                if (errorBool && counter2 < iterationLimit)
                {
                    // Calculate new step by multiplying it by "s" REF[1]Eq(31)Page(508)
                    step *= Math.Pow((tolerance * step / (2 * Math.Abs(zState[0] - yState[0]))), 0.25);
                    counter2++;
                }
                else
                {
                    // Add step to current time
                    time += step;

                    // Add the result of y and t to "result"
                    result.y.Add((double[])zState.Clone());
                    result.t.Add(time);
                    repeat = false;
                }
            }
            timeIndex++;
            repeat = true;
        }
        return result;
    }

}

/// <summary>
/// Represents the result of the Runge-Kutta-Fehlberg 4(5) integration method.
/// </summary>
public class Result
{
    /// <summary>
    /// The list of state vectors at each time step.
    /// </summary>
    public List<double[]> y = new List<double[]>();

    /// <summary>
    /// The list of time values corresponding to each state vector.
    /// </summary>
    public List<double> t = new List<double>();
}
