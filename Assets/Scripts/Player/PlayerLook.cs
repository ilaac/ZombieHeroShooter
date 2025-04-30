using UnityEngine;

public class PlayerCameraControl : MonoBehaviour
{
    [Header("Camera Movement")]
    public Transform cameraPosition;

    [Header("Sensitivity Settings")]
    public float MouseSensitivity = 100f;

    [Header("Camera Tilt Settings")]
    public float MaxTilt = 5f;
    public float TiltSpeed = 5f;
    public float TiltResetSpeed = 2f; // Base speed at which the tilt returns to 0
    public float TiltResetSpeedMultiplier = 1f; // Multiplier for the reset speed

    [Header("Logic")]
    public Transform playerBody; // The player's body to rotate when the mouse moves

    private float xRotation = 0f;
    private float currentZRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerBody = transform.parent;

        if (cameraPosition != null)
        {
            transform.position = cameraPosition.position;
        }
    }

    private void Update()
    {
        HandleMouseLook();
        MoveCameraToPosition();
        HandleCameraTilt();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // Z rotation is always 0

        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    private void MoveCameraToPosition()
    {
        if (cameraPosition != null)
        {
            transform.position = cameraPosition.position;
        }
    }

    private void HandleCameraTilt()
    {
        float mouseX = Input.GetAxis("Mouse X");

        // Calculate the target Z rotation based on mouse movement
        float targetZRotation = -mouseX * MaxTilt;

        // If there's movement, apply tilt, otherwise smoothly reset to 0
        if (Mathf.Abs(mouseX) > 0.1f)
        {
            currentZRotation = Mathf.Lerp(currentZRotation, targetZRotation, Time.deltaTime * TiltSpeed);
        }
        else
        {
            // Smoothly reset to 0 using the multiplier
            currentZRotation = Mathf.Lerp(currentZRotation, 0f, Time.deltaTime * TiltResetSpeed * TiltResetSpeedMultiplier);
        }

        // Set the camera's rotation, ensuring Z rotation is always 0
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    public void SetSensitivity(float newSensitivity)
    {
        MouseSensitivity = newSensitivity;
    }

    public void ToggleCursorLock(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }
}