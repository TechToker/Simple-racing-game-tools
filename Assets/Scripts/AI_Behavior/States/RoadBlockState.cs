using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAI
{
    public class RoadBlockState : BaseState
    {
        private Vector3 _startPos;
        private Vector3 _roadBlockCrossingLine;

        private Vector3 _targetCarPos;

        private bool _startTargetPositionFromVector;

        public RoadBlockState(DriverAI ai, BaseCar car) : base(ai, car)
        {
            _startPos = ai.transform.position;
            _targetCarPos = Driver.TargetCar.transform.position;
        }

        public override void OnEveryGizmosDraw()
        {
            base.OnEveryGizmosDraw();

            Gizmos.color = Color.red;
            Gizmos.DrawLine(_startPos, _targetCarPos);

            Gizmos.color = Color.blue;

            Vector3 t = _startPos - _targetCarPos;
            t = Quaternion.Euler(0, 90, 0) * t;

            _roadBlockCrossingLine = t.normalized * 40;
            Gizmos.DrawRay(Driver.transform.position - new Vector3(0, 0, -20), _roadBlockCrossingLine);


            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(Driver.transform.position - new Vector3(0, 0, -20), Quaternion.Euler(0, 90, 0) * _roadBlockCrossingLine);

            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(Driver.transform.position - new Vector3(0, 0, -20), Driver.TargetCar.transform.position - (Driver.transform.position - new Vector3(0, 0, -20)));

            _startTargetPositionFromVector = Vector3.Dot(Quaternion.Euler(0, 90, 0) * _roadBlockCrossingLine, Driver.TargetCar.transform.position - (Driver.transform.position - new Vector3(0, 0, -20))) > 0;
        }

        public override void OnEveryFixedUpdate()
        {
            Car.SetSteerAngle(-90);
            Car.SetMotorTorque(0);
            Car.SetEBrake(true);
        }

        public override void OnEveryUpdate()
        {
            if(Vector3.Dot(Quaternion.Euler(0, 90, 0) * _roadBlockCrossingLine, Driver.TargetCar.transform.position - (Driver.transform.position - new Vector3(0, 0, -20))) > 0 != _startTargetPositionFromVector)
            {
                Driver.SetDriverMode(DriverMode.Chase);
            }
        }

        public override string GetStateName()
        {
            return "[Roadblock state]";
        }
    }
}
