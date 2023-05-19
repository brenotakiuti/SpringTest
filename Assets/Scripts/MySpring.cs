using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class MySpring : MonoBehaviour
{
    public float mass;
    [SerializeField] float stiffness;
    [SerializeField] float damping;
    //[SerializeField] float externalForce;
    public float distance;
    [SerializeField] GameObject anchorObject;
    public double lastPosition;
    public double[] finalState;
    public double[] initialState;
    public Rigidbody rb;
    public TMP_Text displacementText;
    public TMP_Text velocityText;
    public TMP_Text timeText;

    private Vector3 initialPosition;
    private Vector3 anchorPosition = new Vector3(0f, 0f, 0f);

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        mass = rb.mass;
        //initialPosition = transform.position;
        if(anchorObject!=null)
        {
            anchorPosition = anchorObject.transform.position;
        }
        lastPosition = initialPosition.y;
    }

    // FixedUpdate is called in a fixed time steps
    void FixedUpdate()
    {
        //Use the conditions of the rigidbody as initial conditions for the solution of the ODE
        float deltaT = Time.fixedDeltaTime;
        RKF45 solver = new();
        initialState = new double[2];
        initialState[0] = transform.position.y - lastPosition;
        initialState[1] = rb.velocity.y;
        distance = Mathf.Abs(initialPosition.y - anchorPosition.y);
        Result result = solver.Solve(springODE, 0, deltaT, initialState, 0.005, 1e-10);
        finalState = result.y[^1];

        displacementText.text = "y="+((float)finalState[0]).ToString();
        velocityText.text = "vy="+((float)finalState[1]).ToString();
        timeText.text = "dt=" + deltaT.ToString();

        //Knowing the final state, throw this back to the Physics engine through a new force
        Vector3 force = new(0f, (float)(-stiffness * finalState[0] - damping * finalState[1]), 0f);
        rb.AddForce(force);
    }

    private double[] springODE(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -damping / mass * x[1] - stiffness / mass * x[0];
        return y;
    }

    public void applyForce()
    {
        rb.AddForce(0f, 1000, 0f);
    }

#if UNITY_EDITOR
    // This code will only be compiled in the Unity Editor
    private void OnValidate()
    {
        // Update the Gizmo position to match the object's position
        UnityEditor.SceneView.RepaintAll();
        initialPosition = transform.position;
    }
#endif

    private void OnDrawGizmos()
    {
        Vector3 sphereSize = new Vector3(0.2f, 0.2f, 0.2f);

        Gizmos.color = Color.green;
        // Draw the anchor points using Gizmos
        Gizmos.DrawCube(transform.position, sphereSize); // Replace transform.position with the position of your anchor point
        // If you have a second anchor point, you can draw it as well
        Gizmos.DrawCube(anchorPosition, sphereSize); // Replace secondAnchorPosition with the position of your second anchor point
    }
}
