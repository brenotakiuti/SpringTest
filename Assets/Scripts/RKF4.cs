//using System.Collections;
using System.Collections.Generic;
//using UnityEngine;
using System;
using System.Linq;

//using MathNet.Numerics.LinearAlgebra;

/// <summary>
/// Implements the Runge-Kutta4 numerical integration method. [1]Numerical methods using MATLAB/John H. Mathews, Kurtis D. Fink—4th ed.
/// </summary>
public class RK4
{
    // state = {x,x'}
    // derivative = {x',x''}

    /// <summary>
    /// Solves an ordinary differential equation using the Runge-Kutta4 method.
    /// </summary>
    /// <param name="odeFunction">The function that defines the ordinary differential equation. It takes the current time and state as parameters and returns the derivative.</param>
    /// <param name="startTime">The start time of the integration.</param>
    /// <param name="endTime">The end time of the integration.</param>
    /// <param name="initialState">The initial state of the system.</param>
    /// <param name="stepSize">The initial step size for the integration.</param>
    /// <returns>A Result object containing the time steps and corresponding states.</returns>
    public Result Solve(Func<double, double[], double[]> odeFunction, double startTime, double endTime, double[] initialState, double stepSize)
    {
        //odeFunction(time,state), return final state

        double[] state = (double[])initialState.Clone();    //Do NOT EVER assign an array with another array just to "copy it". In C# double[] state = initialState assigns the reference of initialState to state, making them essentially the same thing (literally the same position in the memory)
        int n = state.Length;
        List<double[]> finalState = new List<double[]>();
        double[] yState = new double[n];
        double time = startTime;
        double[] f1, f2, f3, f4;
        int timeIndex = 1;
        Result result = new Result();

        //Start solving
        result.y.Add((double[])initialState.Clone());
        result.t.Add(startTime);
        while (time < endTime)
        {
            Array.Copy(result.y[timeIndex - 1], state, state.Length);

            // Copy the values in state to yState and zState to RESET these 2
            Array.Copy(state, yState, state.Length);

            // Reset newTime and newState. Those are used to calculate f(newTime, newState)
            double newTime = time + stepSize;
            double[] newState = (double[])state.Clone();

            // Calculation of k_n REF[1]Eq(28)Page(508)
            //f1
            f1 = odeFunction(newTime, state);

            //f2
            newTime = time + stepSize / 2;
            newState[0] = state[0] + f1[0] * stepSize / 2;
            newState[1] = state[1] + f1[1] * stepSize / 2;
            f2 = odeFunction(newTime, newState);

            //f3
            newTime = time + stepSize / 2;
            newState[0] = state[0] + f2[0] * stepSize / 2;
            newState[1] = state[1] + f2[1] * stepSize / 2;
            f3 = odeFunction(newTime, newState);

            //f4
            newTime = time + stepSize;
            newState[0] = state[0] + f3[0] * stepSize;
            newState[1] = state[1] + f3[1] * stepSize;
            f4 = odeFunction(newTime, newState);

            // Calculation of y_k+1 REF[1]Eq(6)Page(502) 
            yState[0] = state[0] + stepSize * (f1[0] + 2 * f2[0] + 2 * f3[0] + f4[0]) / 6;
            yState[1] = state[1] + stepSize * (f1[1] + 2 * f2[1] + 2 * f3[1] + f4[1]) / 6;

            // Add step to current time
            time += stepSize;

            // Add the result of y and t to "result"
            result.y.Add((double[])yState.Clone());
            result.t.Add(time);

            timeIndex++;
        }
        return result;
    }

}
