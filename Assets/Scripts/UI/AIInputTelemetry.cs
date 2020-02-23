using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RacingGame.UI
{
    public class AIInputTelemetry : MonoBehaviour
    {
        [SerializeField] private BaseCar _targetCar;

        [Header("Data containers")]
        [SerializeField] private Slider _uiSliderTurningInput;
        [SerializeField] private Slider _uiSliderAcceleationInput;
        [SerializeField] private Slider _uiSliderBrakingInput;
        [SerializeField] private Toggle _uiToggleEBrake;

        private void LateUpdate()
        {
            _uiSliderTurningInput.value = (_targetCar.TurningAngle + _targetCar.MaxWheelAngle) / (_targetCar.MaxWheelAngle * 2);
            _uiSliderAcceleationInput.value = Mathf.Abs(_targetCar.AccelerationInput);
            _uiSliderBrakingInput.value = Mathf.Abs(_targetCar.BrakingInput);
            _uiToggleEBrake.isOn = _targetCar.EBrakeInput;
        }
    }
}
