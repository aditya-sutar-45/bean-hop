using UnityEngine;
using DG.Tweening;

public class PlayerCamera : MonoBehaviour {
    public float XSensitivity;
    public float YSensitivity;

    public Transform orientation;
    public Transform camHolder;

    private float _xRotation;
    private float _yRotation;

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Update() {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * XSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * YSensitivity;

        _yRotation += mouseX;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        camHolder.rotation = Quaternion.Euler(_xRotation, _yRotation, 0f);
        orientation.rotation = Quaternion.Euler(0f, _yRotation, 0f);
    }

    public void ManageFOV(float endValue) {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void ManageTilting(float zTilt) {
        transform.DOLocalRotate(new Vector3(0f, 0f, zTilt), 0.25f);
    }
}
