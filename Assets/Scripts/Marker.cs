using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour
{
    private Vector3 coordinate;
    public bool isAttached = false;
    [SerializeField] private bool isRenderMesh = true; 
    // Start is called before the first frame update
    void Start()
    {
        coordinate = transform.position;
    }

    public Vector3 GetCoordinate()
    {
        return coordinate;
    }
    public bool GetIsRenderMesh()
    {
        return isRenderMesh;
    }

    public void ToggleMarkersMesh(bool toggle)
    {
        transform.GetChild(0).GetComponent<MeshRenderer>().enabled = toggle;
    }

    public void ToggleIsAttached(bool toggle)
    {
        isAttached = toggle;
    }
    public bool GetIsAttached()
    {
        return isAttached;
    }
}
