using System;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(DrawSpringMesh)), RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(RawRotation))]
// This spring works in 3D but it is linear in all direction. Not a precise simulation of 3D, but precise 1D simulation.
public class Spring3DMatrix : MonoBehaviour
{
    //Set up parameters
    [Tooltip("k values")]
    [SerializeField] private double[] k = new double[9];
    [Tooltip("c values")]
    [SerializeField] private double[] c = new double[9];
    [Tooltip("spring radius r")]
    [SerializeField] private float r;
    private float J;
    [Tooltip("The magnitude of the force applied by ApplyForce()")]
    [SerializeField] private double addForceMagnitude = 100f;
    [Tooltip("This is a correction factor, use this to eliminate Unity's internal damping. The default value is -0.208f")]
    [SerializeField] private double dampingCorrection = -0.208f; //standard value: -0.208
    [Tooltip("The object to which this is anchored")]
    [SerializeField] private GameObject anchorObject;
    [Tooltip("Set up if you want to start simulations with a initial displacement")]
    [SerializeField] private double[] initialDisplacement = new double[] { 0f, 0f, 0f };
    [Tooltip("If true, the object will collide with it's anchor")]
    [SerializeField] private bool enableCollision = false;

    // Internal parameters
    private float mass;
    private double[,] M; private double[,] K; private double[,] C; private double[,] A1;
    private float dampingSimulated = 0f;
    private Vector3 initialLength;
    private Vector3 newLength = new Vector3(0f, 0f, 0f);
    private Vector3 distance; private Vector3 rotation;
    private float time = 0f;
    private double[] finalState; private double[] initialState;

    //Reference variables
    private Rigidbody rb;
    private Collider mainCollider; private Collider anchorCollider;
    private DrawSpringMesh drawSpringMesh;
    private RawRotation rawRotation;

    //ZERO variables
    private Vector3 initialPosition; private Vector3 initialRotation;
    private Vector3 anchorPosition = new Vector3(0f, 0f, 0f);

    // Start is called before the first frame update
    void Start()
    {
        drawSpringMesh = GetComponent<DrawSpringMesh>();
        rb = GetComponent<Rigidbody>();
        rawRotation = GetComponent<RawRotation>();
        mass = rb.mass;
        //J = Mathf.PI*Mathf.Pow(r,4)/4;
        M = new double[,]
        {
            { mass, 0,0},
            { 0, mass,0},
            { 0, 0, mass}
        };
        //Matrix.PrintMatrix("Mass Matrix:", M);
        K = Matrix.ArrayToMatrix(k);
        C = Matrix.ArrayToMatrix(c);
        int size = K.GetLength(0);
        A1 = Matrix.ConcatMatrices(Matrix.Zeroes(size), Matrix.Eye(size), Matrix.MultiScalarMatrix(-1, Matrix.Multiply(Matrix.DiagonalInverse(M), K)), Matrix.MultiScalarMatrix(-1, Matrix.Multiply(Matrix.DiagonalInverse(M), C)));
        Matrix.PrintMatrix("A1: ", A1);

        initialPosition = transform.position;
        //initialRotation = transform.rotation.eulerAngles*Mathf.PI/180;

        drawSpringMesh.AnchorObject = anchorObject;
        drawSpringMesh.BaseObject = gameObject;

        //dampingSimulated = C+dampingCorrection;
        // The initial position is taken directly from the editor (check #IF UNITY_EDITOR)
        Vector3 newPosition = initialPosition;
        newPosition.y += (float)initialDisplacement[0];
        gameObject.transform.position = newPosition;

        UpdateAnchorPosition();
        initialLength = anchorPosition - initialPosition;
        
        mainCollider = GetComponent<Collider>();
        anchorCollider = anchorObject.GetComponent<Collider>();
        transform.position += Matrix.ArrayToVector3(initialDisplacement)[0];
        //rb.AddForce(new Vector3(0f,0.1f,0f));
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
        rotation = rawRotation.GetRawRotation() * Mathf.PI / 180;

        // In the Y direction (new)
        ApplyForce(distance, rotation);
        time += deltaT;
    }

