using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObstacleAvoidanceController : MonoBehaviour
{
    [Header("Base fields")]
    [SerializeField] protected DriverAI Driver;
    protected int ObstacleColliderMask;

    protected virtual void Awake()
    {
        ObstacleColliderMask = LayerMask.GetMask("Car", "Obstacle");
        //ObstacleColliderMask = LayerMask.GetMask("Obstacle");
    }
}
