using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.InputSystem.Utilities;


public class BaseMovement : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] protected Camera camera;
    [SerializeField] protected float lerpRotationPct = 0.1f;
    [SerializeField] protected Transform feetTransform;
    [SerializeField] protected Transform zoomPosition;
    [SerializeField] protected Transform cameraPositionStart;
    [SerializeField] protected Transform cameraPosition;
    [SerializeField] protected float cameraDefaultFOV = 60f;
    [SerializeField] protected float cameraZoomFOV = 40f;

    [Header("Animator")]
    [SerializeField] protected Animator playerAnimator;

    [Header("Movement")]
    [SerializeField] protected LayerMask floorMask;
    [SerializeField] protected LayerMask notFloorMask;
    [SerializeField] protected float walkSpeed = 3f;
    [SerializeField] protected float runSpeed = 6f;
    [SerializeField] protected float zoomSpeed = 2f;
    [SerializeField] protected Transform capsule;

    [Header("Jump")]
    [SerializeField] protected float jumpForce = 0.4f;
    [HideInInspector] protected float currentJumpForce;
    [SerializeField] protected float jumpDecrement = 0.1f;
    [Range(-2f, -0.0f)]
    [SerializeField] protected float fallDetection = -1.0f;
    [SerializeField] protected float fallForce = 10f;
    [SerializeField] protected float coyoteTime = 0.2f;
    [SerializeField] protected float jumpBuffer = 0.2f;

    [Header("Enemies")]
    [SerializeField] protected float knockbackForce = 3f;
    [SerializeField] protected float enableMoveTime = 0.5f;

    //Private Variables
    protected Rigidbody playerRigidbody;
    protected PlayerInputManager playerInputMng;
    protected Vector2 moveInput;
    protected bool hasMovement;
    protected bool canMove = true;
    protected bool isBouncing = false;
    protected float currentBouncing = 0;
    protected float currentTime = 0f;
    protected bool isFalling = false;
    protected bool jumping = false;
    protected bool coyoteJump = false;
    protected bool hasJumped = false;
    protected bool isInAir = false;
    protected bool jumpPressed = false;
    protected bool buffedJump = false;
    protected float currentBuffer = 0;
    protected bool strafe = false;
    protected bool zoom = false;
    protected bool running = false;
    protected float movementSpeed = 0.0f;
    protected float speed = 0.0f;
    protected float weight = 0;
    protected float detectionDistance = 1f;


    void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerInputMng = GetComponent<PlayerInputManager>();
        currentJumpForce = jumpForce;
    }

    // Start is called before the first frame update
    void Start()
    {
        //currentJumpForce = jumpForce;
    }

    virtual protected void Update()
    {

    }

    virtual public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    virtual public void OnJump(InputAction.CallbackContext context)
    {
        if (IsTouchingTheGround())
        {
            if (context.started)
            {
                jumping = true;
                isFalling = false;
                hasJumped = true;
            }
        }

        if (isFalling == true && currentTime <= coyoteTime && hasJumped == false)
        {
            if (context.started)
            {
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
                jumping = true;
                coyoteJump = true;
                isFalling = false;
            }
        }

        if (context.performed || context.canceled)
        {
            jumping = false;
            coyoteJump = false;
            currentJumpForce = jumpForce;
            hasJumped = true;
        }

    }
  
    virtual protected void FixedUpdate()
    {
        hasMovement = false;

        Vector3 forwardCamera = camera.transform.forward;
        Vector3 rightCamera = camera.transform.right;

        forwardCamera.y = 0.0f;
        rightCamera.y = 0.0f;

        forwardCamera.Normalize();
        rightCamera.Normalize();

        Vector3 movement = Vector3.zero;

        //Registering input
        if (canMove)
        {
            if (moveInput.y != 0)
            {
                movement += moveInput.y * forwardCamera;
                hasMovement = true;
            }
            if (moveInput.x != 0)
            {
                movement += moveInput.x * rightCamera;
                hasMovement = true;
            }
        }

        //Movement 
        MovePlayer(movement);
        HandleSpeed();

        //Jumping
        Jump();

        //Strafing
        Strafe(movement);

        //Camera zoom
        CameraZoom();

        //Checking if the player is falling
        CheckFall();

        if (IsTouchingTheGround())
        {
            currentTime = 0;
            hasJumped = false;
            isInAir = false;
            currentBuffer = 0;
        }
        else
        {
            isInAir = true;
        }

        //Enemy knockback
        if (isBouncing)
        {
            currentBouncing -= Time.deltaTime;
            playerRigidbody.AddForce(-capsule.transform.forward * knockbackForce, ForceMode.Impulse);
        }

        //Applying gravity
        playerRigidbody.AddForce(Physics.gravity, ForceMode.Force);
    }

    virtual protected void MovePlayer(Vector3 movement)
    {
        //Moving the player
        if (IsTouchingDifferentGround() == false || IsTouchingTheGround())
        {
            movement += movement * movementSpeed;
            playerRigidbody.velocity = new Vector3(movement.x, playerRigidbody.velocity.y, movement.z);            

        }
    }

    virtual protected void Strafe(Vector3 currentMovement)
    {
        if (strafe == true && hasMovement == false)
        {
            Quaternion lookAtRotation = Quaternion.LookRotation(new Vector3(camera.transform.forward.x, currentMovement.y, camera.transform.forward.z));
            capsule.rotation = Quaternion.Lerp(capsule.rotation, lookAtRotation, lerpRotationPct);
        }

        if(hasMovement)
        {
            //Strafing
            if (strafe == false)
            {
                Quaternion lookAtRotation = Quaternion.LookRotation(currentMovement);
                capsule.rotation = Quaternion.Lerp(capsule.rotation, lookAtRotation, lerpRotationPct);
            }

            if (strafe == true)
            {
                Quaternion lookAtRotation = Quaternion.LookRotation(new Vector3(camera.transform.forward.x, currentMovement.y, camera.transform.forward.z));
                capsule.rotation = Quaternion.Lerp(capsule.rotation, lookAtRotation, lerpRotationPct);
            }
        }
    }

    //Check falling
    virtual protected void CheckFall()
    {
        if (playerRigidbody.velocity.y < fallDetection && !IsTouchingTheGround())
        {
            isFalling = true;
            playerRigidbody.AddForce(Vector3.down * fallForce, ForceMode.Force);
            currentTime += Time.deltaTime;
        }
        else
        {
            isFalling = false;
            isInAir = false;
        }
    }

    virtual protected void HandleSpeed()
    {
        //Movement 
        if (hasMovement)
        {
            if (running == true && zoom == false)
            {
                movementSpeed = runSpeed;
            }
            else if (zoom == true)
            {
                movementSpeed = zoomSpeed;

            }
            else
            {
                movementSpeed = walkSpeed;
            }

            //Speed for gamepad
            if (playerInputMng.GetGamepadActive() == true)
            {

                if (((moveInput.x >= 0.5f || moveInput.y >= 0.5f) || (moveInput.x <= -0.5f || moveInput.y <= -0.5f)) && zoom == false)
                {
                    movementSpeed = runSpeed;

                }
            }

        }
        else
        {
            movementSpeed = 0;
        }
    }

    virtual protected void CameraZoom()
    {

    }

    virtual protected void Jump()
    {
        //Jumping
        if (jumping == true)
        {
            playerRigidbody.AddForce(Vector3.up * currentJumpForce, ForceMode.Impulse);
            if (currentJumpForce > 0)
            {
                currentJumpForce -= jumpDecrement * Time.fixedDeltaTime;
            }
        }

        //Jump buffer
        if (isFalling && jumpPressed)
        {
            currentBuffer += Time.fixedDeltaTime;
            if (currentBuffer <= jumpBuffer)
            {
                buffedJump = true;
            }
        }

        if (buffedJump)
        {
            if (IsTouchingTheGround())
            {
                playerRigidbody.AddForce(Vector3.up * 3f, ForceMode.Impulse);
                jumpPressed = false;
                StartCoroutine(BuffedJumpDisable(0.2f));
            }
        }
    }

    virtual public void CollidedWithEnemy()
    {
        playerRigidbody.AddForce(transform.up * knockbackForce, ForceMode.Impulse);
        isBouncing = true;
        currentBouncing = 2f;
        canMove = false;
        StartCoroutine(EnableMovement(2f));
    }

    virtual protected bool IsTouchingTheGround() => Physics.CheckSphere(feetTransform.position, 0.25f, floorMask);
    virtual protected bool IsCloseToTheGround() => Physics.CheckSphere(feetTransform.position, 1.5f, floorMask);
    protected bool IsTouchingDifferentGround() => Physics.CheckSphere(feetTransform.position, 0.25f, notFloorMask);

    //Getters
    public bool GetStrafe()
    {
        return strafe;
    }

    public bool GetZoom()
    {
        return zoom;
    }

    public void SetMove(bool move)
    {
        canMove = move;
    }

    protected IEnumerator EnableMovement(float time)
    {
        yield return new WaitForSeconds(time);
        canMove = true;
        isBouncing = false;
    }

    protected IEnumerator BuffedJumpDisable(float time)
    {
        yield return new WaitForSeconds(time);
        buffedJump = false;
    }
}
