using UnityEngine;

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