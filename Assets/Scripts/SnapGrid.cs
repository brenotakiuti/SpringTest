using System;
using UnityEngine;

public class SnapGrid : MonoBehaviour
{
    [SerializeField] private int gridSizeX = 10;
    [SerializeField] private int gridSizeY = 10;
    [SerializeField] private int gridSizeZ = 10;
    [SerializeField] private GameObject marker;
    [SerializeField] private bool isRenderMesh = false;
    public float cellSizeX, cellSizeY, cellSizeZ;
    public Vector3[] snapPositions;
    private GameObject[] markers;

    private void Start()
    {
        CalculateCellSizes();
        BuildSnapGrid();
    }

    private void CalculateCellSizes()
    {
        //Vector3 scale = transform.lossyScale; //not sure yet if I should use local or global scale. Global for now
        Vector3 scale = transform.localScale;
        if (gridSizeX > 1)
            cellSizeX = 1f / (gridSizeX - 1);
        else
            cellSizeX = cellSizeY;
        if (gridSizeY > 1)
            cellSizeY = 1f / (gridSizeY - 1);
        else
            cellSizeY = cellSizeX;
        if (gridSizeZ > 1)
            cellSizeZ = 1f / (gridSizeZ - 1);
        else
            cellSizeZ = cellSizeX;
    }

    private void BuildSnapGrid()
    {
        int totalGridSize = gridSizeX * gridSizeY * gridSizeZ;
        snapPositions = new Vector3[totalGridSize];
        markers = new GameObject[totalGridSize];

        Vector3 centerOffset = new Vector3(
            (gridSizeX - 1) * cellSizeX * 0.5f,
            (gridSizeY - 1) * cellSizeY * 0.5f,
            (gridSizeZ - 1) * cellSizeZ * 0.5f
        );

        int index = 0;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 position = new Vector3(x * cellSizeX - centerOffset.x, y * cellSizeY - centerOffset.y, z * cellSizeZ - centerOffset.z);
                    snapPositions[index] = position;
                    markers[index]=Instantiate(marker, position, Quaternion.identity);
                    markers[index].name = markers[index].name + index + "x" + x + "y" + y +"z" + z;
                    //markers[index].transform.localScale = new Vector3(cellSizeX,cellSizeY,cellSizeZ);
                    markers[index].transform.parent=gameObject.transform;
                    markers[index].transform.localPosition = position;
                    index++;
                }
            }
        }
    }

    public void ToggleMarkersMesh(bool toggle){
        foreach(GameObject obj in markers)
        {
            obj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled= toggle;
        }
    }

    public bool GetIsRenderMesh()
    {
        return isRenderMesh;
    }

    public GameObject[] GetMakers()
    {
        return markers;
    }
}
