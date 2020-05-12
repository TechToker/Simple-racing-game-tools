using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class RacingLapTimer : MonoBehaviour
{
    public event Action<BaseDriver, float> OnLapComplete; 
    private readonly Dictionary<BaseDriver, float> _lapTimes = new Dictionary<BaseDriver, float>();

    private int _obstacleColliderMask;
    
    private void Awake()
    {
        _obstacleColliderMask = LayerMask.GetMask("Car");
    }

    public void OnTriggerEnter(Collider other)
    {
        if(_obstacleColliderMask != (_obstacleColliderMask | (1 << other.gameObject.layer)))
            return;
            
        BaseDriver driver = other.GetComponentInParent<BaseDriver>();
        if(driver == null)
            return;
        
        if (_lapTimes.ContainsKey(driver))
        {
            OnLapComplete?.Invoke(driver, _lapTimes[driver]);
            _lapTimes[driver] = 0;
        }
        else
        {
            _lapTimes.Add(driver, 0);
        }
    }

    //TODO: Count time from TriggerEnter to first Update function call; OnTriggerEnter calls every fixedUpdate
    public void Update()
    {
        List<BaseDriver> onLapDrivers = _lapTimes.Keys.ToList();
        foreach (BaseDriver onLapDriver in onLapDrivers)
            _lapTimes[onLapDriver] += Time.deltaTime;
    }
}
