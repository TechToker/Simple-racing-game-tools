using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : BaseDriver
{
    protected void FixedUpdate()
    {
        float turningInput = Input.GetAxis("Horizontal") * Car.MaxWheelAngle;
        float motorInput = Input.GetAxis("Vertical");

        bool isEBrakeEnable = Input.GetKey(KeyCode.Space);

        if (Car.IsCarMovingForward)
        {
            Car.SetMotorTorque(Mathf.Clamp(motorInput, 0, 1));
            Car.SetBrakeTorque(-Mathf.Clamp(motorInput, -1, 0));
        } else
        {
            Car.SetMotorTorque(Mathf.Clamp(motorInput, -1, 0));
            Car.SetBrakeTorque(Mathf.Clamp(motorInput, 0, 1));
        }

        Car.SetEBrake(isEBrakeEnable);
        Car.SetSteerAngle(turningInput);
    }
}
