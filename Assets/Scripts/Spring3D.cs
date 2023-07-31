using Unity.Jobs;
using UnityEngine;

[RequireComponent(typeof(DrawSpringMesh)), RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(RawRotation))]
// This spring works in 3D but it is linear in all direction. Not a precise simulation of 3D, but precise 1D simulation.
public class Spring3D : MonoBehaviour
{
    //Set up parameters
    [Tooltip("Stiffness K of the spring")]
    [SerializeField] private Vector3 linear_K = new Vector3(0f, 1f, 0f);
    [Tooltip("Torsional Stiffness Kt of the spring")]
    [SerializeField] private Vector3 torsional_K = new Vector3(0f, 1f, 0f);
    [Tooltip("Damping C of the spring/damper")]
    [SerializeField] private Vector3 linear_C = new Vector3(0f, 0f, 0f);
    [Tooltip("Torsional Damping Ct of the spring/damper")]
    [SerializeField] private Vector3 torsional_C = new Vector3(0f, 0f, 0f);
    [Tooltip("The magnitude of the force applied by ApplyForce()")]
    [SerializeField] private float addForceMagnitude = 100f;
    [Tooltip("This is a correction factor, use this to eliminate Unity's internal damping. The default value is -0.208f")]
    [SerializeField] private Vector3 dampingCorrection = new Vector3(-0.208f, -0.208f, -0.208f); //standard value: -0.208
    [Tooltip("The object to which this is anchored")]
    [SerializeField] private GameObject anchorObject;
    [Tooltip("Set up if you want to start simulations with a initial displacement")]
    [SerializeField] private float initialDisplacement = 0f;
    [Tooltip("If true, the object will collide with it's anchor")]
    [SerializeField] private bool enableCollision = false;
    //[SerializeField] private DrawSpringMesh drawSpringMesh;

    // Internal parameters
    private float mass;
    private Vector3 J;
    private Vector3 r;
    private Vector3 simulated_C;
    private float time = 0f;
    private double[] linear_initialState; private double[] torsional_initialState;
    private double[] linear_finalState; private double[] torsional_finalState;
    //private double displacementTolerance = 5e-5; 
    private Vector3 newLength = new Vector3(0f, 0f, 0f);
    private Vector3 newRotation;
    //public Quaternion newRotation;
    private Vector3 distance; public Vector3 rotation;
    //public Quaternion rotation;
    private Vector3 springMiddlePoint;

    //ZERO (initial) variables
    private Vector3 initialPosition;
    private Vector3 initialLength;
    private Vector3 anchorPosition = new Vector3(0f, 0f, 0f);
    private Vector3 initialRotation;
    //public Quaternion initialRotation;

    //Reference variables
    private Rigidbody rb;
    private Collider mainCollider; private Collider anchorCollider;
    private RawRotation rawRotation;
    private RawRotation anchorRawRotation;

    //Job variables
    public SpringJobData jData;

    // Start is called before the first frame update
    void Start()
    {
        //StartScript();
    }

    public void StartScript()
    {
        //drawSpringMesh = GetComponent<DrawSpringMesh>();
        rb = gameObject.GetComponent<Rigidbody>();
        rawRotation = GetComponent<RawRotation>();
        anchorRawRotation = anchorObject.GetComponent<RawRotation>();
        mass = rb.mass;
        initialPosition = transform.position;

        //drawSpringMesh.AnchorObject = anchorObject;
        //drawSpringMesh.BaseObject = gameObject;

        simulated_C = linear_C + dampingCorrection;

        // The initial position is taken directly from the editor (check #IF UNITY_EDITOR)
        Vector3 newPosition = initialPosition;
        newPosition.y += initialDisplacement;
        gameObject.transform.position = newPosition;

        UpdateAnchorPosition();
        J = CalculateJ();
        initialLength = anchorPosition - initialPosition;
        //initialRotation = anchorObject.transform.eulerAngles - transform.eulerAngles;
        //initialRotation = transform.rotation*Quaternion.Inverse(anchorObject.transform.rotation);
        //initialRotation = anchorRawRotation.GetRawQuaternion() * Quaternion.Inverse(rawRotation.GetRawQuaternion());
        initialRotation = RawRotation.CalculateRotationDifference(anchorObject.transform.rotation, transform.rotation, 0);
        //initialRotation = new Vector3(anchorObject.transform.eulerAngles.x-transform.eulerAngles.x,anchorObject.transform.eulerAngles.y-transform.eulerAngles.y,anchorObject.transform.eulerAngles.z-transform.eulerAngles.z);

        springMiddlePoint = Vector3.Lerp(initialPosition, anchorPosition, 0.5f);

        mainCollider = GetComponent<Collider>();
        anchorCollider = anchorObject.GetComponent<Collider>();

        UpdateJDataStruct();
    }

