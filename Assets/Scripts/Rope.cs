using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Rope : MonoBehaviour
{
    //[SerializeField] private float length = 10f;
    [SerializeField] private int merCount;
    //[SerializeField] private float merDistance = 1f;
    [SerializeField] private GameObject merPrefab;
    //[SerializeField] private Component configurableJointComponent;
    [SerializeField] private Material material;
    [SerializeField] private float thickness = 1;

    [SerializeField] private readonly int[] indexesA = new int[]{0,1,2,2,1,3};
    [SerializeField] private readonly int[] indexesB = new int[]{0,2,1,2,3,1};
    //  Clockwise:
    //  1     3     5    
    //  o-----o-----o---
    //  | \   | \   | \
    //  |   \ |   \ |
    //  o-----o-----o---
    //  0     2     4    
    // [0,1,2,2,1,3,2,3,4,4,3,5,...]
    //
    //  Anti-clockwise:
    //  1     3     5    
    //  o-----o-----o---
    //  | \   | \   | \
    //  |   \ |   \ |
    //  o-----o-----o---
    //  0     2     4    
    // [0,2,1,2,3,1,2,4,3,4,5,3,...]
    //https://catlikecoding.com/unity/tutorials/procedural-meshes/creating-a-mesh/

    protected GameObject startMer = null;
    protected GameObject endMer = null;
    protected Transform startMerTransform;
    protected Transform endMerTransform;
    protected GameObject previousMer;
    private Mesh mesh;
    private List<GameObject> merList;
    //private List<Vector3> coordinatesList;
    //private List<Vector3> coordinatesListCombined = new List<Vector3>();
    private List<Vector3> normalsListA;
    private List<Vector3> normalsListB;
    private List<Vector3> normalsListCombined = new List<Vector3>();
    private List<int> indexesListA = new List<int>();
    private List<int> indexesListB = new List<int>();
    private List<int> indexesCombined = new List<int>();
    private float radius;
    private float zDelta;
    private float merDistance;
    private float hthickness;

    // Start is called before the first frame update
    public virtual void Start()
    {
        
    }

    void OnEnable()
    {
        hthickness = thickness/2;
        //print(hthickness);
        SetStartEndMers();

        // Check if x coordinates of startmer and endmer are equal. If so, separate a little
        CheckIfXZero();

        // Calculate the radius of the parable (1/2 distance between startmer and endmer)
        radius = XDelta(startMerTransform.localPosition, endMerTransform.localPosition, 2);
        //print(radius);

        // Calculate the distance between two mers (divide the distance by the number of mers desired)
        merDistance = XDelta(startMerTransform.localPosition, endMerTransform.localPosition, merCount + 1);
        //print(merDistance);

        // Calculate the distante in the Z coordinate between startmer and endmer. This is used to draw a line in the xz plane. Basically
        // I use the parable as the xy plane coordinates and the a line for the xz plane coordinates.
        zDelta = Zline(startMerTransform.localPosition, endMerTransform.localPosition, merCount + 1);
        //print(zDelta);

        // InstantiateRope() uses the variables set previously
        merList = InstantiateRope();

        /////////////////////////////////////////////////////////////////////////////////////////////////
        //Mesh Rendering

        // Calculate the distance between the object coordinate and the vertex position
        hthickness = thickness/2;
        
        mesh = new Mesh();
        mesh.name = "Procedural Mesh";
        //print(Vector3.up*hthickness);
        int count = merList.Count;

        // Build the "front" indexes list
        indexesListA = TapeIndexesBuild(count,indexesA);
        // Build the "back" indexes list
        indexesListB = TapeIndexesBuild(count,indexesB);

        // Combine both indexes list
        indexesCombined.AddRange(indexesListA);
        indexesCombined.AddRange(indexesListB);

        // Build both normals lists
        normalsListA = NormalBuild(count*2, Vector3.back);
        normalsListB = NormalBuild(count*2, Vector3.forward);

        // Combine normals lists
        normalsListCombined.AddRange(normalsListA);
        normalsListCombined.AddRange(normalsListB);

        // Build the mesh
        UpdateMesh(mesh);

        // Set the built mesh as the mesh to the mesh filter
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;
    }

    //Need a way to strech when dragged and shrink when ungragged

    // Update is called once per frame
    public virtual void Update()
    {
        // Update mesh 
        UpdateMesh(mesh);
    }

    /// <summary>
    /// x coordinates for startmer and endmer must not be equal 
    /// </summary>
    private void CheckIfXZero()
    {
        if(startMer.transform.localPosition.x == endMer.transform.localPosition.x)
        {
            endMer.transform.localPosition = endMer.transform.localPosition + Vector3.right * 0.1f;
        }
    }

    /// <summary>
    /// Finds and sets the objects to be considered as start and end positions of the rope/wire
    /// </summary>
    public virtual void SetStartEndMers()
    {
        startMer = transform.GetChild(0).gameObject;
        if (transform.GetChild(1) != null)
        {
            endMer = transform.GetChild(1).gameObject;
        }

        startMerTransform = startMer.transform;
        endMerTransform = endMer.transform;
    }

    /// <summary>
    /// Instantiate ONE mer
    /// </summary>
    /// <param name="number">the number of the instantiated mer. This is used only to give propper names to this game object.</param>
    public GameObject InstantiateNewMer(int number)
    {
        GameObject instaMer = Instantiate(merPrefab);
        instaMer.transform.SetParent(transform);
        instaMer.transform.localPosition = CalculatePosition();
        //print("instamer: " + instaMer.transform.localPosition);
        instaMer.gameObject.name = "Monomer(" + number + ")";
        JointSetup(instaMer.GetComponent<ConfigurableJoint>(), previousMer, false);
        return instaMer;
    }

    /// <summary>
    /// Configures the Joints (basically the connected body and anchor pos)
    /// </summary>
    /// <param name="instaMer">the ConfigurableJoint component in gameObject.</param>
    /// <param name="previousMer">the GameObject of the previous mer.</param>
    /// <param name="isKinematic">inform if this mer should be kinematic or not (not being used).</param>
    public void JointSetup(ConfigurableJoint instaMer, GameObject previousMer, bool isKinematic)
    {
        //instaMer.GetComponent<Rigidbody>().isKinematic = true;
        /////////////////////////////////////////////////////////////////////////

        instaMer.connectedBody = previousMer.GetComponent<Rigidbody>();
        Vector3 anchorPos = previousMer.transform.localPosition - instaMer.transform.localPosition;
        anchorPos.z = 0f;
        instaMer.anchor = anchorPos;

        ////////////////////////////////////////////////////////////////////////
        //instaMer.GetComponent<Rigidbody>().isKinematic = isKinematic;
    }

    /// <summary>
    /// Command to instantiate the objects of the rope (does not build the mesh)
    /// </summary>
    public List<GameObject> InstantiateRope()
    {
        previousMer = startMer;
        List<GameObject> list = new List<GameObject>();
        list.Add(startMer);
        for (int i = 1; i <= merCount; i++)
        {
            GameObject instaMer = InstantiateNewMer(i);
            list.Add(instaMer);
            //print("previousmer: " + instaMer.transform.localPosition);
            previousMer = instaMer;
        }
        list.Add(endMer);
        // After adding the last mer, set a second configurable joint to this mer, because one will connect to the previous mer and the other will connect to the endmer
        previousMer.AddComponent<ConfigurableJoint>();
        JointSetup(previousMer.GetComponents<ConfigurableJoint>()[1], endMer, true);
        return list;
    }

    /// <summary>
    /// Draw the shape (function) of the points that each mer must connect. In this case is a parable function
    /// </summary>
    /// <param name="x">x coordinate in the parable function.</param>
    /// <param name="radius">the radius of the parable.</param>
    protected virtual Vector3 RopeShape(float x, float radius)
    {
        float y = Mathf.Pow((x - radius), 2) / (2 * radius);
        Vector3 vec = new Vector3(x, y - radius / 2);
        return vec;
    }

    /// <summary>
    /// Calculate the distance between two vectors and divide by an int
    /// </summary>
    /// <param name="vec1">first point.</param>
    /// <param name="vec2">second point.</param>
    /// <param name="count">the number of points between vec1 and vec2.</param>
    private float XDelta(Vector3 vec1, Vector3 vec2, int count)
    {
        return (vec2.x - vec1.x) / count;
    }

    /// <summary>
    /// Draw the path of the rope/wire in the xz plane (straight line)
    /// </summary>
    /// <param name="vec1">first point.</param>
    /// <param name="vec2">second point.</param>
    /// <param name="count">the number of points between vec1 and vec2.</param>
    private float Zline(Vector3 vec1, Vector3 vec2, int count)
    {
        Vector2 vec1xz = new Vector2(vec1.x, vec1.z);
        Vector2 vec2xz = new Vector2(vec2.x, vec2.z);
        Vector2 vec = (vec2xz - vec1xz) / count;
        return vec.y;
    }

    /// <summary>
    /// Calculate the coordinates for the next MER using the Parable function
    /// </summary>
    private Vector3 CalculatePosition()
    {
        Vector3 startPos = previousMer.transform.localPosition;
        Vector3 newPos = RopeShape(startPos.x + merDistance, radius) + new Vector3(0f, 0f, startPos.z + zDelta);
        //print(newPos);
        return newPos;
    }

    /// <summary>
    /// Convert the coordinates of the game objects into vertices. In this method, each object gets 2 vertices. Check the definition for more info.
    /// </summary>
    /// <param name="posList">the list of all mers.</param>
    private List<Vector3> OrthogonalVerticesBuild(List<GameObject> posList)
    {
        // In this method, the vertices are placed in the orthogonal direction of the line drawn between two consecutive
        // objects. Se below:
        //
        //            o1
        //            o
        //             \      . v1
        //              \.
        //          .    \
        //      .         o
        //      v2        o2
        //
        // Where the distance between v1 to the line is the same value of the radius of the "rope"
        // v2 goes to the coordinates relative to the same direction of v1, but with negative modulus.

        List<Vector3> newList = new List<Vector3>();
        Vector3 displacement;
        for(int i=0;i<merList.Count;i++)
        {
            Vector3 direction;
            Vector3 orthoDir;
            if (i==0)
            {
                // The first mer should use the second mer as reference to draw the line and find the orthogonal vector.
                //displacement = new Vector3(0f,hthickness,0f);
                direction = posList[i].transform.localPosition - posList[i+1].transform.localPosition;
                orthoDir = (Quaternion.AngleAxis(-90, Vector3.forward) * direction).normalized;
                displacement = orthoDir * hthickness;
            }
            else{
                direction = posList[i-1].transform.localPosition - posList[i].transform.localPosition;
                orthoDir = (Quaternion.AngleAxis(-90, Vector3.forward) * direction).normalized;
                displacement = orthoDir*hthickness;
            }
            //displacement = new Vector3(0f,hthickness,0f);
            
            // Add both the up and down vectors 
            newList.Add(posList[i].transform.localPosition-displacement);
            newList.Add(posList[i].transform.localPosition+displacement);
        }

        return newList;
    }

    /// <summary>
    /// Build the indexes for a simple "plane" rope/wire. Using this form, the rope looks like a "tape". Check the definition for more info.
    /// </summary>
    /// <param name="count">the number of mers in your rope.</param>
    /// <param name="indexes">use either indexesA or indexesB.</param>
    private List<int> TapeIndexesBuild(int count, int[] indexes)
    {
        // This builds the indexes for a rectangular mesh between two mers. You need two meshes, one in the front and another in the back.
        List<int> prevList = new List<int>();
        for(int i=1;i<count;i++)
        {
            //print((i-1)*2);
            prevList.AddRange(SumArrayInt(indexes,(i-1)*2));
            // [0,1,2,2,1,3,2,3,4,4,3,5,...]
            // [0,2,1,2,3,1,2,4,3,4,5,3,...]
            // Notice the 2 diference in the 6 number sequence? Hence every cycle, you should add multiples of 2, starting from 0.
        }
        return prevList;
    }

    /// <summary>
    /// This function ADDS a constant value to every elements of an array. This is basically array+single number.
    /// </summary>
    /// <param name="arr">the array for the opperation.</param>
    /// <param name="value">the value to be summed.</param>
    public int[] SumArrayInt(int[] arr, int value)
    {
        int[] narr = (int[]) arr.Clone();
        for(int i=0;i<arr.Length;i++)
        {
            narr[i] = arr[i] + value;
        }
        return narr;
    }

    /// <summary>
    /// Builds a normals list of Vector3s
    /// </summary>
    /// <param name="count">the number os elements.</param>
    /// <param name="direction">either Vector3.forward or Vector3.back.</param>
    public List<Vector3> NormalBuild(int count, Vector3 direction)
    {
        List<Vector3> prevList = new List<Vector3>();
        for(int i=1;i<=count;i++)
        {
            prevList.Add(direction);
        }
        return prevList;
    }

    /// <summary>
    /// Generate the mesh and update
    /// </summary>
    /// <param name="mesh">the mesh type to be generated (choose one in editor).</param>
    private void UpdateMesh(Mesh mesh)
    {
        mesh.vertices = OrthogonalVerticesBuild(merList).ToArray();
        mesh.triangles = indexesCombined.ToArray();
        mesh.RecalculateBounds(); //You need to do this every time the mesh is changed, so we do this at the end of the mesh updade.
    }
}   
