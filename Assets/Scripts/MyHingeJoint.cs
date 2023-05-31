using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyHingeJoint : MonoBehaviour
{
    
    public Transform centerOfRotation;
    public float maxAngle = 90f;

    private Rigidbody rb;
    private Quaternion initialRotation;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialRotation = rb.rotation;
    }

    private void FixedUpdate()
    {
        if (centerOfRotation != null)
        {
            // Calculate the desired rotation based on the center of rotation
            Quaternion targetRotation = Quaternion.LookRotation(transform.position - centerOfRotation.position, centerOfRotation.up);

            // Limit the rotation within the specified range
            float currentAngle = Quaternion.Angle(initialRotation, targetRotation);
            if (currentAngle > maxAngle)
            {
                targetRotation = Quaternion.RotateTowards(initialRotation, targetRotation, maxAngle);
            }

            // Apply the rotation to the Rigidbody
            rb.MoveRotation(targetRotation);
        }
    }
}
