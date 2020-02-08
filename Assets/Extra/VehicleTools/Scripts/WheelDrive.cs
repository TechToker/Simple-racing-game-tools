using UnityEngine;
using System;

[Serializable]
public class WheelDrive : BaseCar
{
	protected override void Update()
	{
        float turningAngle = _turningAngleBySpeed.Evaluate(CarSpeed) * Input.GetAxis("Horizontal");
        float verticalMovementForce = Input.GetAxis("Vertical");

        float motorTorq = 0, brakeTorq = 0;

        if (IsCarMovingForward)
        {
            if (verticalMovementForce >= 0)
                motorTorq = _torqueBySpeed.Evaluate(CarSpeed) * verticalMovementForce;
            else
                brakeTorq = _brakeTorque * -verticalMovementForce;
        }
        else
        {
            if (verticalMovementForce >= 0)
                brakeTorq = _brakeTorque * verticalMovementForce;
            else
                motorTorq = _reverceTorqueBySpeed.Evaluate(CarSpeed) * verticalMovementForce;
        }

        SetSteerAngle(turningAngle);
        SetMotorTorque(motorTorq);
        SetBrakeTorque(brakeTorq);

        //if(IsDebug)
        //    Debug.Log($"Motor: {motorTorq}; Brake: {brakeTorq}; Angle {turningAngle}");

        base.Update();
    }
}
