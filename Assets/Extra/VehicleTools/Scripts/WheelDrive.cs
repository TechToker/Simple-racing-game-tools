using UnityEngine;
using System;

[Serializable]
public enum DriveType
{
	RearWheelDrive,
	FrontWheelDrive,
	AllWheelDrive
}

public class WheelDrive : MonoBehaviour
{
    [SerializeField] private Rigidbody _mainRigidBody;

    [SerializeField] private WheelCollider[] _allWheels;

    [SerializeField] private WheelCollider[] _forwardWheels;
    [SerializeField] private WheelCollider[] _motorWheels;

    [SerializeField] private GameObject _wheelVisualPrefab;

	[SerializeField] private AnimationCurve _turningAngleBySpeed;

    [SerializeField] private AnimationCurve _torqueBySpeed;
    [SerializeField] private AnimationCurve _reverceTorqueBySpeed;

    [SerializeField] private float _brakeTorque = 700f;
    [SerializeField] private float _motorTorqueResistance = 5;

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

	void Update()
	{
        float carSpeed = _mainRigidBody.velocity.magnitude;

        float turningAngle = _turningAngleBySpeed.Evaluate(carSpeed) * Input.GetAxis("Horizontal");
        float verticalMovementForce = Input.GetAxis("Vertical");

        bool isCarMovingForward = _mainRigidBody.transform.InverseTransformDirection(_mainRigidBody.velocity).z >= 0;
        float motorTorq = 0, brakeTorq = 0;

        //IsCarMovingForward
        if (isCarMovingForward)
        {
            if (verticalMovementForce >= 0)
                motorTorq = _torqueBySpeed.Evaluate(carSpeed) * verticalMovementForce;
            else
                brakeTorq = _brakeTorque * -verticalMovementForce;
        }
        else
        {
            if (verticalMovementForce >= 0)
                brakeTorq = _brakeTorque * verticalMovementForce;
            else
                motorTorq = _reverceTorqueBySpeed.Evaluate(carSpeed) * verticalMovementForce;
        }

        //If car speed != 0, add motor resistance force
        if (carSpeed > 0.01f)
            motorTorq += _motorTorqueResistance * (isCarMovingForward ? -1: 1);

        foreach (WheelCollider wheel in _forwardWheels)
            wheel.steerAngle = turningAngle;

        foreach (WheelCollider wheel in _motorWheels)
            wheel.motorTorque = motorTorq;

        foreach (WheelCollider wheel in _allWheels)
            wheel.brakeTorque = brakeTorq;

        Debug.Log($"Motor: {motorTorq}; Brake: {brakeTorq}; Angle {turningAngle}");

        UpdateWheelsVisual();
    }

    private void UpdateWheelsVisual()
    {
        if (_allWheels.Length != _visualWheels.Length)
            return;

        //Set visual wheels position & rotation by physics wheel
        for(int i = 0; i < _allWheels.Length; i++)
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
