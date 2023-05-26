using System.Collections.Generic;
using UnityEngine;

// From: Jayanam Games
// https://www.patreon.com/posts/unity-3d-drag-22917454
public class DraggableForce : MonoBehaviour
{
    protected Vector3 mOffset;
    protected float mZCoord;
    protected Vector3 rotation;
    protected Rigidbody rb;
    [SerializeField] protected float forceMultiplier = 2f;
    [SerializeField] protected bool isButtonDown = false;
    [SerializeField] protected float rotationScale = 1;
    [SerializeField] protected bool isRotatable = false;
    [SerializeField] protected bool isMovable = false;
    [SerializeField] protected bool isRotatableX = false;
    [SerializeField] protected bool isRotatableY = false;
    [SerializeField] protected bool isRotatableZ = false;
    [SerializeField] protected bool isMovableX = false;
    [SerializeField] protected bool isMovableY = false;
    [SerializeField] protected bool isMovableZ = false;
    [SerializeField] protected Color movableColor = Color.yellow;
    [SerializeField] protected Color rotatableColor = Color.green;
    //private Renderer rnder;
    protected List<Color> startColors = new List<Color>();

    public virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        movableColor.a = 0.5f;
        foreach (Renderer rnder in GetComponentsInChildren<Renderer>())
        {
            startColors.Add(rnder.material.color);
        }
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
        if (isMovableX || isMovableY || isMovableZ)
        {
            //print("yes");
            isMovable = true;
        }
        else
        {
            isMovable = false;
        }
        //print(rotation);
    }
    public virtual void OnMouseDown()
    {
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

        // Store offset = gameobject world pos - mouse world pos
        mOffset = gameObject.transform.root.position - GetMouseAsWorldPoint();
        rotation = transform.localRotation.eulerAngles;
        //print(rotation);
        isButtonDown = true;
        //print(rotation);
        ResetObjectColor();
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
        if (!isButtonDown)
        {
            if (isMovable)
            {
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
    }

    void OnMouseExit()
    {
        ResetObjectColor();
    }

    public virtual void OnMouseDrag()
    {

        if (isMovable)
        {
            Move();
        }
        else if (isRotatable)
        {
            Rotate();
        }
    }
    protected virtual void Move()
    {
        Vector3 mv = new Vector3(0f,0f,0f);
        Vector3 force;
        mv = transform.position;
        if (isMovableX)
        {
            mv.x = GetMouseAsWorldPoint().x + mOffset.x;
        }
        else if (isMovableY)
        {
            //rt.y = rotation.y-(GetMouseAsWorldPoint() + rotation).x*rotationScale;
            mv.y = (GetMouseAsWorldPoint()).y + mOffset.y;
        }
        else if (isMovableZ)
        {
            //rt.z = rotation.z-(GetMouseAsWorldPoint() + rotation).x*rotationScale;
            mv.z = GetMouseAsWorldPoint().z + mOffset.z;
        }
        force = (mv - transform.position) * forceMultiplier;
        rb.AddForce(force, ForceMode.Force);
        //transform.root.position = mv;
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
            //rt.y = rotation.y-(GetMouseAsWorldPoint() + rotation).x*rotationScale;
            rt.y = rotation.y - (GetMouseAsWorldPoint()).x * rotationScale;
        }
        else if (isRotatableZ)
        {
            //rt.z = rotation.z-(GetMouseAsWorldPoint() + rotation).x*rotationScale;
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