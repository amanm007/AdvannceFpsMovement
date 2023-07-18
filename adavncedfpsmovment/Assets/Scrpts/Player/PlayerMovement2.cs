using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement2 : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float airMultiplier;
    public float wallRunSpeed;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    private bool grounded;
    public float groundDrag;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    private bool readyToJump = true;

    [Header("Crouching")]
    public float crouchYScale;
    private float startYScale;

    [Header("Wall Running")]
    public float wallRunDuration;
    public float wallRunGravity;
    private bool isWallRunning;
    private Vector3 wallRunDirection;
    private float wallRunTimer;

    [Header("KeyBinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    public Transform pos;

    private float horInput;
    private float verInput;
    private Vector3 moveDir;
    private Rigidbody rb;

    private enum MovementState
    {
        Walking,
        Sprinting,
        Crouching,
        Air
    }
    private MovementState state;

    private Jetpack jetpack;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y;

        jetpack = GetComponent<Jetpack>();
    }

    private void MyInput()
    {
        horInput = Input.GetAxisRaw("Horizontal");
        verInput = Input.GetAxisRaw("Vertical");

        // Jump
        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Crouch
        if (Input.GetKeyDown(crouchKey))
        {
            Crouch();
        }

        if (Input.GetKeyUp(crouchKey))
        {
            ResetCrouch();
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
        SpeedControl();
        StateHandler();
        if (isWallRunning)
        {
            WallRun();
        }
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();

        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0f;
        }
    }

    private void StateHandler()
    {
        if (Input.GetKey(sprintKey) && grounded && !jetpack.IsJetpackActive())
        {
            state = MovementState.Sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (Input.GetKey(crouchKey) && grounded && !jetpack.IsJetpackActive())
        {
            state = MovementState.Crouching;
            moveSpeed = crouchSpeed;
        }
        else if (grounded)
        {
            state = MovementState.Walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.Air;
        }
    }

    private void MovePlayer()
    {
        moveDir = pos.forward * verInput + pos.right * horInput;
        moveDir.Normalize();

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0f)
            {
                rb.AddForce(Vector3.down * 50f, ForceMode.Force);
            }
        }
        else if (isWallRunning)
        {
            rb.velocity = wallRunDirection * wallRunSpeed;
        }
        else if (grounded)
        {
            rb.AddForce(moveDir * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDir * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        rb.useGravity = !OnSlope() && !isWallRunning;
    }


    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else if (!grounded && !isWallRunning)
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }
    private void Jump()
    {
        exitingSlope = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private void Crouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    private void ResetCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0f;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }
    private void WallRun()
    {
        wallRunTimer -= Time.deltaTime;

        if (wallRunTimer <= 0f)
        {
            isWallRunning = false;
            rb.useGravity = !OnSlope();
            return;
        }

        rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Acceleration);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!isWallRunning && collision.collider.CompareTag("Wall") && !grounded && !OnSlope())
        {
            isWallRunning = true;
            wallRunDirection = Vector3.Cross(collision.contacts[0].normal, Vector3.up);
            wallRunTimer = wallRunDuration;
            rb.useGravity = false;
        }
    }
}
