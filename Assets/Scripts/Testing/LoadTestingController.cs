using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LoadTestingController : MBSingleton<LoadTestingController>
{
    [SerializeField] private GameObject AICarPrefab;
    private float _timeScale = 1f;
    
    private List<GameObject> _aiCarPool = new List<GameObject>();

    private Vector3 _carSpawnPosition;
    private Quaternion _carSpawnRotation;

    public int CarCount => _aiCarPool.Count(item => item.activeSelf);

    private void Awake()
    {
        _carSpawnPosition = AICarPrefab.transform.position;
        _carSpawnRotation = AICarPrefab.transform.rotation;
        _aiCarPool.Add(AICarPrefab);
    }

    public void AddAiCar()
    {
        GameObject carFromPool = _aiCarPool.FirstOrDefault(gameObject => !gameObject.activeSelf);
        
        if (carFromPool == null)
        {
            GameObject newCar = Instantiate(AICarPrefab, _carSpawnPosition, _carSpawnRotation);
            _aiCarPool.Add(newCar);
        }
        else
        {
            carFromPool.transform.position = _carSpawnPosition;
            carFromPool.transform.rotation = _carSpawnRotation;
            
            carFromPool.gameObject.SetActive(true);
        }
    }

    public void RemoveAiCar()
    {
        GameObject carFromPool = _aiCarPool.Last(gameObject => gameObject.activeSelf);
        carFromPool.gameObject.SetActive(false);
    }

    private void Update()
    {
        Time.timeScale = _timeScale;
    }
}
