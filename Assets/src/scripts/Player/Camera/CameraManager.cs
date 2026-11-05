using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Range(0f, 10f)]
    public float sensitivity = 1f;

    [SerializeField] private Camera playerCamera;

    private float pitch;
    private float minPitch = -90f;
    private float maxPitch = 90f;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        CameraMovement();
    }

    private void CameraMovement()
    {
        float mouseVrt = Input.GetAxisRaw("Mouse Y") * sensitivity * 10f;
        float mouseHrz = Input.GetAxisRaw("Mouse X") * sensitivity * 10f;

        pitch -= mouseVrt;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        transform.Rotate(Vector3.up * mouseHrz);
    }
}