    void FixedUpdate()
    {
        //FixedUpdateScript();
    }

    public void FixedUpdateScript()
    {
        //Collision ignore part
        if (enableCollision)
        {
            // If collision is re enabled, set ignore to false
            Physics.IgnoreCollision(mainCollider, anchorCollider, false);
        }
        else
        {
            Physics.IgnoreCollision(mainCollider, anchorCollider, true);
        }

        // The real deal
        UpdateAnchorPosition();
        newLength = anchorPosition - transform.position;
        //newRotation = new Vector3(anchorObject.transform.eulerAngles.x-transform.eulerAngles.x,anchorObject.transform.eulerAngles.y-transform.eulerAngles.y,anchorObject.transform.eulerAngles.z-transform.eulerAngles.z);
        //newRotation = transform.rotation * Quaternion.Inverse(anchorObject.transform.rotation);
        //newRotation = anchorRawRotation.GetRawQuaternion() * Quaternion.Inverse(rawRotation.GetRawQuaternion());
        newRotation = RawRotation.CalculateRotationDifference(transform.rotation,anchorObject.transform.rotation, 0);
        Debug.Log("Anchor rotation=" + anchorObject.transform.eulerAngles.x + ", " + anchorObject.transform.eulerAngles.y+ ", " + anchorObject.transform.eulerAngles.z);

        //Use the conditions of the rigidbody as initial conditions for the solution of the ODE
        float deltaT = Time.fixedDeltaTime;
        distance = initialLength - newLength;
        rotation = initialRotation + newRotation;
        //rotation = (initialRotation * Quaternion.Inverse(newRotation)).eulerAngles;
        //rotation = RawRotation.CalculateRotationDifference(newRotation, initialRotation,  0) * Mathf.PI / 180;
        //rotation = RawRotation.CalculateRotationDifference(transform.rotation, anchorObject.transform.rotation,  0) * Mathf.PI / 180;

        float[] displacements6DOF = TwoVector3stoArray(distance, rotation);
        float[] velocitys6DOF = TwoVector3stoArray(rb.velocity, rb.angularVelocity);
        float[] arrayK = TwoVector3stoArray(linear_K, torsional_K);
        float[] arrayC = TwoVector3stoArray(simulated_C, torsional_C);
        //Debug.Log("Displacement 6DOF = " + displacements6DOF[0] +", "+ displacements6DOF[1] +", "+ displacements6DOF[2] +", "+ displacements6DOF[3] +", "+ displacements6DOF[4] +", "+ displacements6DOF[5]);

        // In the Y direction (new)
        //ApplyForce(distance);
        //ApplyTorque(rotation);
        float[] arrayForces = ArrayCalculateForce(displacements6DOF, velocitys6DOF, arrayK, arrayC);
        NewApplyForce(arrayForces);

        time += deltaT;
    }

    private float[] TwoVector3stoArray(Vector3 vec1, Vector3 vec2)
    {
        float[] result = new float[6];
        result[0] = vec1.x;
        result[1] = vec1.y;
        result[2] = vec1.z;
        result[3] = vec2.x;
        result[4] = vec2.y;
        result[5] = vec2.z;
        return result;
    }

