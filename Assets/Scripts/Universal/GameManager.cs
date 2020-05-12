using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CameraManager _cameraManager;

    public CameraManager CameraManager => _cameraManager;


    private static GameManager _instance;
    public static GameManager Instance
    {
        get 
        {
            if (_instance == null)
                _instance = FindObjectOfType<GameManager>();

            return _instance;
        }
    }
}
