using System;
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
    public bool ShowTurnDataGizmos;
    
    public RaceCircuit Circuit;
    
    public bool OvertakeMode;
    public float TurningAlanystDistance = 30f;
    public float WheelAngleSpeed = 300;
    
    [SerializeField] public AnimationCurve SpeedByCornerAnlge;
    [SerializeField] public AnimationCurve BrakingDistanceByDeltaSpeed;
    [SerializeField] public AnimationCurve TurningTargetDistanceBySpeed;

    [Header("Braking system")]
    public BrakeAlgorithm BrakeAlgorithm = BrakeAlgorithm.PID; 
    public float ProportionalCoef = 0.5f;
    public float IntegralCoef = 0.5f;
    public float DerivativeCoef = 0.5f;

    [Header("Rubberbanding")] 
    public RubberbandingController RubberbandingController;

    [Header("Obstacle avoidance")]
    public float ObstacleAvoidanceWeight;
    public HeatLineController ObstacleAvoidController;
    
    [Header("Blackboard: Chase mode")]
    public BaseCar TargetCar;
    public float TargetPositionUpdateDelay = 0.3f;

    public float CarSensorsDistance = 3f;

    [Header("Autobraking")]
    public float _brakingPedalForce = 0.1f;
    public float _brakingPedalForceDelta = 0.05f;

    [Header("TurningBraking")]
    public AnimationCurve TargetSpeedByTurnAngle;

    [Header("FollowState")]
    public AnimationCurve DistanceFactor;
    public AnimationCurve DeltaSpeedFactor;
    public AnimationCurve DeltaSpeedMultyplyerByDistance;
    
    
    private DriverMode _currentMode;
    private BaseState _state;

    public string CurrentStateName => _state.GetStateName();
    public BaseState StateAI => _state;

    private void Awake()
    {
        _state = new BaseState(this, Car);
        SetDriverMode(CurrentMode);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || _state == null)
            return;

        _state.OnEveryGizmosDraw();
    }

    protected void FixedUpdate()
    {
        if(_state == null)
            return;
        
        _state.OnEveryFixedUpdate();
    }

    protected void Update()
    {
        SetDriverMode(CurrentMode);
        
        if (_state == null)
            return;

        _state.OnEveryUpdate();
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
                _state = new RacingState(Circuit, this, Car);
                break;
        }

        _currentMode = mode;
        Debug.LogError($"Set state: {mode}");
    }
}
