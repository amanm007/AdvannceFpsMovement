using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]// will just create a header in the inspector panel
    private float moveSpeed;
    public float walkSpeed;
    public float sprindSpeed;



    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatisGround;
    bool grounded;
    public float groundDrag;

    [Header("Slope Handling")]
    public float maxSlpoeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;



    public float jumpForce;
    public float jumpCoolDown;
    public float airMultiplier;
    bool readyToJump=true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYscale;
    private float startYscale;


    [Header("KeyBinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;




    public Transform pos;

    float horInput;
    float verInput;

    Vector3 moveDir;
    Rigidbody rb;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    public MovementState state;









    // Start is called before the first frame update
    void Start()
    {
        rb=GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        startYscale = transform.localScale.y;


    }
    private void MyInput()
    {
        horInput = Input.GetAxisRaw("Horizontal");
        verInput = Input.GetAxisRaw("Vertical");

        //jump
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump=false;

            Jump();
           Invoke(nameof(ResetJump), jumpCoolDown);

        }
        //start crouch
        if(Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYscale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);


        }
        // 
        if(Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYscale, transform.localScale.z);
        }
        
    }
    private void FixedUpdate() //runs according to the frequency of the physics system
    {
        MovePlayer();

    }

    // Update is called once per frame
    void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f,whatisGround);// (player position, a vector 3 down of player height to check against ground layer
        //this will reture true or falase


        MyInput();
        SpeedControl();
        StateHandler();


        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
            rb.drag = 0;

    }
    private void StateHandler()
    {

        if(Input.GetKeyDown(crouchKey)&& grounded)
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;


        }
        else if(grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprindSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;

            
        }

        else
        {
            state = MovementState.air;

        }
    }
    private void MovePlayer()
    {
        moveDir = pos.forward * verInput + pos.right * horInput;

        if(OnSlope()&& !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            if(rb.velocity.y>0)
            {
                rb.AddForce(Vector3.down * 50f, ForceMode.Force);

            }
        }
        if(grounded)
        rb.AddForce(moveDir.normalized * moveSpeed*10f , ForceMode.Force); //or can multiply with 10f for more speed

        //
        else if(!grounded)
        {
            rb.AddForce(moveDir.normalized * moveSpeed * 10f*airMultiplier, ForceMode.Force);
        }

        rb.useGravity = !OnSlope();

    }
    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;

        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // flat VEL is vector craeted by x and z axis NOT Y

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

        //reset y velocity first

        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse); //transform would be player values since its small t


    }
    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;


    }
    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight*0.5f+0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal); //how steep is the slope
            return angle < maxSlpoeAngle && angle != 0; //if angle less than max slope angle and is not zero

        }
        return false;

    }
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;

    }
}
