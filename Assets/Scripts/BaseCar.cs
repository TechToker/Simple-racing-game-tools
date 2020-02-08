using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCar : MonoBehaviour
{
    public bool IsDebug;

    [SerializeField] protected Rigidbody _mainRigidBody;

    [SerializeField] protected WheelCollider[] _allWheels;

    [SerializeField] protected WheelCollider[] _forwardWheels;
    [SerializeField] protected WheelCollider[] _motorWheels;

    [SerializeField] private GameObject _wheelVisualPrefab;

    [SerializeField] protected AnimationCurve _turningAngleBySpeed;

    [SerializeField] protected AnimationCurve _torqueBySpeed;
    [SerializeField] protected AnimationCurve _reverceTorqueBySpeed;

    [SerializeField] protected float _brakeTorque = 700f;
    [SerializeField] protected float _motorTorqueResistance = 5;

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

    protected float CarSpeed => _mainRigidBody.velocity.magnitude;
    protected bool IsCarMovingForward => _mainRigidBody.transform.InverseTransformDirection(_mainRigidBody.velocity).z >= 0;

    private GameObject[] _visualWheels;

    void Start()
    {
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
    }

    protected virtual void Update()
    {
        UpdateWheelsVisual();
    }

    protected void SetSteerAngle(float turningAngle)
    {
        if (IsDebug)
            Debug.Log($"Angle {turningAngle}");

        foreach (WheelCollider wheel in _forwardWheels)
            wheel.steerAngle = turningAngle;
    }

    protected void SetMotorTorque(float motorTorq)
    {
        //If car speed != 0, add motor resistance force
        if (CarSpeed > 0.01f)
            motorTorq += _motorTorqueResistance * (IsCarMovingForward ? -1 : 1);

        if (IsDebug)
            Debug.Log($"Motor: {motorTorq};");

        foreach (WheelCollider wheel in _motorWheels)
            wheel.motorTorque = motorTorq;
    }

    protected void SetBrakeTorque(float brakeTorq)
    {
        if (IsDebug)
            Debug.Log($"Brake: {brakeTorq};");

        foreach (WheelCollider wheel in _allWheels)
            wheel.brakeTorque = brakeTorq;
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
