using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAI
{
    public class FollowState : BaseState
    {
        public Vector3 offsetFromTarget = new Vector3(0, 0, 15);

        private Vector3 relativeTarget;
        private Vector3 predictTarget;


        public FollowState(DriverAI ai, BaseCar car): base(ai, car)
        {

        }

        public override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(relativeTarget, Vector3.one);

            Gizmos.DrawRay(Driver.TargetCar.transform.position, Driver.TargetCar.RelativeForce);


            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(predictTarget, Vector3.one);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            float angle = GetAngleToBetweenTransfors(Driver.transform, relativeTarget);

            relativeTarget = Driver.TargetCar.transform.TransformPoint(offsetFromTarget);
            predictTarget = Driver.TargetCar.transform.position + Driver.TargetCar.RelativeForce + Driver.TargetCar.transform.rotation * offsetFromTarget;

            if (angle > -90 && angle < 90)
            {
                Debug.LogError($"Follow red sq");

                Car.SetBrakeTorque(0);
                Car.SetMotorTorque(1f);
                Car.SetSteerAngle(angle);
            } else
            {

                // Направление движения автомобилей различаются менее чем на 20 градусов
                if (Mathf.Abs(Quaternion.LookRotation(Driver.TargetCar.RelativeForce).eulerAngles.y - Quaternion.LookRotation(Driver.Car.RelativeForce).eulerAngles.y) < 15f)
                {
                    Debug.LogError($"Overatate red; Try predict pos and brake");

                    Car.SetMotorTorque(0f);
                    Car.SetBrakeTorque(0.5f);
                    Car.SetSteerAngle(GetAngleToBetweenTransfors(Driver.transform, predictTarget));
                }
                else
                {
                    Debug.LogError($"Follow red sq");

                    Car.SetBrakeTorque(0);
                    Car.SetMotorTorque(1f);
                    Car.SetSteerAngle(angle);
                }
            }
        }


        //DUPLICATE from chase state
        private float GetAngleToBetweenTransfors(Transform from, Vector3 to)
        {
            Vector3 deltaVec = from.InverseTransformPoint(to);
            return Mathf.Atan2(deltaVec.x, deltaVec.z) * Mathf.Rad2Deg;
        }
    }
}
