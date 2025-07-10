using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float adsWalkSpeed;
    public float groundDrag;

    private Vector3 moveDirection = Vector3.zero;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchHeight;
    public float standingHeight;
    public float crouchTransitionSpeed;

    [Header("FOV Settings")]
    public Camera playerCamera;
    public float sprintFOVChange;
    public float crouchFOVChange;
    public float fovTransitionSpeed;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public bool isCrouchToggleEnabled = false;
    public bool isSprintToggleEnabled = false;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Logic")]
    public Transform orientation;
    public MovementState state;

    [Header("Animation")]
    public Animator anim;

    float horizontalInput;
    float verticalInput;
    Rigidbody rb;

    private float defaultFOV;
    private float targetFOV;

    private float currentHeight;
    private bool isCrouching;

    [Header("References")]
    [SerializeField] private WeaponSway weaponSwayAndBob;
    public Gun gun;

    public enum MovementState
    {
        Idle,
        Walking,
        Crouching,
        Air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (anim == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }
        else
        {
            Debug.Log("Animator component found: " + anim.name);
        }

        readyToJump = true;
        currentHeight = standingHeight;

        if (playerCamera != null)
        {
            defaultFOV = playerCamera.fieldOfView;
            targetFOV = defaultFOV;

            Vector3 cameraInitialPosition = playerCamera.transform.localPosition;
            cameraInitialPosition.y = standingHeight / 2f;
            playerCamera.transform.localPosition = cameraInitialPosition;
        }
        else
        {
            Debug.LogError("Player Camera not assigned! Assign the camera in the inspector.");
        }

        if (orientation != null)
        {
            orientation.localPosition = Vector3.zero;
        }
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
        }

        UpdateCrouch();
        HandleAnimations();
    }

    public void SetAnimator(Animator newAnimator)
    {
        anim = newAnimator;
        if (anim == null)
        {
            Debug.LogError("New Animator assigned is null!");
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (isCrouchToggleEnabled)
        {
            if (Input.GetKeyDown(crouchKey))
            {
                isCrouching = !isCrouching;
            }
        }
        else
        {
            if (Input.GetKeyDown(crouchKey))
            {
                isCrouching = true;
                anim.SetBool("IsCrouching", true);
            }
            else if (Input.GetKeyUp(crouchKey))
            {
                isCrouching = false;
                anim.SetBool("IsCrouching", false);
            }
        }
    }

    private void StateHandler()
    {
        if (!grounded)
        {
            state = MovementState.Air;
            targetFOV = defaultFOV;
        }
        else if (isCrouching)
        {
            state = MovementState.Crouching;
            moveSpeed = crouchSpeed;
            targetFOV = defaultFOV + crouchFOVChange;
        }
        else if (horizontalInput == 0 && verticalInput == 0)
        {
            state = MovementState.Idle;
            moveSpeed = 0f;
            targetFOV = defaultFOV;
        }
        else
        {
            state = MovementState.Walking;
            moveSpeed = walkSpeed;
            targetFOV = defaultFOV;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
        {
            if (horizontalInput == 0 && verticalInput == 0)
            {
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
                rb.drag = groundDrag;
            }
            else
            {
                rb.drag = 0;
                Vector3 force = moveDirection.normalized * moveSpeed * 10f;
                rb.AddForce(force, ForceMode.Force);
            }
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
            rb.drag = 1f;
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void UpdateCrouch()
    {
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        Vector3 playerScale = transform.GetChild(0).localScale;
        playerScale.y = currentHeight;
        transform.GetChild(0).localScale = playerScale;

        Vector3 cameraPosition = playerCamera.transform.localPosition;
        cameraPosition.y = currentHeight / 2f;
        playerCamera.transform.localPosition = cameraPosition;

        if (orientation != null)
        {
            orientation.localPosition = Vector3.zero;
        }
    }

    private void HandleAnimations()
    {
        if (anim == null)
        {
            //Debug.LogError("Animator is null in HandleAnimations!");
            return;
        }

        if (gun != null && gun.anim != null)
        {
            gun.anim.SetBool("IsWalking", state == MovementState.Walking);
        }

        // Set Speed without damping to ensure immediate update
        float targetSpeed = 0f;
        switch (state)
        {
            case MovementState.Idle:
                targetSpeed = 0f;
                break;
            case MovementState.Walking:
                targetSpeed = 1f;
                break;
            case MovementState.Crouching:
                targetSpeed = moveDirection != Vector3.zero ? 0.95f : 0f;
                break;
            case MovementState.Air:
                targetSpeed = 0f;
                break;
        }
        anim.SetFloat("Speed", targetSpeed);
    }
}