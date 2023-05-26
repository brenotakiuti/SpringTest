using UnityEngine;

[ExecuteInEditMode]
public class GridGenerator : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private Vector3 wallSize;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private float gridSpacing = 1;
    [SerializeField] private float lineWidth = 0.02f;
    [SerializeField] private float multiplier = 3;
    [SerializeField] private int thickerLineAt = 10;

    private void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateGrid();
        }
    }

    // private void OnValidate()
    // {
    //     if (Application.isPlaying)
    //     {
    //         UpdateGrid();
    //     }
    // }

    public Vector3 GetWallSize()
    {
        return transform.parent.transform.localScale;
    }

    private void UpdateGrid()
    {
        // Calculate the gridSize by taking the wall size and dividing by the spacing
        wallSize = GetWallSize();
        gridSize.x = Mathf.FloorToInt(wallSize.x/gridSpacing);
        gridSize.y = Mathf.FloorToInt(wallSize.y/gridSpacing);

        // Remove any previously generated grid lines
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Generate new grid lines (old)
        // for (int x = 0; x <= gridSize.x; x++)
        // {
        //     DrawLine(new Vector3(x * gridSpacing, 0f, 0f), new Vector3(x * gridSpacing, gridSize.y * gridSpacing, 0f));
        // }

        // for (int y = 0; y <= gridSize.y; y++)
        // {
        //     DrawLine(new Vector3(0f, y * gridSpacing, 0f), new Vector3(gridSize.x * gridSpacing, y * gridSpacing, 0f));
        // }

        // Generate new grid lines (new)
        for (int x = 0; x <= gridSize.x/2; x++)
        {
            Vector3 lineStart = new Vector3(x * gridSpacing, -gridSize.y/2 * gridSpacing, 0f);
            Vector3 lineEnd = new Vector3(x * gridSpacing, gridSize.y/2 * gridSpacing, 0f);
            if(x%thickerLineAt == 0)
            {
                DrawLine(lineStart, lineEnd,lineWidth*multiplier);
            }else{
                DrawLine(lineStart, lineEnd,lineWidth);
            }
        }
        for (int x = 0; x >= -gridSize.x/2; x--)
        {
            Vector3 lineStart = new Vector3(x * gridSpacing, -gridSize.y/2 * gridSpacing, 0f);
            Vector3 lineEnd = new Vector3(x * gridSpacing, gridSize.y/2 * gridSpacing, 0f);
            if(x%thickerLineAt == 0)
            {
                DrawLine(lineStart, lineEnd,lineWidth*multiplier);
            }else{
                DrawLine(lineStart, lineEnd,lineWidth);
            }
        }

        for (int y = 0; y <= gridSize.y/2; y++)
        {
            Vector3 lineStart = new Vector3(-gridSize.x/2 * gridSpacing, y * gridSpacing, 0f);
            Vector3 lineEnd = new Vector3(gridSize.x/2 * gridSpacing, y * gridSpacing, 0f);
            if(y%thickerLineAt == 0)
            {
                DrawLine(lineStart, lineEnd,lineWidth*multiplier);
            }else{
                DrawLine(lineStart, lineEnd,lineWidth);
            }
        }

        for (int y = 0; y >= -gridSize.y/2; y--)
        {
            Vector3 lineStart = new Vector3(-gridSize.x/2 * gridSpacing, y * gridSpacing, 0f);
            Vector3 lineEnd = new Vector3(gridSize.x/2 * gridSpacing, y * gridSpacing, 0f);
            if(y%thickerLineAt == 0)
            {
                DrawLine(lineStart, lineEnd,lineWidth*multiplier);
            }else{
                DrawLine(lineStart, lineEnd,lineWidth);
            }
        }
    }

    private void DrawLine(Vector3 start, Vector3 end, float lineWidth)
    {
        GameObject line = new GameObject("Line");
        line.transform.SetParent(transform);
        line.transform.localPosition = Vector3.zero;

        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = false;
        lineRenderer.SetPositions(new Vector3[] { start, end });

        // Set line thickness
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        // Create a new material and assign it to the LineRenderer
        lineRenderer.material = material;

    }
}
