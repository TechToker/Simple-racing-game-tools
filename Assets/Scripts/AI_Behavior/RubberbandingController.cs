using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubberbandingController : MonoBehaviour
{
    [Range(0.5f, 1.5f)]
    public float RubberBandingValue = 1;

    [ReadOnly] public float RbTorqueMultiplyer = 1;
    [ReadOnly] public float RbAccelerationSpeedMultiplyer = 200;
    [ReadOnly] public float RbBrakingDistanceMultiplyer = 1;

    [SerializeField] private float _minTorqueMultiplier = 1f;
    [SerializeField] private float _maxTorqueMultiplier = 2f;
    
    [SerializeField] private float _minAccelPedalSpeedMultiplier = 0.5f;
    [SerializeField] private float _maxAccelPedalSpeedMultiplier = 50f;
    
    [SerializeField] private float _minBrakingDistanceMultiplier = 3f;
    [SerializeField] private float _maxBrakingDistanceMultiplier = 1f;
    
    public AnimationCurve RBTorqueMultiplyerBySpeed;
    
    private void OnValidate()
    {
        float positiveRbPercent = Mathf.Clamp((RubberBandingValue - 1) / (1.5f - 1), 0, float.MaxValue);
        float negativeRbPercent = (Mathf.Clamp(RubberBandingValue, 0.5f, 1f) - 0.5f) / 0.5f;
        
        RbTorqueMultiplyer = _minTorqueMultiplier + positiveRbPercent * (_maxTorqueMultiplier - _minTorqueMultiplier);
        RbAccelerationSpeedMultiplyer = _minAccelPedalSpeedMultiplier + negativeRbPercent * (_maxAccelPedalSpeedMultiplier - _minAccelPedalSpeedMultiplier);
        RbBrakingDistanceMultiplyer = _minBrakingDistanceMultiplier + negativeRbPercent * (_maxBrakingDistanceMultiplier - _minBrakingDistanceMultiplier);
    }
}