    private void ApplyForce(Vector3 displacement, Vector3 angles)
    {
        float deltaT = Time.fixedDeltaTime;
        RKF45 solver = new RKF45();

        double[] initialState = new double[6];

        //if (displacement.y != 0 || displacement.x != 0)
        //{
            initialState[0] = displacement.x+0.00001;
            initialState[1] = displacement.y+0.00001;
            initialState[2] = displacement.z+0.00001;
            initialState[3] = rb.velocity.x+0.00001;
            initialState[4] = rb.velocity.y+0.00001;
            initialState[5] = rb.velocity.z+0.00001;
            //initialState[1] = angles.z;
            //initialState[2] = rb.velocity.y;
            //initialState[3] = rb.angularVelocity.z;
            //Matrix.PrintArray("Initial state:", initialState); //ok
            //Matrix.PrintArray("A1*x:", Matrix.MultiMatrixArray(A1,initialState));
            Result result = solver.Solve(springODE, 0, deltaT, initialState, deltaT/10, 1e-10);
            finalState = result.y[result.y.Count - 1];
            //Matrix.PrintArray("Final state:", finalState);
            //Knowing the final state, throw this back to the Physics engine through a new force
            //Matrix.PrintMatrix("KC: ", Matrix.ConcatMatricesInLine(K, C)); //ok
            double[] forceMatrix = Matrix.MultiMatrixArray(Matrix.MultiScalarMatrix(-1, Matrix.ConcatMatricesInLine(K, C)), finalState);
            //Matrix.PrintArray("Force Matrix: ", forceMatrix);
            // float springDamperForce = (float)forceMatrix[0];
            // float springDamperTorque = (float)forceMatrix[1];

            //ArraySegment<double> segment = new ArraySegment<double>(forceMatrix, 0, 3);
            //double[] springDamperForce = segment.Select(element => (double)element).ToArray();

            // if (float.IsNaN(springDamperForce))
            // {
            //     springDamperForce = 0;
            // }
            //springDamperForce = springDamperForce.Select(element => double.IsNaN(element) ? 0.0f : element).ToArray();
            forceMatrix = forceMatrix.Select(element => double.IsNaN(element) ? 0.0f : element).ToArray();

            //Debug.Log(springDamperForce);
            Vector3 force = new Vector3((float)forceMatrix[0], (float)forceMatrix[1], (float)forceMatrix[2]);
            // torqueZ = new Vector3(0f,0f,springDamperTorque);
            //Vector3[] force = Matrix.ArrayToVector3(forceMatrix);


            rb.AddForce(force);
            //rb.AddTorque(force[1]);
            anchorObject.GetComponent<Rigidbody>().AddForce(-force);
            //anchorObject.GetComponent<Rigidbody>().AddTorque(-force[1]);
        //}

        // if(displacement.x != 0)
        // {
        //     initialState[0] = displacement.x;
        //     initialState[2] = rb.velocity.x;
        //     Result result = solver.Solve(springODE, 0, deltaT, initialState, 0.005, 1e-10);
        //     finalState = result.y[result.y.Count-1];

        //     //Knowing the final state, throw this back to the Physics engine through a new force
        //     //float springDamperForce = (float)(-K * finalState[0] - dampingSimulated * finalState[1]);
        //     float springDamperForce = 0;

        //     if (float.IsNaN(springDamperForce))
        //     {
        //         springDamperForce = 0;
        //     }


        //     Vector3 forceX = new Vector3(springDamperForce, 0f, 0f);
        //     rb.AddForce(forceX);
        //     anchorObject.GetComponent<Rigidbody>().AddForce(-forceX);
        // }

        // if(displacement.z != 0)
        // {
        //     initialState[0] = displacement.z;
        //     initialState[2] = rb.velocity.z;
        //     Result result = solver.Solve(springODE, 0, deltaT, initialState, 0.005, 1e-10);
        //     finalState = result.y[result.y.Count-1];

        //     //Knowing the final state, throw this back to the Physics engine through a new force
        //     //float springDamperForce = (float)(-K * finalState[0] - dampingSimulated * finalState[1]);
        //     float springDamperForce = 0;
        //     if (float.IsNaN(springDamperForce))
        //     {
        //         springDamperForce = 0;
        //     }

        //     Vector3 forceZ = new Vector3(0f, 0f, springDamperForce);
        //     rb.AddForce(forceZ);
        //     anchorObject.GetComponent<Rigidbody>().AddForce(-forceZ);
        // }
    }

    private double[] springODE(double t, double[] x)
    {
        double[] y = (double[])x.Clone();

        y = Matrix.MultiMatrixArray(A1, x);
        //y[0] = x[1];
        //y[1] = -dampingSimulated / mass * x[1] - K / mass * x[0];
        return y;
    }

    // public void applyForceY()
    // {
    //     rb.AddForce(0f, addForceMagnitude, 0f);
    // }
    // public void applyForceX()
    // {
    //     rb.AddForce( addForceMagnitude, 0f, 0f);
    // }

    // public void applyForceZ()
    // {
    //     rb.AddForce( 0f, 0f, addForceMagnitude);
    // }

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
        if (anchorObject != null)
        {
            anchorPosition = anchorObject.transform.position;
        }
        else
        {
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
    public Vector3 GetRotationDifference()
    {
        return rotation;
    }
    public Vector3 GetInitialLength()
    {
        return initialLength;
    }
    public Vector3 GetNewLength()
    {
        return newLength;
    }
    //public float GetDisplacement()
    //{
    //    return distanceY;
    //}
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