    // When flag is true, return linear forces. If it is false, return rotation forces (torques).
    private Vector3 ArrayToTwoVector3s(float[] array, bool flag)
    {
        Vector3 result = new Vector3();
        if (flag)
        {
            result.x = array[0];
            result.y = array[1];
            result.z = array[2];
        }
        else
        {
            result.x = array[3];
            result.y = array[4];
            result.z = array[5];
        }
        return result;
    }

    private Result SolveODE6DOF(int i, float deltaT, double[] initialState)
    {
        //RKF45 solver = new RKF45();
        RK4 solver = new RK4();

        Result result;
        switch (i)
        {
            default:
                result = solver.Solve(linearODEx, 0, deltaT, initialState, 0.005);
                //result = solver.Solve(linearODEx, 0, deltaT, initialState, 0.005, 1e-10);
                break;
            case 1:
                result = solver.Solve(linearODEy, 0, deltaT, initialState, 0.005);
                //result = solver.Solve(linearODEy, 0, deltaT, initialState, 0.005, 1e-10);
                break;
            case 2:
                result = solver.Solve(linearODEz, 0, deltaT, initialState, 0.005);
                //result = solver.Solve(linearODEz, 0, deltaT, initialState, 0.005, 1e-10);
                break;

            case 3:
                result = solver.Solve(torsionalODEx, 0, deltaT, initialState, 0.005);
                //result = solver.Solve(torsionalODEx, 0, deltaT, initialState, 0.005, 1e-10);
                break;
            case 4:
                result = solver.Solve(torsionalODEy, 0, deltaT, initialState, 0.005);
                //result = solver.Solve(torsionalODEy, 0, deltaT, initialState, 0.005, 1e-10);
                break;
            case 5:
                result = solver.Solve(torsionalODEz, 0, deltaT, initialState, 0.005);
                //result = solver.Solve(torsionalODEz, 0, deltaT, initialState, 0.005, 1e-10);
                break;
        }
        return result;
    }

    private float[] ArrayCalculateForce(float[] displacement, float[] velocity, float[] arrayK, float[] arrayC)
    {
        float deltaT = Time.fixedDeltaTime;
        int n = displacement.Length;
        float[] thisResult = new float[n];

        for (int i = 0; i < n; i++)
        {
            if (displacement[i] != 0)
            {
                double[] initialState = new double[2];
                initialState[0] = displacement[i];
                initialState[1] = velocity[i];

                // Solve
                Result result = SolveODE6DOF(i,deltaT,initialState);

                linear_finalState = result.y[result.y.Count - 1];

                //Knowing the final state, throw this back to the Physics engine through a new force
                float springDamperForce = (float)(-arrayK[i] * linear_finalState[0] - arrayC[i] * linear_finalState[1]);

                if (float.IsNaN(springDamperForce))
                {
                    springDamperForce = 0;
                }

                thisResult[i] = springDamperForce;
            }
        }
        return thisResult;
    }

    private void NewApplyForce(float[] forces)
    {
        Vector3 linearForces = ArrayToTwoVector3s(forces, true);
        Vector3 angularForces = ArrayToTwoVector3s(forces, false);

        rb.AddForce(linearForces);
        anchorObject.GetComponent<Rigidbody>().AddForce(-linearForces);
        rb.AddTorque(angularForces);
        anchorObject.GetComponent<Rigidbody>().AddTorque(-angularForces);
    }

