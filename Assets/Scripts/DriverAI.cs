using BehaviourAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CurrentDriverMode
{
    None = 0,
    Chase = 1,
    BrakeAroundTarget = 2,
}

public class DriverAI : BaseDriver
{
    [Header("Blackboard: Main")]
    public BaseCar TargetCar;
    public float TargetPositionUpdateDelay = 0.3f;

    public float CarSensorsDistance = 3f;

    public AnimationCurve TargetSpeedByTarget;

    [Header("Autobraking")]
    public float _brakingPedalForce = 0.1f;
    public float _brakingPedalForceDelta = 0.05f;

    [Header("TurningBraking")]
    public AnimationCurve TargetSpeedByTurnAngle;


    public CurrentDriverMode CurrentMode;
    private BaseState _state;

    public float TurningAlanystDistance = 30f;
    public float CarRadius = 1.5f;

    public string CurrentStateName => _state.GetStateName();

    public Transform TargetTransform => TargetCar.transform;

    private void OnEnable()
    {
        //_state = new ChaseState(this, Car);
        _state = new FollowState(this, Car);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        _state.OnDrawGizmos();
    }

    protected void FixedUpdate()
    {
        _state.FixedUpdate();
    }

    protected void Update()
    {
        _state.OnUpdate();

        if (Input.GetKeyDown(KeyCode.B))
            _state = new BrakingByTargetState(TargetTransform.position, this, Car);
    }
}
