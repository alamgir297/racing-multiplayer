using UnityEngine;

public class MinimapFollow : MonoBehaviour {

    [SerializeField] private Transform _target;
    [SerializeField] private float _height;

    private void LateUpdate() {
        Vector3 newPos = _target.position;
        newPos.y += _height;
        transform.position = newPos;

        Quaternion targetRotation = Quaternion.Euler(90f, _target.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

    }
}
