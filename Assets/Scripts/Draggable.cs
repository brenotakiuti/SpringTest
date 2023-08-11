using System.Collections.Generic;
using UnityEngine;

// From: Jayanam Games
// https://www.patreon.com/posts/unity-3d-drag-22917454
public class Draggable : MonoBehaviour
{
    [SerializeField] protected bool isButtonDown = false;
    [SerializeField] protected float rotationScale = 1;
    [SerializeField] protected bool isRotatable = false;
    [SerializeField] protected bool isMovable = false;
    [SerializeField] protected bool isRotatableX = false;
    [SerializeField] protected bool isRotatableY = false;
    [SerializeField] protected bool isRotatableZ = false;
    [SerializeField] protected Color movableColor = Color.yellow;
    [SerializeField] protected Color rotatableColor = Color.green;
    //private Renderer rnder;
    [SerializeField] protected List<Color> startColors = new List<Color>();
    [SerializeField] protected bool isSnap = false;
    [SerializeField] protected string snapTag = "Snap";
    protected Vector3 mOffset;
    protected float mZCoord;
    protected Vector3 rotation;
    public Vector3 snapCoordinates;
    protected Rigidbody rb;
    protected GridController gridController;
    protected bool isMouseOver = false;
    protected GameObject previewObject;
    protected Marker triggeredMarker;

    public virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        movableColor.a = 0.5f;
        foreach (Renderer rnder in GetComponentsInChildren<Renderer>())
        {
            startColors.Add(rnder.material.color);
        }
        gridController = FindObjectOfType<GridController>();

        previewObject = Instantiate(gameObject,Vector3.zero,Quaternion.identity);
        // Get all components attached to the GameObject
        Component[] components = previewObject.GetComponents<Component>();

        // Loop through the components and destroy those that are not the componentToKeep
        foreach (Component component in components)
        {
            if (component.GetType() != typeof(MeshRenderer) &&
                component.GetType() != typeof(MeshFilter) &&
                component.GetType() != typeof(Transform))
            {
                Destroy(component);
            }
        }
        Color materialColor = previewObject.GetComponent<MeshRenderer>().material.color;
        materialColor.a = 0.5f;
        previewObject.GetComponent<MeshRenderer>().material.color = materialColor;
        previewObject.SetActive(false);
    }

    void Update()
    {
        if (isRotatableX || isRotatableY || isRotatableZ)
        {
            //print("yes");
            isRotatable = true;
        }
        else
        {
            isRotatable = false;
        }
        //print(rotation);
        if (Input.GetMouseButton(0) && isMouseOver)
        {
            if (isMovable)
            {
                transform.root.position = GetMouseAsWorldPoint() + mOffset;
            }
            else if (isRotatable)
            {
                Rotate();
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            isMouseOver = false;
            if(isSnap){
                transform.root.position = snapCoordinates;
                triggeredMarker.ToggleIsAttached(true);
            }            
            previewObject.SetActive(false);
            previewObject.transform.position = Vector3.zero;
        }
    }
    public virtual void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        if(triggeredMarker!=null)
        {
            triggeredMarker.ToggleIsAttached(false);
        }
        
        // Store offset = gameobject world pos - mouse world pos
        mOffset = gameObject.transform.root.position - GetMouseAsWorldPoint();
        rotation = transform.localRotation.eulerAngles;
        //print(rotation);
        isButtonDown = true;
        //print(rotation);
        ResetObjectColor();
    }

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Detection: " + collider.name);
        if (collider.tag == snapTag)
        {
            triggeredMarker = collider.GetComponent<Marker>();
            snapCoordinates = triggeredMarker.GetCoordinate();
            
            previewObject.transform.position = snapCoordinates;
            previewObject.SetActive(true);
            isSnap = true;
        }
    }
    void OnTriggerExit(Collider collider)
    {
        previewObject.SetActive(false);
        isSnap = false;
        //triggeredMarker = null;
    }

    public virtual void OnMouseUp()
    {
        isButtonDown = false;
    }


    protected Vector3 GetMouseAsWorldPoint()
    {
        // Pixel coordinates of mouse (x,y)
        Vector3 mousePoint = Input.mousePosition;

        // z coordinate of game object on screen
        mousePoint.z = mZCoord;

        // Convert it to world points
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    void OnMouseEnter()
    {
        //isSnap = false;
        if (!isButtonDown)
        {
            if (isMovable)
            {
                isMouseOver = true;
                foreach (Renderer rnder in GetComponentsInChildren<Renderer>())
                {
                    rnder.material.color = movableColor;
                }
                //rnder.material.color = Color.yellow;
            }
            else if (isRotatable)
            {
                foreach (Renderer rnder in GetComponentsInChildren<Renderer>())
                {
                    rnder.material.color = rotatableColor;
                }
            }
        }
        if (gridController != null)
        {
            gridController.ToggleDraggableClick(true);
        }
    }

    void OnMouseExit()
    {
        ResetObjectColor();
        if(!isButtonDown)
        {
            isMouseOver = false;
        }
        
        if (gridController != null)
        {
            gridController.ToggleDraggableClick(false);
        }
    }

    protected virtual void Rotate()
    {
        Vector3 rt = rotation;
        if (isRotatableX)
        {
            rt.x = rotation.x - GetMouseAsWorldPoint().x * rotationScale;
        }
        else if (isRotatableY)
        {
            rt.y = rotation.y - (GetMouseAsWorldPoint()).x * rotationScale;
        }
        else if (isRotatableZ)
        {
            rt.z = rotation.z - (GetMouseAsWorldPoint()).x * rotationScale;
        }
        transform.localRotation = Quaternion.Euler(rt);
    }

    public void SetRotationZ(float deg)
    {
        transform.localEulerAngles = new Vector3(0f, 0f, deg);
    }

    public float RotateTowardsCursor()
    {
        //Get the Screen positions of the object
        Vector2 positionOnScreen = Camera.main.WorldToViewportPoint(transform.position);

        //Get the Screen position of the mouse
        Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);

        //Get the angle between the points
        float angle = AngleBetweenTwoPoints(positionOnScreen, mouseOnScreen);

        //Ta Daaa
        return angle;
    }

    private float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }

    protected void ResetObjectColor()
    {
        int i = 0;
        foreach (Renderer rnder in GetComponentsInChildren<Renderer>())
        {
            rnder.material.color = startColors[i];
            i++;
        }
    }
}