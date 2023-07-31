using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class SpringManager : MonoBehaviour
{
    [SerializeField] private bool useJobSystem = false;
    public MonoBehaviour[] targetScripts;
    // Start is called before the first frame update
    void Start()
    {
        MonoBehaviour[] scripts = FindObjectsOfType<MonoBehaviour>();

        // Filter scripts that match the target script type
        targetScripts = System.Array.FindAll(scripts, script => script.GetType() == typeof(Spring3D));
        foreach (Spring3D spring in targetScripts)
        {
            spring.StartScript();
        }

    }

    void FixedUpdate()
    {
        if (!useJobSystem)
        {
            foreach (Spring3D spring in targetScripts)
            {

                spring.FixedUpdateScript();

            }
        }
        else
        {
            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
            foreach (Spring3D spring in targetScripts)
            {
                JobHandle jobHandle = Spring3DUpdate(spring.jData);
                jobHandleList.Add(jobHandle);
            }
            JobHandle.CompleteAll(jobHandleList);
            jobHandleList.Dispose();
        }
    }

    private JobHandle Spring3DUpdate(SpringJobData jdata)
    {
        Spring3DJob job = new Spring3DJob(jdata);
        return job.Schedule();
    }
}

// From here starts the JOBs version of this script
public struct SpringJobData
{
    public Transform transform;
    public Rigidbody rb;
    public GameObject anchorObject;
    public float fixedDeltaTime;
    public Vector3 anchorPosition;
    public Vector3 initialLength;
    public Vector3 J;
    public Vector3 simulated_C;
    public Vector3 torsional_C;
    public Vector3 linear_K;
    public Vector3 torsional_K;
    public RawRotation rawRotation;
    public float mass;
    public float addForceMagnitude;
    public float fixedTime;
    //private double[] linear_initialState; private double[] torsional_initialState;
    public double[] linear_finalState; public double[] torsional_finalState;
}

public struct Spring3DJob : IJob
{
    public SpringJobData jData;

    public Spring3DJob(SpringJobData jdata)
    {
        this.jData = jdata;
    }

