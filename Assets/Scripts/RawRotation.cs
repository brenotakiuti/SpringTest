using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RawRotation : MonoBehaviour
{
    public Vector3 rawRotation;
    private Quaternion rawQuaternion;
    private Vector3 turns;
    private Vector3 deltaEuler;
    private Quaternion previousRotation;
    [SerializeField] private int turnTolerance = 0; // Adjust the tolerance value as needed

    void Start()
    {
        previousRotation = transform.rotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rawRotation += CalculateRotationDifference(transform.rotation, previousRotation, turnTolerance);
        rawQuaternion = transform.rotation * Quaternion.Inverse(previousRotation);

        CalculateTurns();

        previousRotation = transform.rotation;
    }

    public Quaternion GetRawQuaternion()
    {
        return rawQuaternion;
    } 

    public static Vector3 CalculateRotationDifference(Quaternion rotation1, Quaternion rotation2, int turnTolerance)
    {
        Quaternion deltaQuaternion = rotation1 * Quaternion.Inverse(rotation2);
        Vector3 deltaEuler = deltaQuaternion.eulerAngles;

        // Tolerance Check. In a FixedDeltaTime, deltaEuler angles cannot be bigger than or close to 360
        if (Mathf.Abs(deltaEuler.x) >= 180f - turnTolerance)
             deltaEuler.x -= 360f;

        if (Mathf.Abs(deltaEuler.y) >= 180f - turnTolerance)
             deltaEuler.y -= 360f;

        if (Mathf.Abs(deltaEuler.z) >= 180f - turnTolerance)
             deltaEuler.z -= 360f;

        return deltaEuler;
    }

    private void CalculateTurns()
    {
        turns.x = Mathf.Floor(rawRotation.x / 360);
        turns.y = Mathf.Floor(rawRotation.y / 360);
        turns.z = Mathf.Floor(rawRotation.z / 360);
    }

    public void ResetRawRotation()
    {
        rawRotation = Vector3.zero;
        turns = Vector3.zero;
    }
    public Vector3 GetDeltaEuler()
    {
        return deltaEuler;
    }
    public Vector3 GetRawRotation()
    {
        return rawRotation;
    }
    public Vector3 GetTurns()
    {
        return turns;
    }
}
