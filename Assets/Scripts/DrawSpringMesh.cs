using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawSpringMesh : MonoBehaviour
{
    [SerializeField] private GameObject anchorObject;
    [SerializeField] private GameObject baseObject;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private int numberOfHelixes;
    [SerializeField] private float springRadius;
    [SerializeField] private GameObject vertexMarker;

    private Vector3 anchorPosition;
    private Vector3 initialPosition;
    private Vector2[] spinePoints;
    private Vector2[] ribbPoints;
    private GameObject[] markers;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        initialPosition = baseObject.transform.position;
        anchorPosition = anchorObject.transform.position;
        spinePoints = CalculateSpinePoints();
        //ArrayDebugger("spine points", spinePoints);
        ribbPoints = CalculateRibbPoints(spinePoints);
        //ArrayDebugger("ribb points", ribbPoints);
        markers = InstantiateMarkers(markers,ribbPoints);
    }

    private Vector2[] CalculateSpinePoints()
    {
        Vector2[] spineList = new Vector2[numberOfHelixes];
        Vector2 delta = (initialPosition - anchorPosition)/(numberOfHelixes-1);
        spineList[0] = anchorPosition;
        for(int i=1;i<numberOfHelixes;i++)
        {
            spineList[i] = spineList[i - 1] + delta;
        }
        return spineList;
    }

    private Vector2[] CalculateRibbPoints(Vector2[] spineList)
    {
        int n = spineList.Length;
        Vector2[] ribbList = new Vector2[n-1];
        Vector2 ribb = Vector2.zero;
        float midX = 0f;
        float midY = 0f;
        for (int i = 0; i < n-1; i ++)
        {
            midX = (spineList[i + 1].x + spineList[i].x) / 2;
            //Debug.Log("Midx = " + midX);
            midY = (spineList[i + 1].y + spineList[i].y) / 2;
            //Debug.Log("Midy = " + midY);
            float slope = CalculateSlope(spineList[i].x, spineList[i + 1].x, spineList[i].y, spineList[i + 1].y);
            //Debug.Log("Slope = " + slope);
            float perpSlope = CalculateNormalSlope(slope);
            //Debug.Log("perpSlope = " + perpSlope);
            if(perpSlope!=0)
            {
                if (i % 2 == 0)
                {
                    ribb.x = midX + (springRadius / Mathf.Sqrt(1 + Mathf.Pow(perpSlope, 2)));
                    ribb.y = midY + (springRadius * perpSlope / Mathf.Sqrt(1 + Mathf.Pow(perpSlope, 2)));
                }
                else
                {
                    ribb.x = midX - (springRadius / Mathf.Sqrt(1 + Mathf.Pow(perpSlope, 2)));
                    ribb.y = midY - (springRadius * perpSlope / Mathf.Sqrt(1 + Mathf.Pow(perpSlope, 2)));
                }
            }else
            {
                if (i % 2 == 0)
                {
                    ribb.x = spineList[i].x + springRadius;
                    ribb.y = midY;
                }
                else
                {
                    ribb.x = spineList[i].x - springRadius;
                    ribb.y = midY;
                }
            }
            Debug.Log("Ribb = " + ribb.x + ", " + ribb.y);
            ribbList[i] = ribb;
        }
        Vector2[] newArrayOfRibbs = new Vector2[n + 1];
        newArrayOfRibbs[0] = anchorPosition;
        Array.Copy(ribbList, 0, newArrayOfRibbs, 1, ribbList.Length); // Copy original array starting from index 0 to newArray starting from index 1
        newArrayOfRibbs[newArrayOfRibbs.Length-1] = initialPosition;
        return newArrayOfRibbs;
    }

    private float CalculateSlope(float x1, float x2, float y1, float y2)
    {
        if(x2==x1)
        {
            return 0;
        }else
        {
            return (y2 - y1) / (x2 - x1);
        }
    }
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
    private void ArrayDebugger(string message, Vector2[] array)
    {
        int n = array.Length;
        for(int i=0; i<n; i++)
        {
            Debug.Log(message + "[" + i + "]=" + array[i]);
        }
    }

    private GameObject[] InstantiateMarkers(GameObject[] toBeDestroyed, Vector2[] positions)
    {
        if (toBeDestroyed != null)
        {
            DestroyInstantiated(toBeDestroyed);
        }
        int n = positions.Length;
        GameObject[] instantiated = new GameObject[n]; 
        for (int i = 0; i < n; i++)
        {
            Vector3 position = new Vector3(positions[i].x, positions[i].y, 0f);
            instantiated[i] = Instantiate(vertexMarker, position, Quaternion.identity);
        }
        return instantiated;
    }
    private void DestroyInstantiated(GameObject[] instantiated)
    {
        foreach(GameObject obj in instantiated)
        {
            Destroy(obj);
        }
    }
}
