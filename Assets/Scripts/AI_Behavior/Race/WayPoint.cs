using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    [SerializeField] private float _radius;

    public float Radius => _radius;


    public void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
