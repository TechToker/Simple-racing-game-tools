﻿using System;
using BehaviourAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum DriverMode
{
    None = 0,

    Chase = 1,
    Follow = 2,
    PitManeuver = 3,
    SideRam = 4,
    RoadBlock = 5,

    WaypointRace = 10,
    BrakeAroundTarget = 20,
}

public class DriverAI : BaseDriver
{
    public DriverMode CurrentMode;
    
    [Header("Blackboard: Race mode")]
    [SerializeField] private RaceCircuit _circuit;
    public float TurningAlanystDistance = 30f;

    [SerializeField] public AnimationCurve SpeedByCornerAnlge;
    [SerializeField] public AnimationCurve BrakingDistanceByDeltaSpeed;
    [SerializeField] public AnimationCurve TurningTargetDistanceBySpeed;
    
    public bool OvertakeMode;

    [Header("PID controller")]
    public float ProportionalCoef = 0.5f;
    public float IntegralCoef = 0.5f;
    public float DerivativeCoef = 0.5f;
    
    [Header("Rubberbanding")]
    [Range(0.5f, 1.5f)]
    public float RubberBandingValue = 1;

    [ReadOnly] public float RbTorqueMultiplyer = 1;
    [ReadOnly] public float RbAccelerationSpeedMultiplyer = 200;
    [ReadOnly] public float RbBrakingDistanceMultiplyer = 1;

    [SerializeField] private float _minTorqueMultiplier = 1f;
    [SerializeField] private float _maxTorqueMultiplier = 2f;
    
    [SerializeField] private float _minAccelPedalSpeedMultiplier = 50f;
    [SerializeField] private float _maxAccelPedalSpeedMultiplier = 0.5f;
    
    [SerializeField] private float _minBrakingDistanceMultiplier = 1f;
    [SerializeField] private float _maxBrakingDistanceMultiplier = 3f;
    
    public AnimationCurve RBTorqueMultiplyerBySpeed;

    [Header("Obstacle avoidance")] 
    public float ObstacleAvoidanceWeight;
    
    [Header("Blackboard: Follow mode")]
    public BaseCar TargetCar;
    public float TargetPositionUpdateDelay = 0.3f;

    public float CarSensorsDistance = 3f;

    public AnimationCurve TargetSpeedByTarget;

    [Header("Autobraking")]
    public float _brakingPedalForce = 0.1f;
    public float _brakingPedalForceDelta = 0.05f;

    [Header("TurningBraking")]
    public AnimationCurve TargetSpeedByTurnAngle;

    [Header("FollowState")]
    public float WheelAngleSpeed = 300;

    public AnimationCurve DistanceFactor;
    public AnimationCurve DeltaSpeedFactor;
    public AnimationCurve DeltaSpeedMultyplyerByDistance;
    
    private DriverMode _currentMode;
    private BaseState _state;

    public string CurrentStateName => _state.GetStateName();
    public BaseState StateAI => _state;

    private void OnValidate()
    {
        float positiveRbPercent = Mathf.Clamp((RubberBandingValue - 1) / (1.5f - 1), 0, float.MaxValue);
        float negativeRbPercent = (Mathf.Clamp(RubberBandingValue, 0.5f, 1f) - 0.5f) / 0.5f;
        
        RbTorqueMultiplyer = _minTorqueMultiplier + positiveRbPercent * (_maxTorqueMultiplier - _minTorqueMultiplier);
        RbAccelerationSpeedMultiplyer = _minAccelPedalSpeedMultiplier + negativeRbPercent * (_maxAccelPedalSpeedMultiplier - _minAccelPedalSpeedMultiplier);
        RbBrakingDistanceMultiplyer = _minBrakingDistanceMultiplier + negativeRbPercent * (_maxBrakingDistanceMultiplier - _minBrakingDistanceMultiplier);
    }

    private void OnEnable()
    {
        _state = new BaseState(this, Car);
        SetDriverMode(CurrentMode);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || _state == null)
            return;

        _state.OnDrawGizmos();
    }

    protected void FixedUpdate()
    {
        if(_state == null)
            return;
        
        _state.FixedUpdate();
    }

    protected void Update()
    {
        SetDriverMode(CurrentMode);
        
        if (_state == null)
            return;

        _state.OnUpdate();
    }

    public void SetDriverMode(DriverMode mode)
    {
        if (mode == _currentMode)
            return;

        switch (mode)
        {
            case (DriverMode.None):
                _state = new BaseState(this, Car);
                break;
            case (DriverMode.Chase):
                _state = new ChaseState(this, Car);
                break;
            case (DriverMode.Follow):
                _state = new FollowRelativeTargetState(this, Car);
                break;
            case (DriverMode.PitManeuver):
                _state = new PitManeuverState(this, Car);
                break;
            case (DriverMode.RoadBlock):
                _state = new RoadBlockState(this, Car);
                break;
            case (DriverMode.WaypointRace):
                _state = new RacingState(_circuit, this, Car);
                break;
        }

        _currentMode = mode;
        Debug.LogError($"Set state: {mode}");
    }
}