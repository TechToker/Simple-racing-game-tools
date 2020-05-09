using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

namespace BehaviourAI
{
    public class CarSensor
    {
        public Vector3 SensorOffset { get; }
        public float SensorAngle { get;}
        public float SensorDistance { get; }

        public bool IsDetected;

        private float _weight = 0;

        public float Weight => IsDetected? _weight: 0;

        private readonly Transform _targetTransform;

        public CarSensor(Transform targetTransform, Vector3 fromPosition, float angle, float weight, float sensorDistance = 12)
        {
            _targetTransform = targetTransform;
            SensorOffset = fromPosition;
            SensorAngle = angle;

            _weight = weight;

            SensorDistance = sensorDistance;
        }

        public Vector3 SensorPosition => _targetTransform.TransformPoint(SensorOffset);
        public Vector3 SensorDirection => Quaternion.Euler(0, SensorAngle, 0) * _targetTransform.forward * SensorDistance;
    }

    public class BaseState
    {
        public DriverAI Driver;
        public BaseCar Car;
        
        protected List<CarSensor> _allSensors = new List<CarSensor>();
        private int SensorLayer;

        public BaseState(DriverAI driver, BaseCar car)
        {
            Driver = driver;
            Car = car;
            
            float offset = 0.2f;
            
            //Catfish whiskers
            float frontSensorDistance = 12f;
            float frontSideSensorSideOffset = 3f;

            float frontSideSonsorDistance = Mathf.Sqrt(Mathf.Pow(frontSensorDistance, 2) + (Mathf.Pow(frontSideSensorSideOffset, 2)));
            float frontSideSensorAngle = Mathf.Atan2(frontSideSensorSideOffset, frontSensorDistance) * Mathf.Rad2Deg;
            
            _allSensors.AddRange(
            new List<CarSensor>
                {
                    //Forward
                    new CarSensor(Car.transform, new Vector3(-Car.CarSize.x / 2 + offset, 0, Car.CarSize.y / 2 - offset), 0, -0.3f, frontSensorDistance),
                    new CarSensor(Car.transform, new Vector3(0, 0, Car.CarSize.y / 2 - offset), 0, 0.1f, frontSensorDistance),
                    new CarSensor(Car.transform, new Vector3(Car.CarSize.x / 2 - offset, 0, Car.CarSize.y / 2 - offset), 0,0.3f, frontSensorDistance),

                    //Catfish whiskers
                    new CarSensor(Car.transform, new Vector3(-Car.CarSize.x / 2 + offset, 0, Car.CarSize.y / 2 - offset), -frontSideSensorAngle, -0.2f, frontSideSonsorDistance),
                    new CarSensor(Car.transform, new Vector3(Car.CarSize.x / 2 - offset, 0, Car.CarSize.y / 2 - offset), frontSideSensorAngle, 0.2f, frontSideSonsorDistance),
                }
            );

            _allSensors.AddRange(
                new List<CarSensor>
                { 
                    new CarSensor(Car.transform, new Vector3(0, 0, -Car.CarSize.y / 2 + offset), -180, 0, 2)
                });
            
            _allSensors.AddRange(new List<CarSensor>
            {
                //Left
                new CarSensor(Car.transform, new Vector3(-Car.CarSize.x / 2 + offset, 0, Car.CarSize.y / 2 - offset), -90, 0f, 2),
                new CarSensor(Car.transform, new Vector3(-Car.CarSize.x / 2 + offset, 0, -Car.CarSize.y / 2 + offset), -90, 0f, 2),
                
                //Right
                new CarSensor(Car.transform, new Vector3(Car.CarSize.x / 2 - offset, 0, Car.CarSize.y / 2 - offset), 90, 0f, 2),
                new CarSensor(Car.transform, new Vector3(Car.CarSize.x / 2 - offset, 0, -Car.CarSize.y / 2 + offset), 90, 0f, 2),
            });
            
            SensorLayer = LayerMask.GetMask("Car", "Obstacle");
        }

        public virtual void OnDrawGizmos()
        {
            foreach(CarSensor sensor in _allSensors)
            {
                Gizmos.color = sensor.IsDetected? Color.red : Color.white;
                Gizmos.DrawRay(sensor.SensorPosition, sensor.SensorDirection);
            }
        }


        public virtual void OnUpdate()
        {
            foreach (CarSensor sensor in _allSensors)
            {
                RaycastHit hit;
                sensor.IsDetected = Physics.Raycast(sensor.SensorPosition, sensor.SensorDirection,
                    out hit, sensor.SensorDistance, SensorLayer, QueryTriggerInteraction.Ignore);
            }

            Driver.ObstacleAvoidanceWeight = _allSensors.Sum(item => item.Weight);
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
