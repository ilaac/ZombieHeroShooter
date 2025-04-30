using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float smooth = 10f;
    [SerializeField] private float multiplier = 1f;

    [Header("Rotation Intensity Settings")]
    [SerializeField] private float xAxisRotationIntensity = 2f;
    [SerializeField] private float zAxisRotationIntensity = 2f;

    [Header("Recovery Speed Settings")]
    [SerializeField] private float xAxisRecoverySpeed = 5f;
    [SerializeField] private float zAxisRecoverySpeed = 5f;

    [Header("Sway Limits")]
    [SerializeField] private float maxHorizontalOffset = 0.1f;
    [SerializeField] private float maxZAxisOffset = 0.1f;
    [SerializeField] private float aimingMaxHorizontalOffset = 0.05f;
    [SerializeField] private float aimingMaxZAxisOffset = 0.05f;

    private Quaternion defaultRotation;
    private float currentXRotation;
    private float currentZRotation;
    private Vector3 defaultPosition;

    private void Start()
    {
        defaultRotation = transform.localRotation;
        defaultPosition = transform.localPosition;
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * multiplier;
        float mouseY = Input.GetAxisRaw("Mouse Y") * multiplier;

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        float targetXAxisRotation = mouseY * xAxisRotationIntensity;
        float targetZAxisRotation = mouseX * zAxisRotationIntensity;

        currentXRotation = Mathf.Lerp(currentXRotation, targetXAxisRotation, xAxisRecoverySpeed * Time.deltaTime);
        currentZRotation = Mathf.Lerp(currentZRotation, targetZAxisRotation, zAxisRecoverySpeed * Time.deltaTime);

        Quaternion targetRotation = rotationX * rotationY;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, defaultRotation * targetRotation, smooth * Time.deltaTime);

        ApplySway();
    }

    private void ApplySway()
    {
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");

        bool isAiming = Input.GetButton("Fire2");
        float maxHorizontal = isAiming ? aimingMaxHorizontalOffset : maxHorizontalOffset;
        float maxZAxis = isAiming ? aimingMaxZAxisOffset : maxZAxisOffset;

        Vector3 targetPosition = defaultPosition;
        targetPosition.x += Mathf.Clamp(horizontalMovement * maxHorizontal, -maxHorizontal, maxHorizontal);
        targetPosition.z += Mathf.Clamp(verticalMovement * maxZAxis, -maxZAxis, maxZAxis);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, smooth * Time.deltaTime);
    }
}
