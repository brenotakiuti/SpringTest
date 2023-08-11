using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    private Vector3 initialRelativeVector;
    public GameObject obj2;
    public float angle;
    public float torqueFactor = 10;
    public float maxTorqueMagnitude = 200;
    public float angleThreshold = 0.1f;
    void Start()
    {
        // Calculate the initial relative vector between obj1 and obj2 positions
        initialRelativeVector = transform.position - obj2.transform.position;
    }

    void FixedUpdate()
    {
        // Calculate the current relative vector between obj1 and obj2 positions
        Vector3 currentRelativeVector = transform.position - obj2.transform.position;

        // Calculate the angle between initial and current relative vectors
        angle = Vector3.Angle(initialRelativeVector, currentRelativeVector);
        if (angle > angleThreshold)
        {
            // Calculate the torque magnitude based on the angle (you might need to adjust this)
            float torqueMagnitude = angle * torqueFactor;

            // Limit the maximum torque magnitude to avoid excessive rotation
            torqueMagnitude = Mathf.Clamp(torqueMagnitude, 0, maxTorqueMagnitude);

            // Calculate torque direction (use cross product between initial and current vectors)
            Vector3 torqueDirection = Vector3.Cross(initialRelativeVector, currentRelativeVector);

            // Apply torque to obj2's rigidbody around the calculated direction
            obj2.GetComponent<Rigidbody>().AddTorque(torqueDirection * torqueMagnitude);
        }
        initialRelativeVector = transform.position - obj2.transform.position;
    }

}
