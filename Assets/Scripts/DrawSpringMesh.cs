using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DrawSpringMesh : MonoBehaviour
{
    [SerializeField] private bool turnOn = true;
    [SerializeField] private Vector3 anchorOffset;
    [SerializeField] private Vector3 baseOffset;
    [SerializeField] private Material material;
    [Tooltip("Number of periods (helixes). You will always get +1 because you have half in connected do anchor and half to the base (this body)")]
    [SerializeField] private int numberOfHelixes = 9;
    [SerializeField] private float springRadius = 0.5f;
    [SerializeField] private float springWidth = 0.1f;
    [SerializeField] private GameObject vertexMarker;
    [SerializeField] private double tolerance = 1e-6;


    private GameObject anchorObject;
    private GameObject baseObject;
    private GameObject newParentObject;
    private Vector3 anchorPosition;
    private Vector3 initialPosition;
    private Vector3[] spinePoints;
    private Vector3[] ribbPoints;
    private Vector3[] vertexPoints;
    private GameObject[] markers;
    private Vector3 previousDelta;
    private DrawMesh drawMesh = new DrawMesh();
    private Mesh mesh;

    public GameObject AnchorObject { get => anchorObject; set => anchorObject = value; }
    public GameObject BaseObject { get => baseObject; set => baseObject = value; }

    private void Start()
    {
        // Create a game object to be parent of both the mass and the spring
        newParentObject = new GameObject("SpringMass");
        // Make this object the child of the new parent object
        gameObject.transform.parent = newParentObject.transform;
        // Create a new object for the spring and add all components needed for the mesh
        drawMesh.BuildMeshComponent(newParentObject, material);
        anchorObject = gameObject.GetComponent<Spring3D>().GetAnchorObject();
        baseObject = gameObject;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (turnOn)
        {
            initialPosition = baseObject.transform.position + baseOffset;
            anchorPosition = anchorObject.transform.position + anchorOffset;
            Vector3 difference = (initialPosition - anchorPosition);
            Vector2 delta = difference / (numberOfHelixes - 1);
            Vector3 deltaVersor = Vector3.Normalize(difference);
            Vector3 rotation = CalculateRotationAngles(deltaVersor, Vector3.up);
            //Debug.Log("P1 = " + deltaVersor);
            //Debug.Log("P2 = " + Vector3.up);
            //Debug.Log("Rotation vector = " + rotation);
            if (Mathf.Abs(delta.magnitude - previousDelta.magnitude) > tolerance)
            {
                UpdateSpringMesh(delta);
                previousDelta = delta;

            }
        }

    }


    private void UpdateSpringMesh(Vector2 delta)
    {
        // Calculate the spine points of the spring. The spine is the center line starting from the anchored object and ending at the object itself.
        spinePoints = CalculateSpinePoints(delta);
        // Calculate the ribb points. The ribb points are points positioned at a distance "springRadius" from the spine line.
        ribbPoints = CalculateRibbPoints(spinePoints);
        // Calculate the vertices. If we want to draw a mesh, we need at  least 4 points (to draw a rectangle) so we transform the spinePoints into two pairs of points with a distance of springWidth from each other.
        vertexPoints = CalculateVerticesFromRibbs(ribbPoints);

        // Draw markers at ribbpoints. This is used mostly for debugging purposes.
        //markers = InstantiateMarkers(markers, ribbPoints);

        // Generate the mesh with the calculated vertex points
        mesh = drawMesh.GenerateMesh(vertexPoints, numberOfHelixes + 1);

        // Update the mesh
        drawMesh.UpdateMesh(mesh);
    }

    private Vector3 RotateCoordinates(Vector3 from, Vector3 rotation)
    {
        float xGlobal = (float)((from.x * Math.Cos(rotation.y) * Math.Cos(rotation.z)) - (from.y * Math.Sin(rotation.z) * Math.Cos(rotation.y)) + (from.z * Math.Sin(rotation.y)));
        float yGlobal = (float)((from.x * (Math.Sin(rotation.x) * Math.Sin(rotation.y) * Math.Cos(rotation.z) + Math.Cos(rotation.x) * Math.Sin(rotation.z))) + (from.y * (Math.Sin(rotation.x) * Math.Sin(rotation.y) * Math.Sin(rotation.z) - Math.Cos(rotation.x) * Math.Cos(rotation.z))) - (from.z * Math.Sin(rotation.x) * Math.Cos(rotation.y)));
        float zGlobal = (float)((from.x * (Math.Cos(rotation.x) * Math.Sin(rotation.y) * Math.Cos(rotation.z) - Math.Sin(rotation.x) * Math.Sin(rotation.z))) + (from.y * (Math.Cos(rotation.x) * Math.Sin(rotation.y) * Math.Sin(rotation.z) + Math.Sin(rotation.x) * Math.Cos(rotation.z))) + (from.z * Math.Cos(rotation.x) * Math.Cos(rotation.y)));
        return new Vector3(xGlobal, yGlobal, zGlobal);
    }

    private Vector3 CalculateRotationAngles(Vector3 from, Vector3 to)
    {
        Vector3 diff = new Vector3(to.x - from.x, to.y - from.y, to.z - from.z);
        float rotationX = (float)Math.Atan2(diff.y, diff.z);
        float rotationY = (float)Math.Atan2(diff.x, Math.Sqrt(diff.y * diff.y + diff.z * diff.z));
        float rotationZ = (float)Math.Atan2(diff.y, diff.x);
        Vector3 rotation = new Vector3(rotationX, rotationY, rotationZ);

        return rotation;
    }

    /// <summary>
    /// Calculate the spine points of the spring. The spine line is a straight line from the anchor position to the object position. This line is divided in smaller sections with a distance of "delta" from each other.
    /// </summary>
    /// <param name="delta">the distance from one point of the spine to the next.</param>
    private Vector3[] CalculateSpinePoints(Vector3 delta)
    {
        Vector3[] spineList = new Vector3[numberOfHelixes];
        //Vector2 delta = (initialPosition - anchorPosition)/(numberOfHelixes-1);
        spineList[0] = anchorPosition;
        for (int i = 1; i < numberOfHelixes; i++)
        {
            spineList[i] = spineList[i - 1] + delta;
        }
        return spineList;
    }

    /// <summary>
    /// Calculate the ribb points of the spring. The ribbs are points positioned in a distance of "springRadius" from the spine line. These points when connected form a ZIGZAG line.
    /// </summary>
    /// <param name="spineArray">the array of spine points.</param>
    private Vector3[] CalculateRibbPoints(Vector3[] spineArray)
    {
        int n = spineArray.Length;
        Vector3[] ribbList = new Vector3[n - 1];
        Vector3 ribb = Vector3.zero;
        float midX = 0f;
        float midY = 0f;
        for (int i = 0; i < n - 1; i++)
        {
            // Calculate the midpoint between the first spine point to the next spine point
            midX = (spineArray[i + 1].x + spineArray[i].x) / 2;
            midY = (spineArray[i + 1].y + spineArray[i].y) / 2;
            // Calculate the slope of the line between the two spine points
            float slope = CalculateSlope(spineArray[i].x, spineArray[i + 1].x, spineArray[i].y, spineArray[i + 1].y);
            // Calculate the slope of a line that is perpendicular to the previous line
            float perpSlope = CalculateNormalSlope(slope);

            // If the slope of any of the two lines above are different than zero
            if (perpSlope != 0)
            {
                // Position the ribb to the relative right of the spine line
                if (i % 2 == 0)
                {
                    ribb.x = midX + (springRadius / Mathf.Sqrt(1 + Mathf.Pow(perpSlope, 2)));
                    ribb.y = midY + (springRadius * perpSlope / Mathf.Sqrt(1 + Mathf.Pow(perpSlope, 2)));
                }
                // Position the ribb to the relative left of the spine line
                else
                {
                    ribb.x = midX - (springRadius / Mathf.Sqrt(1 + Mathf.Pow(perpSlope, 2)));
                    ribb.y = midY - (springRadius * perpSlope / Mathf.Sqrt(1 + Mathf.Pow(perpSlope, 2)));
                }
                // Else if there are any slopes that are zero, the spine is a completely vertical line, meaning that the ribbs can be placed simply to the right and left of the spine line
            }
            else
            {
                // Position the ribb to the global right of the spine line
                if (i % 2 == 0)
                {
                    ribb.x = spineArray[i].x + springRadius;
                    ribb.y = midY;
                }
                // Position the ribb to the global left of the spine line
                else
                {
                    ribb.x = spineArray[i].x - springRadius;
                    ribb.y = midY;
                }
            }
            ribbList[i] = ribb;
        }
        // The ribbList is calculated disrregarding the position of the anchor and the position of the object. So we just add those manually.
        Vector3[] newArrayOfRibbs = new Vector3[n + 1];
        newArrayOfRibbs[0] = anchorPosition; // Add the anchor position as zero
        Array.Copy(ribbList, 0, newArrayOfRibbs, 1, ribbList.Length); // Copy original array starting from index 0 to newArray starting from index 1
        newArrayOfRibbs[newArrayOfRibbs.Length - 1] = initialPosition; // Add the initial position as the last index
        return newArrayOfRibbs;
    }

    /// <summary>
    /// Calculate the vertices of the spring. Using the ribbpoints as reference, we just need to place one point above and one under each ribb point, to form the 4 vertices needed for the mesh.
    /// </summary>
    /// <param name="ribbs">the array of ribb points.</param>
    private Vector3[] CalculateVerticesFromRibbs(Vector3[] ribbs)
    {
        int n = ribbs.Length;
        Vector3[] vertices = new Vector3[2 * n];
        int counter = 0;
        for (int i = 0; i < n * 2; i += 2)
        {
            vertices[i] = ribbs[counter];
            vertices[i].y = vertices[i].y - springWidth / 2;
            vertices[i + 1] = ribbs[counter];
            vertices[i + 1].y = vertices[i + 1].y + springWidth / 2;
            counter++;
        }
        return vertices;
    }

    /// <summary>
    /// Calculate the slope between two points. Basically the definition of tangent.
    /// </summary>
    /// <param name="x1">initial x coordinate.</param>
    /// <param name="x2">final x coordinate.</param>
    /// <param name="y1">initial y coordinate.</param>
    /// <param name="y2">final y coordinate.</param>
    private float CalculateSlope(float x1, float x2, float y1, float y2)
    {
        if (x2 == x1)
        {
            return 0;
        }
        else
        {
            return (y2 - y1) / (x2 - x1);
        }
    }

    /// <summary>
    /// Calculate the normal (orthogonal) slope from a given slope. 
    /// </summary>
    /// <param name="slope">slope to be converted.</param>
    private float CalculateNormalSlope(float slope)
    {
        if (slope == 0)
        {
            return 0;
        }
        else
        {
            return -1 / slope;
        }
    }
    /// <summary>
    /// Take any array and log it at the console. 
    /// </summary>
    /// <param name="message">Just write any message before starting to log.</param>
    /// <param name="array">The array to log.</param>
    private void ArrayDebugger(string message, Vector3[] array)
    {
        int n = array.Length;
        for (int i = 0; i < n; i++)
        {
            Debug.Log(message + "[" + i + "]=" + array[i]);
        }
    }

    /// <summary>
    /// Just place markers at any position contained at a "positions" array of Vector3s. 
    /// </summary>
    /// <param name="toBeDestroyed">GameObjects generated previously that you want to destroy to avoid having too much objects in the hierarchy.</param>
    /// <param name="positions">The position that the markers will appear.</param>
    private GameObject[] InstantiateMarkers(GameObject[] toBeDestroyed, Vector3[] positions)
    {
        if (toBeDestroyed != null)
        {
            DestroyInstantiated(toBeDestroyed);
        }
        int n = positions.Length;
        GameObject[] instantiated = new GameObject[n];
        for (int i = 0; i < n; i++)
        {
            //Vector3 position = new Vector3(positions[i].x, positions[i].y, 0f);
            instantiated[i] = Instantiate(vertexMarker, positions[i], Quaternion.identity);
        }
        return instantiated;
    }

    /// <summary>
    /// Destroy any object inside an array of objects. 
    /// </summary>
    /// <param name="instantiated">Array containing game objects to be destroyed.</param>
    private void DestroyInstantiated(GameObject[] instantiated)
    {
        foreach (GameObject obj in instantiated)
        {
            Destroy(obj);
        }
    }
}