    public void Execute()
    {
        UpdateAnchorPosition();
        Vector3 newLength = jData.anchorPosition - jData.transform.position;

        //Use the conditions of the rigidbody as initial conditions for the solution of the ODE
        //float deltaT = jData.fixedDeltaTime;
        Vector3 distance = jData.initialLength - newLength;
        Vector3 rotation = jData.rawRotation.GetRawRotation() * Mathf.PI / 180; 

        // In the Y direction (new)
        ApplyForce(distance);
        ApplyTorque(rotation);
        //jData.time += deltaT;
    }
    private void ApplyForce(Vector3 displacement)
    {
        float deltaT = Time.fixedDeltaTime;
        RKF45 solver = new RKF45();

        if (displacement.x != 0)
        {
            double[] initialStateX = new double[2];
            initialStateX[0] = displacement.x;
            initialStateX[1] = jData.rb.velocity.x;
            Result result = solver.Solve(linearODEx, 0, deltaT, initialStateX, 0.005, 1e-10);
            jData.linear_finalState = result.y[result.y.Count - 1];


            //Knowing the final state, throw this back to the Physics engine through a new force
            float springDamperForce = (float)(-jData.linear_K.x * jData.linear_finalState[0] - jData.simulated_C.x * jData.linear_finalState[1]);
            //Vector3 torqueFromForce = new Vector3(0f, -linear_K.x * springMiddlePoint.z*(float)linear_finalState[0], -linear_K.x * springMiddlePoint.y* (float)linear_finalState[0]);

            if (float.IsNaN(springDamperForce))
            {
                springDamperForce = 0;
            }
            Vector3 forceX = new Vector3(springDamperForce, 0f, 0f);

            jData.rb.AddForce(forceX);
            //rb.AddTorque(torqueFromForce);
            jData.anchorObject.GetComponent<Rigidbody>().AddForce(-forceX);
            //anchorObject.GetComponent<Rigidbody>().AddTorque(-torqueFromForce);
        }

        if (displacement.y != 0)
        {
            double[] initialStateY = new double[2];
            initialStateY[0] = displacement.y;
            initialStateY[1] = jData.rb.velocity.y;
            Result result = solver.Solve(linearODEy, 0, deltaT, initialStateY, 0.005, 1e-10);
            jData.linear_finalState = result.y[result.y.Count - 1];

            //Knowing the final state, throw this back to the Physics engine through a new force
            float springDamperForce = (float)(-jData.linear_K.y * jData.linear_finalState[0] - jData.simulated_C.y * jData.linear_finalState[1]);
            //Vector3 torqueFromForce = new Vector3(linear_K.y * springMiddlePoint.z, 0f,linear_K.y * springMiddlePoint.x);

            if (float.IsNaN(springDamperForce))
            {
                springDamperForce = 0;
            }
            Vector3 forceY = new Vector3(0f, springDamperForce, 0f);

            jData.rb.AddForce(forceY);
            //rb.AddTorque(torqueFromForce);
            jData.anchorObject.GetComponent<Rigidbody>().AddForce(-forceY);
            //anchorObject.GetComponent<Rigidbody>().AddTorque(-torqueFromForce);
        }

        if (displacement.z != 0)
        {
            double[] initialStateZ = new double[2];
            initialStateZ[0] = displacement.z;
            initialStateZ[1] = jData.rb.velocity.z;
            Result result = solver.Solve(linearODEz, 0, deltaT, initialStateZ, 0.005, 1e-10);
            jData.linear_finalState = result.y[result.y.Count - 1];


            //Knowing the final state, throw this back to the Physics engine through a new force
            float springDamperForce = (float)(-jData.linear_K.z * jData.linear_finalState[0] - jData.simulated_C.z * jData.linear_finalState[1]);
            //Vector3 torqueFromForce = new Vector3(linear_K.z * springMiddlePoint.y, linear_K.z * springMiddlePoint.x, 0f);

            if (float.IsNaN(springDamperForce))
            {
                springDamperForce = 0;
            }
            Vector3 forceZ = new Vector3(0f, 0f, springDamperForce);

            jData.rb.AddForce(forceZ);
            //rb.AddTorque(torqueFromForce);
            jData.anchorObject.GetComponent<Rigidbody>().AddForce(-forceZ);
            //anchorObject.GetComponent<Rigidbody>().AddTorque(-torqueFromForce);
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
            initialStateX[1] = jData.rb.angularVelocity.x;
            Result result = solver.Solve(torsionalODEx, 0, deltaT, initialStateX, 0.005, 1e-10);
            jData.torsional_finalState = result.y[result.y.Count - 1];


            //Knowing the final state, throw this back to the Physics engine through a new force
            float springDamperTorque = (float)(-jData.torsional_K.x * jData.torsional_finalState[0] - jData.torsional_C.x * jData.torsional_finalState[1]);


            if (float.IsNaN(springDamperTorque))
            {
                springDamperTorque = 0;
            }
            Vector3 torqueX = new Vector3(springDamperTorque, 0f, 0f);

            jData.rb.AddTorque(torqueX);
            jData.anchorObject.GetComponent<Rigidbody>().AddTorque(-torqueX);
        }

        if (rotation.y != 0)
        {
            double[] initialStateY = new double[2];
            initialStateY[0] = rotation.y;
            initialStateY[1] = jData.rb.angularVelocity.y;
            Result result = solver.Solve(torsionalODEy, 0, deltaT, initialStateY, 0.005, 1e-10);
            jData.torsional_finalState = result.y[result.y.Count - 1];


            //Knowing the final state, throw this back to the Physics engine through a new force
            float springDamperTorque = (float)(-jData.torsional_K.y * jData.torsional_finalState[0] - jData.torsional_C.y * jData.torsional_finalState[1]);


            if (float.IsNaN(springDamperTorque))
            {
                springDamperTorque = 0;
            }
            Vector3 torqueY = new Vector3(0f, springDamperTorque, 0f);

            jData.rb.AddTorque(torqueY);
            jData.anchorObject.GetComponent<Rigidbody>().AddTorque(-torqueY);
        }

        if (rotation.z != 0)
        {
            double[] initialStateZ = new double[2];
            initialStateZ[0] = rotation.z;
            initialStateZ[1] = jData.rb.angularVelocity.z;
            Result result = solver.Solve(torsionalODEz, 0, deltaT, initialStateZ, 0.005, 1e-10);
            jData.torsional_finalState = result.y[result.y.Count - 1];


            //Knowing the final state, throw this back to the Physics engine through a new force
            float springDamperTorque = (float)(-jData.torsional_K.z * jData.torsional_finalState[0] - jData.torsional_C.z * jData.torsional_finalState[1]);


            if (float.IsNaN(springDamperTorque))
            {
                springDamperTorque = 0;
            }
            Vector3 torqueZ = new Vector3(0f, 0f, springDamperTorque);

            jData.rb.AddTorque(torqueZ);
            jData.anchorObject.GetComponent<Rigidbody>().AddTorque(-torqueZ);
        }
    }

    // Linear ODEs
    private double[] linearODEx(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -jData.simulated_C.x / jData.mass * x[1] - jData.linear_K.x / jData.mass * x[0];
        return y;
    }
    private double[] linearODEy(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -jData.simulated_C.y / jData.mass * x[1] - jData.linear_K.y / jData.mass * x[0];
        return y;
    }
    private double[] linearODEz(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -jData.simulated_C.z / jData.mass * x[1] - jData.linear_K.z / jData.mass * x[0];
        return y;
    }

    // Torsional ODEs
    private double[] torsionalODEx(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -jData.torsional_C.x / jData.J.x * x[1] - jData.torsional_K.x / jData.J.x * x[0];
        return y;
    }
    private double[] torsionalODEy(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -jData.torsional_C.y / jData.J.y * x[1] - jData.torsional_K.y / jData.J.y * x[0];
        return y;
    }
    private double[] torsionalODEz(double t, double[] x)
    {
        double[] y = (double[])x.Clone();
        y[0] = x[1];
        y[1] = -jData.torsional_C.z / jData.J.z * x[1] - jData.torsional_K.z / jData.J.z * x[0];
        return y;
    }

    public void UpdateAnchorPosition()
    {
        if (jData.anchorObject != null)
        {
            jData.anchorPosition = jData.anchorObject.transform.position;
        }
        else
        {
            jData.anchorPosition = Vector3.zero;
        }
    }
}
