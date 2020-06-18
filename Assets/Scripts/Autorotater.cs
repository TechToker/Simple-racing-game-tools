using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Autorotater : MonoBehaviour
{
    public float Speed;
    public Vector3 MovingVector;
    
    // Update is called once per frame
    void Update()
    {
        var t = Quaternion.AngleAxis(Speed, MovingVector);
        transform.Rotate(MovingVector, Speed);
    }
}
