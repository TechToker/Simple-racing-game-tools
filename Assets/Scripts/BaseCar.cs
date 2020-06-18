using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseCar : MonoBehaviour
{
    public bool IsDebug;

    [Header("Main")]
    [SerializeField] protected Rigidbody _mainRigidBody;
    [SerializeField] protected Collider _mainCollider;
    
    [Header("Wheels")]
    [SerializeField] protected WheelCollider[] _forwardWheels;
    [SerializeField] protected WheelCollider[] _backwardWheels;

    [SerializeField] protected WheelCollider[] _motorWheels;

    [SerializeField] private GameObject _wheelVisualPrefab;

    [Header("Specifications")]
    [SerializeField] protected AnimationCurve _turningAngleBySpeed;
    [SerializeField] protected AnimationCurve _torqueBySpeed;
    [SerializeField] protected AnimationCurve _reverceTorqueBySpeed;

    [SerializeField] protected float _brakeTorque = 700f;
    [SerializeField] protected float _handBrakeTorque = 500f;
    [SerializeField] protected float _motorTorqueResistance = 5;

    [Header("Extra params")]
    [SerializeField] protected float _defaultRearWheelStiffness = 0.9f;
    [SerializeField] protected float _handbrakeRearWheelStiffness = 0.75f;

    public readonly Vector2 CarSize = new Vector2(1.92f, 4.3f);
    public Vector3 CarFrontBumperPos => _mainRigidBody.transform.position + transform.forward * CarSize.y / 2;
    public Transform RigidbodyTransform => _mainRigidBody.transform;
    
    [Header("Vehicle substeps")]
    // На малых скоростях колесо может пробуксовывать и терять сцепление с дорогой.
    // Это может быть из-за недостатка вычислений физического движка, 
    // Чтобы этого не происходило можно увеличить количество "подшагов" для просчёта физики колёс
    // Т.е увеличить частоту вычислений для колёс на один fixed update
    // На высоких скоростях сцепления достаточно и необходимость в большем количестве вычислений избыточна

    [Tooltip("The vehicle's speed when the physics engine can use different amount of sub-steps (in m/s).")]
    [SerializeField] private float _criticalSpeed = 6f;
    [Tooltip("Simulation sub-steps when the speed is above critical.")]
    [SerializeField] private int _stepsBelow = 8;
    [Tooltip("Simulation sub-steps when the speed is below critical.")]
    [SerializeField] private int _stepsAbove = 2;

    [Header("Effects")]
    [SerializeField] private GameObject _lights;

    public float CarSpeed => _mainRigidBody.velocity.magnitude;
    public bool IsCarMovingForward => _mainRigidBody.transform.InverseTransformDirection(_mainRigidBody.velocity).z >= 0;
    public Vector3 RelativeForce => _mainRigidBody.velocity;

    public Collider MainCollider => _mainCollider;

    public float CurrentWheelAngle => _forwardWheels[0].steerAngle;

    public float MaxWheelAngle => _turningAngleBySpeed.Evaluate(CarSpeed);

    public float TurningAngle { get; private set; }
    public float AccelerationInput { get; private set; }
    public float BrakingInput { get; private set; }
    public bool EBrakeInput { get; private set; }
    
    private GameObject[] _visualWheels;
    protected WheelCollider[] _allWheels;

    private float _currentBrakeTorque;

    void Start()
    {
        _allWheels = new WheelCollider[_forwardWheels.Length + _backwardWheels.Length];
        Array.Copy(_forwardWheels, _allWheels, _forwardWheels.Length);
        Array.Copy(_backwardWheels, 0, _allWheels, _forwardWheels.Length, _backwardWheels.Length);

        //It actually sets parameters to the vehicle but not to a wheel.
        _allWheels[0].ConfigureVehicleSubsteps(_criticalSpeed, _stepsBelow, _stepsAbove);

        if (_wheelVisualPrefab == null)
            return;

        _visualWheels = new GameObject[_allWheels.Length];

        for (int i = 0; i < _allWheels.Length; i++)
        {
            GameObject ws = Instantiate(_wheelVisualPrefab);
            ws.transform.parent = _allWheels[i].transform;
            _visualWheels[i] = ws;
        }

        foreach(WheelCollider wc in _backwardWheels)
        {
            WheelFrictionCurve friction = wc.sidewaysFriction;
            friction.stiffness = _defaultRearWheelStiffness;
            wc.sidewaysFriction = friction;
        }
    }

    protected virtual void Update()
    {
        UpdateWheelsVisual();
    }

    private void LateUpdate()
    {
       // _lights.gameObject.SetActive(BrakingInput > 0);
    }

    public void SetSteerAngle(float turningAnlge)
    {
        TurningAngle = turningAnlge;

        if (IsDebug)
            Debug.Log($"Angle {turningAnlge}");

        float maxPossibleAngle = MaxWheelAngle;

        foreach (WheelCollider wheel in _forwardWheels)
            wheel.steerAngle = Mathf.Clamp(turningAnlge, -maxPossibleAngle, maxPossibleAngle);
    }

    public void SetMotorTorque(float motorInput)
    {
        AccelerationInput = motorInput;
        float motorTorq = 0;

        if (IsCarMovingForward)
            motorTorq = _torqueBySpeed.Evaluate(CarSpeed) * motorInput;
        else
            motorTorq = _reverceTorqueBySpeed.Evaluate(CarSpeed) * motorInput;
        
        //If car speed != 0, add motor resistance force
        if (CarSpeed > 0.01f)
            motorTorq += _motorTorqueResistance * (IsCarMovingForward ? -1 : 1);

        if (IsDebug)
            Debug.Log($"Motor: {motorTorq};");

        foreach (WheelCollider wheel in _motorWheels)
            wheel.motorTorque = motorTorq;
    }

    public void SetBrakeTorque(float brakeInput)
    {
        BrakingInput = brakeInput;
        float brakeTorq = _brakeTorque * brakeInput;

        if (IsDebug)
            Debug.LogError($"Brake: {brakeTorq}");

        _currentBrakeTorque = brakeTorq;
        foreach (WheelCollider wheel in _allWheels)
            wheel.brakeTorque = brakeTorq;
    }

    public void SetEBrake(bool isEnable)
    {
        EBrakeInput = isEnable;

        if (IsDebug)
            Debug.Log($"HandBrake: {isEnable}");

        foreach (WheelCollider wheel in _backwardWheels)
        {
            WheelFrictionCurve friction = wheel.sidewaysFriction;
            if (isEnable)
            {
                friction.stiffness = _handbrakeRearWheelStiffness;
                wheel.brakeTorque = _currentBrakeTorque + _handBrakeTorque;
            }
            else
            {
                friction.stiffness = _defaultRearWheelStiffness;
                wheel.brakeTorque = _currentBrakeTorque;
            }

            wheel.sidewaysFriction = friction;
        }
    }

    private void UpdateWheelsVisual()
    {
        if (_allWheels.Length != _visualWheels.Length)
            return;

        //Set visual wheels position & rotation by physics wheel
        for (int i = 0; i < _allWheels.Length; i++)
        {
            Quaternion q; Vector3 p;
            _allWheels[i].GetWorldPose(out p, out q);

            //If wheel is left
            if (_allWheels[i].transform.localPosition.x < 0)
            {
                _visualWheels[i].transform.rotation = q * Quaternion.Euler(0, 180, 0);
                _visualWheels[i].transform.position = p;
            }
            else
            {
                _visualWheels[i].transform.position = p;
                _visualWheels[i].transform.rotation = q;
            }
        }
    }
}
