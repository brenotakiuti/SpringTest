using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for Sensors
// Sensors and actuators have some properties and methods that may be similar, but I chose to separate them because even though the name might be the same, those can
// refer to something completely different.
public abstract class Sensor : Transducer
{
    protected string output;
    // Get some output
    public virtual string GetOutput()
    {
        return output;
    }
}
