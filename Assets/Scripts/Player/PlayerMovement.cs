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
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air,
        aiming,
        aimWalking
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        rb.freezeRotation = true;

        readyToJump = true;
        currentHeight = standingHeight;

        if (playerCamera != null)
        {
            defaultFOV = playerCamera.fieldOfView;
            targetFOV = defaultFOV;
        
            // Ensure camera is at the correct local position initially
            Vector3 cameraInitialPosition = playerCamera.transform.localPosition;
            cameraInitialPosition.y = standingHeight / 2f; // Set camera at half the player's height
            playerCamera.transform.localPosition = cameraInitialPosition;
        }
        else
        {
            Debug.LogError("Player Camera not assigned! Assign the camera in the inspector.");
        }

        // Ensure the orientation is correctly positioned at the start
        if (orientation != null)
        {
            orientation.localPosition = Vector3.zero; // Position orientation correctly
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
    }


    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Crouch (toggle or hold depending on the boolean flag)
        if (isCrouchToggleEnabled)
        {
            if (Input.GetKeyDown(crouchKey))
            {
                isCrouching = !isCrouching; // Toggle crouch state when the key is pressed
            }
        }
        else
        {
            if (Input.GetKeyDown(crouchKey))
            {
                isCrouching = true; // Start crouching
                anim.SetBool("IsCrouching", true);
            }
            else if (Input.GetKeyUp(crouchKey))
            {
                isCrouching = false; // Stop crouching
                anim.SetBool("IsCrouching", false);
            }
        }

        // Sprint (toggle or hold depending on the boolean flag)
        if (isSprintToggleEnabled)
        {
            if (Input.GetKeyUp(sprintKey))
            {
                state = MovementState.walking;
                moveSpeed = walkSpeed;
                targetFOV = defaultFOV;
            }
        }
        else
        {
            {
                state = MovementState.walking;
                moveSpeed = walkSpeed;
                targetFOV = defaultFOV;
            }
        }
    }

    private void StateHandler()
    {
        if (grounded)
        {
            if (isCrouching)
            {
                state = MovementState.crouching;
                moveSpeed = crouchSpeed;
                targetFOV = defaultFOV + crouchFOVChange;
            }
            else if (horizontalInput == 0 && verticalInput == 0)
            {
                state = MovementState.walking;
                moveSpeed = 0f;
                targetFOV = defaultFOV;
            }
            else
            {
                state = MovementState.walking;
                moveSpeed = walkSpeed;
                targetFOV = defaultFOV;
            }
        }
        else
        {
            state = MovementState.air;
            targetFOV = defaultFOV;
        }
    }

    private void MovePlayer()
    {
        // Use the orientation's forward and right directions for movement
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
        {
            if (horizontalInput == 0 && verticalInput == 0)
            {
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f); // Stop movement instantly
                rb.drag = groundDrag;
            }
            else
            {
                rb.drag = 0;
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
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

        // Adjust the player's body height (capsule) but not the orientation
        Vector3 playerScale = transform.GetChild(0).localScale;
        playerScale.y = currentHeight;
        transform.GetChild(0).localScale = playerScale;

        // Adjust the camera's position during crouch/stand
        Vector3 cameraPosition = playerCamera.transform.localPosition;
        cameraPosition.y = currentHeight / 2f; // Keep camera at half the height
        playerCamera.transform.localPosition = cameraPosition;
        
        if (orientation != null)
        {
            // Fix the orientation position to stay at the correct height
            orientation.localPosition = Vector3.zero;
        }
    }
    
    private void HandleAnimations()
    {
        //walking animation logic
        if (moveDirection == Vector3.zero && !isCrouching)
        {
            anim.SetFloat("Speed", .025f, 0.3f, Time.deltaTime);
        }
        else if (moveDirection != Vector3.zero && !Input.GetKey(KeyCode.LeftShift) && !isCrouching)
        {
            anim.SetFloat("Speed", 0.5f, 0.1f, Time.deltaTime);
        }
        else if (moveDirection == Vector3.zero && isCrouching)
        {
            anim.SetFloat("Speed", 0.25f, 0.3f, Time.deltaTime);
        }
        else if (moveDirection != Vector3.zero && isCrouching)
        {
            anim.SetFloat("Speed", 0.95f, 0.3f, Time.deltaTime);
        }
    }
}
