using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace RacingGame.UI
{
    public class MainWindowUI : MonoBehaviour
    {
        [Header("Car counter")] 
        [SerializeField] private TextMeshProUGUI _uiTextCarCounter;
        [SerializeField] private Button _uiButtonAddCar;
        [SerializeField] private Button _uiButtonRemoveCar;

        [Header("Camera selector")]
        [SerializeField] private TextMeshProUGUI _uiTextCurrentCarName;
        [SerializeField] private Button _uiButtonPrevCar;
        [SerializeField] private Button _uiButtonNextCar;
        [SerializeField] private RawImage _uiRawImageControllableCar;

        private void OnEnable()
        {
            _uiTextCarCounter.SetText($"{LoadTestingController.Instance().CarCount.ToString()} cars");
            
            _uiButtonAddCar.onClick.AddListener(() =>
            {
                LoadTestingController.Instance().AddAiCar();
                _uiTextCarCounter.SetText($"{LoadTestingController.Instance().CarCount.ToString()} cars");
            }); 
            
            _uiButtonRemoveCar.onClick.AddListener(() =>
            {
                LoadTestingController.Instance().RemoveAiCar();
                _uiTextCarCounter.SetText($"{LoadTestingController.Instance().CarCount.ToString()} cars");
            }); 
            
            _uiButtonPrevCar.onClick.AddListener(() =>
            {
                GameManager.Instance.CameraManager.SelectPrevCamera();
                OnCameraChange();
            });
            _uiButtonNextCar.onClick.AddListener(() =>
            {
                GameManager.Instance.CameraManager.SelectPrevCamera();
                OnCameraChange();
            });
        }

        private void OnCameraChange()
        {
            _uiTextCurrentCarName.SetText(GameManager.Instance.CameraManager.CurrentCamName);
            _uiRawImageControllableCar.gameObject.SetActive(GameManager.Instance.CameraManager.CurrentTarget != 0);
        }

        private void OnDisable()
        {
            _uiButtonAddCar.onClick.RemoveAllListeners();
            _uiButtonRemoveCar.onClick.RemoveAllListeners();
            
            _uiButtonPrevCar.onClick.RemoveAllListeners();
            _uiButtonNextCar.onClick.RemoveAllListeners();
        }
    }
}
