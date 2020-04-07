using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplinePathCreator : MonoBehaviour {

    [HideInInspector]
    public SplinePath path;

    public void CreatePath()
    {
        path = new SplinePath(transform.position);
    }
}
