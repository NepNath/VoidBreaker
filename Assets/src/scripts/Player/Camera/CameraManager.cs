using System;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    [Range(0f, 10f)]
    public float sensitivity = 1f;

    private float yaw;
    private float pitch;
    private float minPitch = -90f;
    private float maxPitch = 90f;
    
    [SerializeField] private Camera playerCamera;


    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        cameraMovement();
    }
    
    void cameraMovement()
    {
        float mouseVrt = Input.GetAxis("Mouse Y") * sensitivity * 10f;
        float mouseHrz = Input.GetAxis("Mouse X") * sensitivity * 10f;
        
        yaw += mouseHrz;
        pitch -= mouseVrt; 

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        BaseMovements.rb.transform.Rotate(Vector3.up * mouseHrz);

    }
}
