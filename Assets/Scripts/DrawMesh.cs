using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DrawMesh 
{
    [SerializeField] private readonly int[] indexesA = new int[] { 0, 1, 2, 2, 1, 3 };
    [SerializeField] private readonly int[] indexesB = new int[] { 0, 2, 1, 2, 3, 1 };
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

    private Mesh mesh;

    private List<Vector3> normalsListA;
    private List<Vector3> normalsListB;
    private List<Vector3> normalsListCombined = new List<Vector3>();
    private List<int> indexesListA = new List<int>();
    private List<int> indexesListB = new List<int>();
    private List<int> indexesCombined = new List<int>();
    private GameObject meshObject;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    public Mesh GenerateMesh(Vector3[] vertices, int n)
    {

        /////////////////////////////////////////////////////////////////////////////////////////////////
        //Mesh Rendering

        mesh = new Mesh();
        mesh.name = "Procedural Mesh";

        // Build the "front" indexes list
        indexesListA = TapeIndexesBuild(n, indexesA);
        // Build the "back" indexes list
        indexesListB = TapeIndexesBuild(n, indexesB);

        // Combine both indexes list
        indexesCombined.AddRange(indexesListA);
        indexesCombined.AddRange(indexesListB);

        // Build both normals lists
        normalsListA = NormalBuild(n * 2, Vector3.back);
        normalsListB = NormalBuild(n * 2, Vector3.forward);

        // Combine normals lists
        normalsListCombined.AddRange(normalsListA);
        normalsListCombined.AddRange(normalsListB);

        // Build the mesh
        mesh.vertices = vertices;
        mesh.triangles = indexesCombined.ToArray();
        mesh.RecalculateBounds(); //You need to do this every time the mesh is changed, so we do this at the end of the mesh updade.
        return mesh;
    }

    public void BuildMeshComponent(GameObject parent, Material material)
    {
        // Create a GameObject to visualize the mesh
        meshObject = new GameObject("MeshObject");
        meshObject.transform.parent = parent.transform;
        meshObject.transform.localPosition = Vector3.zero;
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
    }
    public void UpdateMesh(Mesh mesh)
    {
        meshFilter.mesh = mesh;
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
        for (int i = 1; i < count; i++)
        {
            //print((i-1)*2);
            prevList.AddRange(SumArrayInt(indexes, (i - 1) * 2));
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
        int[] narr = (int[])arr.Clone();
        for (int i = 0; i < arr.Length; i++)
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
        for (int i = 1; i <= count; i++)
        {
            prevList.Add(direction);
        }
        return prevList;
    }
}
