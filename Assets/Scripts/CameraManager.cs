using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera _controllableCarExtraCamera;
    [SerializeField] private List<CinemachineVirtualCamera> _allCarsVirtualCameras;

    public int CurrentTarget;
    public string CurrentCamName { get; private set; }

    public void Awake()
    {
        SetTargetID(0);
    }

    public void SelectNextCamera()
    {
        if (CurrentTarget + 1 == _allCarsVirtualCameras.Count)
            SetTargetID(0);
        else
            SetTargetID(CurrentTarget + 1);
    }

    public void SelectPrevCamera()
    {
        if (CurrentTarget == 0)
            SetTargetID(_allCarsVirtualCameras.Count - 1);
        else
            SetTargetID(CurrentTarget - 1);
    }

    public void SetTargetID(int id)
    {
        CurrentTarget = id;

        for (int i = 0; i < _allCarsVirtualCameras.Count; i++)
        {
            if (i == id)
            {
                _allCarsVirtualCameras[i].gameObject.SetActive(true);
                CurrentCamName = _allCarsVirtualCameras[i].gameObject.name;
            }
            else
            {
                _allCarsVirtualCameras[i].gameObject.SetActive(false);
            }
        }
    }
}
