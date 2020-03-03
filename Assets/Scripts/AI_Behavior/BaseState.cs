using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BehaviourAI
{
    public class BaseState
    {
        public DriverAI Driver;
        public BaseCar Car;

        public BaseState(DriverAI driver, BaseCar car)
        {
            Driver = driver;
            Car = car;
        }

        public virtual void OnDrawGizmos()
        {
        }

        public virtual void OnUpdate()
        {

        }


        public virtual void FixedUpdate()
        {
        }

        public virtual string GetStateName()
        {
            return "[Base state]";
        }
    }
}
