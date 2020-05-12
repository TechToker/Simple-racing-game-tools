using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarSensorController : BaseObstacleAvoidanceController
{
    private List<CarSensor> _allSensors = new List<CarSensor>();

    protected override void Awake()
    {
        base.Awake();
        InitSensors();
    }

    private void InitSensors()
    {
        BaseCar car = Driver.Car;
        float offset = 0.2f;

        //Catfish whiskers
        float frontSensorDistance = 12f;
        float frontSideSensorSideOffset = 3f;

        float frontSideSonsorDistance =
            Mathf.Sqrt(Mathf.Pow(frontSensorDistance, 2) + (Mathf.Pow(frontSideSensorSideOffset, 2)));
        float frontSideSensorAngle = Mathf.Atan2(frontSideSensorSideOffset, frontSensorDistance) * Mathf.Rad2Deg;

        _allSensors.AddRange(new List<CarSensor>
            {
                //Forward
                new CarSensor(car.transform, new Vector3(-car.CarSize.x / 2 + offset, 0, car.CarSize.y / 2 - offset), 0,
                    -0.3f, frontSensorDistance),
                new CarSensor(car.transform, new Vector3(0, 0, car.CarSize.y / 2 - offset), 0, 0.1f,
                    frontSensorDistance),
                new CarSensor(car.transform, new Vector3(car.CarSize.x / 2 - offset, 0, car.CarSize.y / 2 - offset), 0,
                    0.3f, frontSensorDistance),

                //Catfish whiskers
                new CarSensor(car.transform, new Vector3(-car.CarSize.x / 2 + offset, 0, car.CarSize.y / 2 - offset),
                    -frontSideSensorAngle, -0.2f, frontSideSonsorDistance),
                new CarSensor(car.transform, new Vector3(car.CarSize.x / 2 - offset, 0, car.CarSize.y / 2 - offset),
                    frontSideSensorAngle, 0.2f, frontSideSonsorDistance),
            }
        );

        _allSensors.AddRange(new List<CarSensor>
            {
                new CarSensor(car.transform, new Vector3(0, 0, -car.CarSize.y / 2 + offset), -180, 0, 2)
            });

        _allSensors.AddRange(new List<CarSensor>
        {
            //Left
            new CarSensor(car.transform, new Vector3(-car.CarSize.x / 2 + offset, 0, car.CarSize.y / 2 - offset), -90,
                0f, 2),
            new CarSensor(car.transform, new Vector3(-car.CarSize.x / 2 + offset, 0, -car.CarSize.y / 2 + offset), -90,
                0f, 2),

            //Right
            new CarSensor(car.transform, new Vector3(car.CarSize.x / 2 - offset, 0, car.CarSize.y / 2 - offset), 90, 0f,
                2),
            new CarSensor(car.transform, new Vector3(car.CarSize.x / 2 - offset, 0, -car.CarSize.y / 2 + offset), 90,
                0f, 2),
        });
    }

    private void OnDrawGizmos()
    {
        foreach(CarSensor sensor in _allSensors)
        {
            Gizmos.color = sensor.IsDetected? Color.red : Color.white;
            Gizmos.DrawRay(sensor.SensorPosition, sensor.SensorDirection);
        }
            
        Driver.ObstacleAvoidanceWeight = _allSensors.Sum(item => item.Weight);
    }

    private void Update()
    {
        foreach (CarSensor sensor in _allSensors)
        {
            RaycastHit hit;
            sensor.IsDetected = Physics.Raycast(sensor.SensorPosition, sensor.SensorDirection,
                out hit, sensor.SensorDistance, ObstacleColliderMask, QueryTriggerInteraction.Ignore);
        }
    }
}
