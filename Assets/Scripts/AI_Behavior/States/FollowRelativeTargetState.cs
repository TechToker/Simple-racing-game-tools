using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAI
{
    public class FollowRelativeTargetState : BaseState
    {
        public Vector3 offsetFromTarget = new Vector3(3f, 0, 0f);


        private Vector3 offsetFromTarget1 = new Vector3(3f, 0, 7f);

        private Vector3 relativeTarget;
        private Vector3 predictTarget;

        private float _distanceToTarget;
        private float _nearTargetTime;
        private float _targetAttackTime = 2;


        public FollowRelativeTargetState(DriverAI ai, BaseCar car) : base(ai, car)
        {

        }

        public override void OnEveryGizmosDraw()
        {
            base.OnEveryGizmosDraw();

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(relativeTarget, Vector3.one);

            Gizmos.DrawRay(Driver.TargetCar.transform.position, Driver.TargetCar.RelativeForce);


            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(predictTarget, Vector3.one);
        }

        public override void OnEveryFixedUpdate()
        {
            base.OnEveryFixedUpdate();
            FollowTarget(offsetFromTarget);
        }

        protected void FollowTarget(Vector3 targetCarLocalPosition)
        {
            relativeTarget = Driver.TargetCar.transform.TransformPoint(targetCarLocalPosition);
            predictTarget = Driver.TargetCar.transform.position + Driver.TargetCar.RelativeForce + Driver.TargetCar.transform.rotation * targetCarLocalPosition;

            float angle = GetAngleToBetweenTransfors(Driver.transform, relativeTarget);

            bool isTargetInFront = angle > -90 && angle < 90;

            // Направление движения автомобилей различаются менее чем на 15 градусов
            bool isPredictDirectionSame = Mathf.Abs(Quaternion.LookRotation(Driver.TargetCar.RelativeForce).eulerAngles.y - Quaternion.LookRotation(Driver.Car.RelativeForce).eulerAngles.y) < 15f;

            _distanceToTarget = Vector3.Distance(Car.transform.position, relativeTarget) * (isTargetInFront ? 1 : -1);
            float speedDifference = Driver.TargetCar.CarSpeed - Car.CarSpeed;

            float distanceInput = Driver.DistanceFactor.Evaluate(Mathf.Abs(_distanceToTarget)) * (isTargetInFront ? 1 : -1);

            float deltaSpeedInput = Driver.DeltaSpeedFactor.Evaluate(Mathf.Abs(speedDifference)) * Mathf.Sign(speedDifference);
            float deltaSpeedInputM = deltaSpeedInput * Driver.DeltaSpeedMultyplyerByDistance.Evaluate(_distanceToTarget);

            float moveInput = Mathf.Clamp(distanceInput + deltaSpeedInputM, -1, 1);

            Debug.Log($"Dist: {_distanceToTarget}; DistInp: {distanceInput}; SpeedDelta: {speedDifference}; SpeedDI: {deltaSpeedInput}; SInputM {deltaSpeedInputM} Result: {moveInput}");

            Car.SetMotorTorque(Mathf.Clamp(moveInput, 0, 1));
            Car.SetBrakeTorque(Mathf.Clamp(-moveInput, 0, 1));

            float wheelAngle = 0;
            if (isPredictDirectionSame)
            {
                if (Mathf.Abs(_distanceToTarget) > 4f)
                {
                    wheelAngle = angle;
                }
                else
                {
                    wheelAngle = GetAngleToBetweenTransfors(Driver.transform, predictTarget);
                }
            }
            else
            {
                wheelAngle = angle;
            }

            //Debug.LogError($"Target angle: {wheelAngle}; Angle: {Mathf.Lerp(Driver.Car.CurrentWheelAngle, wheelAngle, Time.fixedDeltaTime * Driver.WheelAngleSpeed)}");
            Car.SetSteerAngle(wheelAngle);
        }

        public override void OnEveryUpdate()
        {
            if (_distanceToTarget < 1.5f)
            {
                _nearTargetTime += Time.deltaTime;
            } else
            {
                _nearTargetTime = 0;
            }

            if (_nearTargetTime > _targetAttackTime && offsetFromTarget != offsetFromTarget1)
            {
                ChangeRelativePosition(offsetFromTarget1);
                _nearTargetTime = 0;
            }

            if (_nearTargetTime > _targetAttackTime && offsetFromTarget == offsetFromTarget1)
            {
                Driver.SetDriverMode(DriverMode.PitManeuver);
            }

            Debug.Log($"{_nearTargetTime}");

            base.OnEveryUpdate();
        }

        private void ChangeRelativePosition(Vector3 newRelativePos)
        {
            offsetFromTarget = newRelativePos;
        }


        //DUPLICATE from chase state
        private float GetAngleToBetweenTransfors(Transform from, Vector3 to)
        {
            Vector3 deltaVec = from.InverseTransformPoint(to);
            return Mathf.Atan2(deltaVec.x, deltaVec.z) * Mathf.Rad2Deg;
        }
        public override string GetStateName()
        {
            return "[Follow state]";
        }
    }
}
