using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LiveTimingContainerUI : MonoBehaviour
{
    [SerializeField] private RacingLapTimer _lapTimer;
    
    private Dictionary<BaseDriver, CarTimingContainerUI> _timingByCar = new Dictionary<BaseDriver, CarTimingContainerUI>();
    private CarTimingContainerUI _carTimingContainerPrefab;
    
    private void Awake()
    {
        _carTimingContainerPrefab = GetComponentInChildren<CarTimingContainerUI>();
        _carTimingContainerPrefab.gameObject.SetActive(false);
    }
    
    private void OnEnable()
    {
        _lapTimer.OnLapComplete += OnLapComplete;
    }
    
    private void OnLapComplete(BaseDriver driver, float lapTime)
    {
        string formatTime = ConvertSecondsToString(lapTime);
        
        if (_timingByCar.TryGetValue(driver, out CarTimingContainerUI timingContainer))
        {
            timingContainer.TextLapTimes.SetText(timingContainer.TextLapTimes.text + Environment.NewLine + formatTime);
        }
        else
        {
            CarTimingContainerUI carTiming = GetCarTimingContainer();
            carTiming.TextCarName.SetText(driver.Car.name);
            carTiming.TextLapTimes.SetText(formatTime);
            
            _timingByCar.Add(driver, carTiming);
        }
    }

    private CarTimingContainerUI GetCarTimingContainer()
    {
        if (!_carTimingContainerPrefab.gameObject.activeSelf)
        {
            _carTimingContainerPrefab.gameObject.SetActive(true);
            return _carTimingContainerPrefab;
        }
        
        return Instantiate(_carTimingContainerPrefab, transform).GetComponent<CarTimingContainerUI>();
    }

    private string ConvertSecondsToString(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return time.ToString(time.Minutes > 0 ? "mm':'ss'.'fff" : "ss'.'fff");
    }
    
    private void OnDisable()
    {
        _lapTimer.OnLapComplete -= OnLapComplete;
    }
}
