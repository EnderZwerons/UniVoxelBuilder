using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Vector3 rotationAxes;

    void Update()
    {
        transform.Rotate(rotationAxes.x, rotationAxes.y, rotationAxes.z);
    }
}