    private void ApplyForce(Vector3 displacement)
    {
        float deltaT = Time.fixedDeltaTime;
        RKF45 solver = new RKF45();

        if (displacement.x != 0)
        {
            double[] initialStateX = new double[2];
            initialStateX[0] = displacement.x;
            initialStateX[1] = rb.velocity.x;

            Result result = solver.Solve(linearODEx, 0, deltaT, initialStateX, 0.005, 1e-10);
            linear_finalState = result.y[result.y.Count - 1];


            //Knowing the final state, throw this back to the Physics engine through a new force
            float springDamperForce = (float)(-linear_K.x * linear_finalState[0] - simulated_C.x * linear_finalState[1]);

            if (float.IsNaN(springDamperForce))
            {
                springDamperForce = 0;
            }
            Vector3 forceX = new Vector3(springDamperForce, 0f, 0f);

            rb.AddForce(forceX);
            anchorObject.GetComponent<Rigidbody>().AddForce(-forceX);
        }

        if (displacement.y != 0)
        {
            double[] initialStateY = new double[2];
            initialStateY[0] = displacement.y;
            initialStateY[1] = rb.velocity.y;
            Result result = solver.Solve(linearODEy, 0, deltaT, initialStateY, 0.005, 1e-10);
            linear_finalState = result.y[result.y.Count - 1];

            //Knowing the final state, throw this back to the Physics engine through a new force
            float springDamperForce = (float)(-linear_K.y * linear_finalState[0] - simulated_C.y * linear_finalState[1]);

            if (float.IsNaN(springDamperForce))
            {
                springDamperForce = 0;
            }
            Vector3 forceY = new Vector3(0f, springDamperForce, 0f);

            rb.AddForce(forceY);
            anchorObject.GetComponent<Rigidbody>().AddForce(-forceY);
        }

        if (displacement.z != 0)
        {
            double[] initialStateZ = new double[2];
            initialStateZ[0] = displacement.z;
            initialStateZ[1] = rb.velocity.z;
            Result result = solver.Solve(linearODEz, 0, deltaT, initialStateZ, 0.005, 1e-10);
            linear_finalState = result.y[result.y.Count - 1];


            //Knowing the final state, throw this back to the Physics engine through a new force
            float springDamperForce = (float)(-linear_K.z * linear_finalState[0] - simulated_C.z * linear_finalState[1]);

            if (float.IsNaN(springDamperForce))
            {
                springDamperForce = 0;
            }
            Vector3 forceZ = new Vector3(0f, 0f, springDamperForce);

            rb.AddForce(forceZ);
            anchorObject.GetComponent<Rigidbody>().AddForce(-forceZ);
        }
    }

    private void ApplyTorque(Vector3 rotation)
    {
        float deltaT = Time.fixedDeltaTime;
        RKF45 solver = new RKF45();

        if (rotation.x != 0)
        {
            double[] initialStateX = new double[2];
            initialStateX[0] = rotation.x;
            initialStateX[1] = rb.angularVelocity.x;
            Result result = solver.Solve(torsionalODEx, 0, deltaT, initialStateX, 0.005, 1e-10);
            torsional_finalState = result.y[result.y.Count - 1];

            //Knowing the final state, throw this back to the Physics engine through a new force
            float springDamperTorque = (float)(-torsional_K.x * torsional_finalState[0] - torsional_C.x * torsional_finalState[1]);


            if (float.IsNaN(springDamperTorque))
            {
                springDamperTorque = 0;
            }
            Vector3 torqueX = new Vector3(springDamperTorque, 0f, 0f);

            rb.AddTorque(torqueX);
            anchorObject.GetComponent<Rigidbody>().AddTorque(-torqueX);
        }

        if (rotation.y != 0)
        {
            double[] initialStateY = new double[2];
            initialStateY[0] = rotation.y;
            initialStateY[1] = rb.angularVelocity.y;
            Result result = solver.Solve(torsionalODEy, 0, deltaT, initialStateY, 0.005, 1e-10);
            torsional_finalState = result.y[result.y.Count - 1];

            //Knowing the final state, throw this back to the Physics engine through a new force
            float springDamperTorque = (float)(-torsional_K.y * torsional_finalState[0] - torsional_C.y * torsional_finalState[1]);


            if (float.IsNaN(springDamperTorque))
            {
                springDamperTorque = 0;
            }
            Vector3 torqueY = new Vector3(0f, springDamperTorque, 0f);

            rb.AddTorque(torqueY);
            anchorObject.GetComponent<Rigidbody>().AddTorque(-torqueY);
        }

        if (rotation.z != 0)
        {
            double[] initialStateZ = new double[2];
            initialStateZ[0] = rotation.z;
            initialStateZ[1] = rb.angularVelocity.z;
            Result result = solver.Solve(torsionalODEz, 0, deltaT, initialStateZ, 0.005, 1e-10);
            torsional_finalState = result.y[result.y.Count - 1];

            //Knowing the final state, throw this back to the Physics engine through a new force
            float springDamperTorque = (float)(-torsional_K.z * torsional_finalState[0] - torsional_C.z * torsional_finalState[1]);


            if (float.IsNaN(springDamperTorque))
            {
                springDamperTorque = 0;
            }
            Vector3 torqueZ = new Vector3(0f, 0f, springDamperTorque);

            rb.AddTorque(torqueZ);
            anchorObject.GetComponent<Rigidbody>().AddTorque(-torqueZ);
        }
    }
    private Vector3 CalculateJ()
    {
        Vector3 scale = transform.localScale;
        Vector3 result = new Vector3(0f, 0f, 0f);
        result.x = mass * Mathf.Pow(scale.y, 2) * Mathf.Pow(scale.z, 2) / 12;
        result.y = mass * Mathf.Pow(scale.x, 2) * Mathf.Pow(scale.z, 2) / 12;
        result.z = mass * Mathf.Pow(scale.x, 2) * Mathf.Pow(scale.y, 2) / 12;
        return result;
    }

