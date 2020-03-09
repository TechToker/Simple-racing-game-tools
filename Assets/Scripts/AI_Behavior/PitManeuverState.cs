using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourAI {
    public class PitManeuverState : FollowRelativeTargetState
    {
        public PitManeuverState(DriverAI ai, BaseCar car) : base(ai, car)
        {

        }

        public override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
        }

        public override void FixedUpdate()
        {
            FollowTarget(Vector3.zero);
        }

        public override void OnUpdate()
        {
        }

        public override string GetStateName()
        {
            return "[Pit state]";
        }
    }
}
