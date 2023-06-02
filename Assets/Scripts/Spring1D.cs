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
    [SerializeField] private float addForceMagnitude = 100f;
    private float dampingSimulated = 0f;
    [Tooltip("This is a correction factor. The default value is -0.208f")]
    [SerializeField] private float dampingCorrection = -0.208f; //standard value: -0.208
    [SerializeField] private GameObject anchorObject;
    [SerializeField] private float initialDisplacement;
    [SerializeField] private bool enableCollision = false;
    private float distanceY;
    private Vector3 initialLength;
    private Vector3 distanceVector;    
    private Vector3 newLength = new Vector3(0f, 0f, 0f);
    private Vector3 distance;
    private float time = 0f;
    private double lastPosition;
    private double[] finalState;
    private double[] initialState;
    private Rigidbody rb;
    private Collider mainCollider;
    private Collider anchorCollider;
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

        UpdateAnchorPosition();
        initialLength = anchorPosition - initialPosition;

        //lastPosition = initialPosition.y;

        mainCollider = GetComponent<Collider>();
        anchorCollider = anchorObject.GetComponent<Collider>();

    }

    private void Update()
    {
        if (enableCollision)
        {
            // If collision is re enabled, set ignore to false
            Physics.IgnoreCollision(mainCollider, anchorCollider, false);
        }
        else
        {
            Physics.IgnoreCollision(mainCollider, anchorCollider, true);
        }
    }

    // FixedUpdate is called in a fixed time steps
    void FixedUpdate()
    {
        UpdateAnchorPosition();
        newLength = anchorPosition - transform.position;

        //Use the conditions of the rigidbody as initial conditions for the solution of the ODE
        float deltaT = Time.fixedDeltaTime;
        distance = initialLength - newLength;

        //distanceY = Mathf.Abs(initialPosition.y - anchorPosition.y);

        // In the Y direction (new)
        ApplyForce(distance);
        time += deltaT;
    }

    private void ApplyForce(Vector3 displacement)
    {
        float deltaT = Time.fixedDeltaTime;
        RKF45 solver = new RKF45();
        
        if(displacement.x != 0)
        {
            double[] initialStateX = new double[2];
            initialStateX[0] = displacement.x;
            initialStateX[1] = rb.velocity.x;
            Result result = solver.Solve(springODE, 0, deltaT, initialStateX, 0.005, 1e-10);
            finalState = result.y[result.y.Count-1];

            //Knowing the final state, throw this back to the Physics engine through a new force
            Vector3 forceX = new Vector3((float)(-stiffness * finalState[0] - dampingSimulated * finalState[1]), 0f, 0f);
            rb.AddForce(forceX);
            anchorObject.GetComponent<Rigidbody>().AddForce(-forceX);
        }

        if(displacement.y != 0)
        {
            double[] initialStateY = new double[2];
            initialStateY[0] = displacement.y;
            initialStateY[1] = rb.velocity.y;
            Result result = solver.Solve(springODE, 0, deltaT, initialStateY, 0.005, 1e-10);
            finalState = result.y[result.y.Count-1];

            //Knowing the final state, throw this back to the Physics engine through a new force
            Vector3 forceY = new Vector3(0f, (float)(-stiffness * finalState[0] - dampingSimulated * finalState[1]), 0f);
            rb.AddForce(forceY);
            anchorObject.GetComponent<Rigidbody>().AddForce(-forceY);
        }

        if(displacement.z != 0)
        {
            double[] initialStateZ = new double[2];
            initialStateZ[0] = displacement.z;
            initialStateZ[1] = rb.velocity.z;
            Result result = solver.Solve(springODE, 0, deltaT, initialStateZ, 0.005, 1e-10);
            finalState = result.y[result.y.Count-1];

            //Knowing the final state, throw this back to the Physics engine through a new force
            Vector3 forceZ = new Vector3(0f, 0f, (float)(-stiffness * finalState[0] - dampingSimulated * finalState[1]));
            rb.AddForce(forceZ);
            anchorObject.GetComponent<Rigidbody>().AddForce(-forceZ);
        }
    }

    private double[] springODE(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -dampingSimulated / mass * x[1] - stiffness / mass * x[0];
        return y;
    }

    public void applyForceY()
    {
        rb.AddForce(0f, addForceMagnitude, 0f);
    }
    public void applyForceX()
    {
        rb.AddForce( addForceMagnitude, 0f, 0f);
    }

    public void applyForceZ()
    {
        rb.AddForce( 0f, 0f, addForceMagnitude);
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
    public void UpdateAnchorPosition()
    {
        if(anchorObject!=null)
        {
            anchorPosition = anchorObject.transform.position;
        }else{
            anchorPosition = Vector3.zero;
        }
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
    public Vector3 GetDistanceVector()
    {
        return distance;
    }
    public Vector3 GetInitialLength()
    {
        return initialLength;
    }
    public Vector3 GetNewLength()
    {
        return newLength;
    }
    public float GetDisplacement()
    {
        return distanceY;
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