    // Linear ODEs
    private double[] linearODEx(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -simulated_C.x / mass * x[1] - linear_K.x / mass * x[0];
        return y;
    }
    private double[] linearODEy(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -simulated_C.y / mass * x[1] - linear_K.y / mass * x[0];
        return y;
    }
    private double[] linearODEz(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -simulated_C.z / mass * x[1] - linear_K.z / mass * x[0];
        return y;
    }

    // Torsional ODEs
    private double[] torsionalODEx(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -torsional_C.x / J.x * x[1] - torsional_K.x / J.x * x[0];
        return y;
    }
    private double[] torsionalODEy(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -torsional_C.y / J.y * x[1] - torsional_K.y / J.y * x[0];
        return y;
    }
    private double[] torsionalODEz(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -torsional_C.z / J.z * x[1] - torsional_K.z / J.z * x[0];
        return y;
    }

    public void ApplyForceY()
    {
        rb.AddForce(0f, addForceMagnitude, 0f);
    }
    public void ApplyForceX()
    {
        rb.AddForce(addForceMagnitude, 0f, 0f);
    }

    public void ApplyForceZ()
    {
        rb.AddForce(0f, 0f, addForceMagnitude);
    }

    private Vector3 CalculateR()
    {
        Vector3 result = new Vector3(0f, 0f, 0f);
        result.x = transform.localScale.x / 2;
        result.y = transform.localScale.y / 2;
        result.z = transform.localScale.z / 2;

        return result;
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
        if (anchorObject != null)
        {
            anchorPosition = anchorObject.transform.position;
        }
        else
        {
            anchorPosition = Vector3.zero;
        }
    }

    public void UpdateJDataStruct()
    {
        jData.transform = transform;
        jData.rb = rb;
        jData.anchorObject = anchorObject;
        jData.fixedDeltaTime = Time.fixedDeltaTime;
        jData.anchorPosition = anchorPosition;
        jData.initialLength = initialLength;
        jData.J = J;
        jData.simulated_C = simulated_C;
        jData.torsional_C = torsional_C;
        jData.linear_K = linear_K;
        jData.torsional_K = torsional_K;
        jData.rawRotation = rawRotation;
        jData.mass = mass;
        jData.addForceMagnitude = addForceMagnitude;
        jData.fixedTime = Time.fixedDeltaTime;
        jData.linear_finalState = linear_finalState;
        jData.torsional_finalState = linear_finalState;
    }

    public GameObject GetAnchorObject()
    {
        return anchorObject;
    }

}

