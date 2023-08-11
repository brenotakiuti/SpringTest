using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public Marker[] markers;
    public bool[] markerRenderArray;
    public int numberOfMarkers;
    public bool isDraggableClick = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SearchForMarkersAfterDelay());
    }

    private IEnumerator SearchForMarkersAfterDelay()
    {
        yield return new WaitForSeconds(0.1f); // Wait for a short time (adjust as needed)

        // Now search for the object using FindObjectOfType()
        markers = FindObjectsOfType<Marker>();

        if (markers != null)
        {
            // Found the component
            Debug.Log("Found MyComponent using FindObjectOfType.");
            numberOfMarkers = markers.Length;
            markerRenderArray = new bool[numberOfMarkers];
            for (int index = 0; index < numberOfMarkers; index++)
            {
                markerRenderArray[index] = markers[index].GetIsRenderMesh();
            }
        }
        else
        {
            // Component not found
            Debug.Log("MyComponent not found using FindObjectOfType.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && isDraggableClick)
        {
            for (int index = 0; index < numberOfMarkers; index++)
            {
                if (markerRenderArray[index])
                {
                    markers[index].ToggleMarkersMesh(true);
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            for (int index = 0; index < numberOfMarkers; index++)
            {
                if (markerRenderArray[index])
                {
                    markers[index].ToggleMarkersMesh(false);
                }
            }
        }
    }
    public void ToggleDraggableClick(bool toggle)
    {
        isDraggableClick = toggle;
    }
}
