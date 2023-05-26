using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class Spring1D : MonoBehaviour
{
    private float mass;
    [SerializeField] private float stiffness;
    [SerializeField] private float damping;
    private float dampingSimulated = 0f;
    [Tooltip("This is a correction factor. The default value is -0.208f")]
    [SerializeField] private float dampingCorrection = -0.208f; //standard value: -0.208
    [SerializeField] private GameObject anchorObject;
    [SerializeField] private float initialDisplacement;
    private float distance;
    private float time = 0f;
    private double lastPosition;
    private double[] finalState;
    private double[] initialState;
    private Rigidbody rb;
    //[SerializeField] private TMP_Text timeText;
    //public TMP_Text displacementText;
    //public TMP_Text velocityText;

    private Vector3 initialPosition;
    private Vector3 anchorPosition = new Vector3(0f, 0f, 0f);

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        mass = rb.mass;
        initialPosition = transform.position;
        dampingSimulated = damping+dampingCorrection;
        // The initial position is taken directly from the editor (check #IF UNITY_EDITOR)
        Vector3 newPosition = initialPosition;
        newPosition.y += initialDisplacement;
        gameObject.transform.position = newPosition;

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
        RKF45 solver = new RKF45();
        initialState = new double[2];
        initialState[0] = transform.position.y - initialPosition.y;
        initialState[1] = rb.velocity.y;
        distance = Mathf.Abs(initialPosition.y - anchorPosition.y);
        Result result = solver.Solve(springODE, 0, deltaT, initialState, 0.005, 1e-10);
        finalState = result.y[result.y.Count-1];
        time += (float)result.t[result.t.Count-1];
        //displacementText.text = "y="+((float)finalState[0]).ToString();
        //velocityText.text = "vy="+((float)finalState[1]).ToString();
        //timeText.text = "dt=" + deltaT.ToString();

        //Knowing the final state, throw this back to the Physics engine through a new force
        Vector3 force = new Vector3(0f, (float)(-stiffness * finalState[0] - dampingSimulated * finalState[1]), 0f);
        rb.AddForce(force);
    }

    private double[] springODE(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -dampingSimulated / mass * x[1] - stiffness / mass * x[0];
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
    public float GetMass()
    {
        return mass;
    }
    public float GetSimulatedDamping()
    {
        return dampingSimulated;
    }
    public Vector3 GetAnchorPosition()
    {
        return anchorPosition;
    }
    public Vector3 GetInitialPosition()
    {
        return initialPosition;
    }
    public float GetDisplacement()
    {
        return distance;
    }
    public float GetVelocity()
    {
        return (float)finalState[1];
    }
    public double[] GetInitialState()
    {
        return initialState;
    }
    public double[] GetFinalState()
    {
        return finalState;
    }
    public float GetTime()
    {
        return time;
    }
}

