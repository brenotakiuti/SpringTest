using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls how the ultrassoninc sensor works
// Inherits from "sensor"
// Outputs: distance between the sensor object and an obstacle
// Calibration: corrects the distance value when the sensor "collides" with an obstacle (zero offset)
public class UltrassonicScript : Sensor
{
    [SerializeField] private LayerMask sensorIgnore;
    [SerializeField] private float offset = 0f;
    [SerializeField] private string direction = "right";
    private Ray ray;
    private RaycastHit hitData;    
    //public float radius = 1f;
    private Vector3 offsetRay;

    // Start is called before the first frame update
    public override void Start()
    {
        output = "0";      
    }

    // Update is called once per frame
    public override void Update()
    {
        // Cast a Ray from the pivot of the game object
        //offsetRay = transform.right * offset + new Vector3(0f, -radius,0f);
        Vector3 directionVector3 = StringToDirection(direction);
        offsetRay = directionVector3 * offset;
        ray = new Ray(transform.position+offsetRay, directionVector3);

        Debug.DrawRay(transform.position+offsetRay, directionVector3 * 10);
        

        if(Physics.Raycast(ray, out hitData, 100f, ~sensorIgnore))
        //if(Physics.SphereCast(transform.position+offsetRay, radius, transform.right, out hitData, 100f, ~sensorIgnore))
        {
            //Debug.Log(hitData.transform.name);
            // Set output to be the distance to the hit object
            output = (hitData.distance-calibration).ToString();
        }else{
            output = "9999999999";
        }
    }

    public Vector3 StringToDirection(string directionString)
    {
        Vector3 directionVector3;
        if(directionString=="right"|| directionString == "Right")
        {
            directionVector3 = Vector3.right;
        }
        else if (directionString == "left" || directionString == "Left")
        {
            directionVector3 = Vector3.left;
        }
        else if (directionString == "up" || directionString == "Up")
        {
            directionVector3 = Vector3.up;
        }
        else if (directionString == "down" || directionString == "Down")
        {
            directionVector3 = Vector3.down;
        }
        else if (directionString == "back" || directionString == "Back")
        {
            directionVector3 = Vector3.back;
        }else
        {
            directionVector3 = Vector3.forward;
        }
        return directionVector3;
    }
}
