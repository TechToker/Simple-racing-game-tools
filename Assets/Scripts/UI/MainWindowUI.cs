using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace RacingGame.UI
{
    public class MainWindowUI : MonoBehaviour
    {
        [SerializeField] private RawImage _uiRawImageControllableCar;

        [SerializeField] private TextMeshProUGUI _uiTextCurrentCarName;
        [SerializeField] private Button _uiButtonPrevCar;
        [SerializeField] private Button _uiButtonNextCar;

        private void OnEnable()
        {
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
            _uiButtonPrevCar.onClick.RemoveAllListeners();
            _uiButtonNextCar.onClick.RemoveAllListeners();
        }
    }
}
