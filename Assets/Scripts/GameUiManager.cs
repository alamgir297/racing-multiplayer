using TMPro;
using UnityEngine;

public class GameUiManager : MonoBehaviour {


    [SerializeField] private CarController _playerCar;
    [SerializeField] private TextMeshProUGUI _speedText;
    void Start() {
        UpdateCarSpeed(0);

    }

    private void OnEnable() {
        _playerCar.OnSpeedChange += UpdateCarSpeed;
    }
    private void OnDisable() {
        _playerCar.OnSpeedChange -= UpdateCarSpeed;
    }
    public void UpdateCarSpeed(float speed) {
        _speedText.text = speed.ToString("F2") + " km/h";
    }
}
