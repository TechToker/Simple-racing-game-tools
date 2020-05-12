using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAI {
    public class PitManeuverState : FollowRelativeTargetState
    {
        public PitManeuverState(DriverAI ai, BaseCar car) : base(ai, car)
        {

        }

        public override void OnEveryGizmosDraw()
        {
            base.OnEveryGizmosDraw();
        }

        public override void OnEveryFixedUpdate()
        {
            FollowTarget(Vector3.zero);
        }

        public override void OnEveryUpdate()
        {
            base.OnEveryUpdate();
        }

        public override string GetStateName()
        {
            return "[Pit state]";
        }
    }
}
