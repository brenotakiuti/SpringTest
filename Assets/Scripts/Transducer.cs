using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Transducer : MonoBehaviour
{
    public float calibration;

    // Start is called before the first frame update
    public abstract void Start();

    // Update is called once per frame
    public abstract void Update();

}
