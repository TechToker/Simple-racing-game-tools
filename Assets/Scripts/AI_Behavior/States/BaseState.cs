using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

namespace BehaviourAI
{
    public class BaseState
    {
        protected readonly DriverAI Driver;
        protected readonly BaseCar Car;
        
        public BaseState(DriverAI driver, BaseCar car)
        {
            Driver = driver;
            Car = car;
        }

        public virtual void OnEveryGizmosDraw()
        {
        }

        public virtual void OnEveryUpdate()
        {
        }

        public virtual void OnEveryFixedUpdate()
        {
        }

        public virtual string GetStateName()
        {
            return "[Base state]";
        }
    }
}
