using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//I'll change this to go by deltatime and redo whatever uses this later. also you would not believe how many projects of mine use Rotator.cs
public class Rotator : MonoBehaviour
{
    public Vector3 rotationAxes;

    void Update()
    {
        transform.Rotate(rotationAxes.x, rotationAxes.y, rotationAxes.z);
    }
}
