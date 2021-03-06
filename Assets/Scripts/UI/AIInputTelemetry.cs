﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RacingGame.UI
{
    public class AIInputTelemetry : MonoBehaviour
    {
        [SerializeField] private DriverAI _driver;

        [Header("Data containers")]
        [SerializeField] private TextMeshProUGUI _uiTextBehaviourState;
        [SerializeField] private TextMeshProUGUI _uiTextSpeed;
        [SerializeField] private TextMeshProUGUI _uiTextRubberbandingValue;
        [SerializeField] private TextMeshProUGUI _uiTextObstacleAvoidanceValue;
        
        [SerializeField] private Slider _uiSliderTurningInput;
        [SerializeField] private Slider _uiSliderAcceleationInput;
        [SerializeField] private Slider _uiSliderBrakingInput;
        [SerializeField] private Toggle _uiToggleEBrake;

        private void LateUpdate()
        {
            _uiTextBehaviourState.SetText(_driver.CurrentStateName);
            _uiTextSpeed.SetText($"{_driver.Car.CarSpeed:N1}m/s");
            _uiTextRubberbandingValue.SetText($"RB:{_driver.RubberbandingController.RubberBandingValue:N2}");
            _uiTextObstacleAvoidanceValue.SetText($"OA: {_driver.ObstacleAvoidanceWeight}");

            _uiSliderTurningInput.value = (_driver.Car.TurningAngle + _driver.Car.MaxWheelAngle) / (_driver.Car.MaxWheelAngle * 2);
            _uiSliderAcceleationInput.value = Mathf.Abs(_driver.Car.AccelerationInput);
            _uiSliderBrakingInput.value = Mathf.Abs(_driver.Car.BrakingInput);
            _uiToggleEBrake.isOn = _driver.Car.EBrakeInput;
        }
    }
}
