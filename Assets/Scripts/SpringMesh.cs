using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringMesh : Rope
{
    /// <summary>
    /// Draw the shape (function) of the points that each mer must connect. In this case is a parable function
    /// </summary>
    /// <param name="x">x coordinate in the parable function.</param>
    /// <param name="radius">the radius of the parable.</param>
    protected override Vector3 RopeShape(float x, float radius)
    {
        float y = Mathf.Pow((x - radius), 2) / (2 * radius);
        Vector3 vec = new Vector3(x, y - radius / 2);
        return vec;
    }
}
