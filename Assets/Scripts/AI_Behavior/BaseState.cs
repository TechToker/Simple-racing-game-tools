using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

namespace BehaviourAI
{
    public class CarSensor
    {
        public Vector3 SensorOffset { get; private set; }
        public float SensorAngle { get; private set; }

        public bool IsDetected;
        public float SensorDistance = 12;

        public Transform _targetTransform;

        public CarSensor(Transform targetTransform, Vector3 position, float angle)
        {
            _targetTransform = targetTransform;
            SensorOffset = position;
            SensorAngle = angle;
        }

        public Vector3 SensorPosition => _targetTransform.TransformPoint(SensorOffset);
        public Vector3 SensorDirection => Quaternion.Euler(0, SensorAngle, 0) * _targetTransform.forward * SensorDistance;

    }

    public class BaseState
    {
        public DriverAI Driver;
        public BaseCar Car;

        public float CarWidth = 1.7f;
        public float CarLength = 3.6f;

        //public float SensorDistance = 12;

        protected CarSensor[] _frontSensors;
        protected CarSensor[] _backwardSensors;
        protected List<CarSensor> _allSensors = new List<CarSensor>();

        public BaseState(DriverAI driver, BaseCar car)
        {
            Driver = driver;
            Car = car;

            _frontSensors = new CarSensor[]
            {
                new CarSensor(Car.transform, new Vector3(-CarWidth / 2, 0, CarLength / 2), 0),
                new CarSensor(Car.transform, new Vector3(0, 0, CarLength / 2), 0),
                new CarSensor(Car.transform, new Vector3(CarWidth / 2, 0, CarLength / 2), 0),

                new CarSensor(Car.transform, new Vector3(-CarWidth / 2, 0, CarLength / 2), -20),
                new CarSensor(Car.transform, new Vector3(CarWidth / 2, 0, CarLength / 2), 20),
            };

            _backwardSensors = new CarSensor[]
            {
                new CarSensor(Car.transform, new Vector3(-CarWidth / 2, 0, -CarLength / 2), -180),
                new CarSensor(Car.transform, new Vector3(0, 0, -CarLength / 2), -180),
                new CarSensor(Car.transform, new Vector3(CarWidth / 2, 0, -CarLength / 2), -180),
            };

            _allSensors.AddRange(_frontSensors);
            _allSensors.AddRange(_backwardSensors);
        }

        public virtual void OnDrawGizmos()
        {
//            foreach(CarSensor sensor in _allSensors)
//            {
//                Gizmos.color = sensor.IsDetected? Color.red : Color.white;
//                Gizmos.DrawRay(sensor.SensorPosition, sensor.SensorDirection);
//            }
        }

        private bool _isCarMoveForward;

        public virtual void OnUpdate()
        {
            foreach (CarSensor sensor in _allSensors)
            {
                RaycastHit hit;
                sensor.IsDetected = Physics.Raycast(sensor.SensorPosition, sensor.SensorDirection, out hit, sensor.SensorDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            }

            if (!_isCarMoveForward && _backwardSensors.Take(3).Where(item => item.IsDetected).Count() == 3)
            {
                _isCarMoveForward = true;
            }

            if (_isCarMoveForward && _frontSensors.Take(3).Where(item => item.IsDetected).Count() == 3)
            {
                _isCarMoveForward = false;
            }
        }


        public virtual void FixedUpdate()
        {
            if (_isCarMoveForward)
            {
                Driver.Car.SetMotorTorque(0.1f);
            } else
            {
                Driver.Car.SetMotorTorque(-0.1f);
            }


            float steerAngle = 0;

            if (_frontSensors[0].IsDetected)
                steerAngle += Driver.Car.MaxWheelAngle * 0.33f;
            else
                steerAngle -= Driver.Car.MaxWheelAngle * 0.33f;

            if (_frontSensors[2].IsDetected)
                steerAngle -= Driver.Car.MaxWheelAngle * 0.33f;
            else
                steerAngle += Driver.Car.MaxWheelAngle * 0.33f;

            //Driver.Car.SetSteerAngle(steerAngle);
        }

        public virtual string GetStateName()
        {
            return "[Base state]";
        }
    }
}
