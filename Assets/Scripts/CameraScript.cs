using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScript:MonoBehaviour {
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 2F;
    public float sensitivityY = 2F;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -90F;
    public float maximumY = 90F;
    public float rotationY = 0F;

    public float moveSpeed = 5f;
    public float clickdelay = 0.8f;
    private int clicked = 0;
    private float clicktime = 0f;
    private GameObject lastObj = null;
    public float cameraOffset = 3f;
    public float camMoveSpeed = 3f;
    private Vector3 startCamPosition;
    private Transform objTransform;
    private Quaternion startCamRotation;
    private Quaternion objRotation;
    private Vector3 objPosition;
    private bool isLerpingCamera = false;
    private float lerpTime = 0f;

    void Update()
    {
        MouseInput();
        KeyboardInput();

        if(isLerpingCamera)
        {
            lerpTime += camMoveSpeed*Time.deltaTime;
            //Debug.Log(camMoveSpeed*Time.deltaTime);
            FocusCamera();
        }
        if(lerpTime>=1)
        {
            ResetLastClick();
        }
    }

    void KeyboardInput()
    {
        Vector3 pos = transform.position;
        if(Input.GetKey(KeyCode.UpArrow))
        {
            pos += transform.up*moveSpeed * Time.deltaTime;
        }else if(Input.GetKey(KeyCode.DownArrow))
        {
            pos -= transform.up*moveSpeed * Time.deltaTime;
        }else if(Input.GetKey(KeyCode.RightArrow))
        {
            pos += transform.right*moveSpeed * Time.deltaTime;
        }else if(Input.GetKey(KeyCode.LeftArrow))
        {
            pos -= transform.right*moveSpeed * Time.deltaTime;
        }
        transform.position = pos;
    }
    void MouseInput()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            GameObject obj = null;
            if (Input.GetMouseButtonDown(0))
            {
                clicked++;
                obj = CastRay();
                if (clicked == 1)
                {
                    clicktime = Time.time;
                    lastObj = obj;
                } 
            }
            else if (Input.GetMouseButton(1))
            {
                MouseRightClick();
                ResetLastClick();
            }
            else if (Input.GetMouseButton(2))
            {
                MouseMiddleButtonClicked();
                ResetLastClick();
            }
            else if (Input.GetMouseButtonUp(1))
            {
                ShowAndUnlockCursor();
                ResetLastClick();
            }
            else if (Input.GetMouseButtonUp(2))
            {
                ShowAndUnlockCursor();
                ResetLastClick();
            }
            else
            {
                MouseWheeling();
            }
            // if (clicked > 1 && Time.time - clicktime < clickdelay)
            // {
            //     clicked = 0;
            //     clicktime = 0;
            //     GameObject newObj = CastRay();
            //     if(newObj!=null && lastObj==newObj)
            //     {
            //         startCamPosition = transform.position;
            //         objTransform = newObj.transform.root;
            //         startCamRotation = transform.rotation;
            //         objRotation = newObj.transform.root.rotation;
            //         //Debug.Log(newObj.transform.position);
            //         isLerpingCamera = true;
            //     }
            // }else if (clicked > 2 || Time.time - clicktime > 1)
            // {
            //     clicked = 0;
            // }
        }
    }

    void FocusCamera()
    {
        Vector3 newVec = objTransform.position;
        newVec -= objTransform.forward*cameraOffset;
        transform.position = Vector3.Lerp(startCamPosition, newVec, lerpTime);
        transform.rotation = Quaternion.Lerp(startCamRotation, objRotation, lerpTime);
    }

    void ResetLastClick()
    {
        clicked = 0;
        clicktime = 0;
        lastObj = null;
        isLerpingCamera = false;
        lerpTime = 0f;
    }

    void ShowAndUnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void HideAndLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void MouseMiddleButtonClicked()
    {
        HideAndLockCursor();
        Vector3 NewPosition = new Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y"));
        Vector3 pos = transform.position;
        if (NewPosition.x > 0.0f)
        {
            pos += transform.right*moveSpeed/3*Time.deltaTime;
        }
        else if (NewPosition.x < 0.0f)
        {
            pos -= transform.right*moveSpeed/3*Time.deltaTime;
        }
        if (NewPosition.z > 0.0f)
        {
            pos += transform.forward*moveSpeed/3*Time.deltaTime;
        }
        if (NewPosition.z < 0.0f)
        {
            pos -= transform.forward*moveSpeed/3*Time.deltaTime;
        }
        pos.y = transform.position.y;
        transform.position = pos;
    }

    void MouseRightClick()
    {
        HideAndLockCursor();
        if (axes == RotationAxes.MouseXAndY)
        {
            float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
            //float rotationY = transform.localEulerAngles.x + Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
        }
        else if (axes == RotationAxes.MouseX)
        {
            transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
        }
        else
        {
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

            transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
        }
    }

    void MouseWheeling()
    {
        Vector3 pos = transform.position;
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            pos = pos - transform.forward;
            transform.position = pos;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            pos = pos + transform.forward;
            transform.position = pos;
        }
    }
    
    GameObject CastRay() 
    {
         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
         RaycastHit hit;
         Physics.Raycast(ray, out hit);

         if (hit.collider !=null) 
         {
             //Debug.Log(hit.collider.gameObject.name);
             return hit.collider.gameObject;
         }else
         {
             return null;
         }
     }
 }