using BehaviourAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrakingByTargetState : BaseState
{
    private Vector3 _finishBrakingPosition;

    public BrakingByTargetState(Vector3 pos, DriverAI driver, BaseCar car) : base(driver, car)
    {
        _finishBrakingPosition = pos;
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        DrawCarBraking();
    }

    private void DrawCarBraking()
    {

    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        DOBrakingByTarget();
    }

    private void DOBrakingByTarget()
    {
        float distanceToTarget = Vector2.Distance(Driver.transform.position, _finishBrakingPosition) - 3f;
        float expectedSpeed = Driver.TargetSpeedByTarget.Evaluate(distanceToTarget);

        if (Car.CarSpeed > expectedSpeed)
        {
            Debug.LogError($"Brake with {Driver._brakingPedalForce}; Current Speed: {Car.CarSpeed}; Expected speed: {expectedSpeed}; Brake more!");
            Driver._brakingPedalForce += Driver._brakingPedalForceDelta;
        }
        else
        {
            Debug.LogError($"Brake with {Driver._brakingPedalForce}; Current Speed: {Car.CarSpeed}; Expected speed: {expectedSpeed}; Brake lower!");
            Driver._brakingPedalForce -= Driver._brakingPedalForceDelta;
        }

        Driver._brakingPedalForce = Mathf.Clamp(Driver._brakingPedalForce, 0, 1f);

        Car.SetBrakeTorque(Driver._brakingPedalForce);
        Car.SetMotorTorque(0);
    }

    public override string GetStateName()
    {
        return "[Brake by target]";
    }
}
