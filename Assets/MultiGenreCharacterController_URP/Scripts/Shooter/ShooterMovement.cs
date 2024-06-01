using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShooterMovement : BaseMovement
{
    //All the inherited variables are in the script called BaseMovement
    //You can add any variables you need :)

    [Header("General")]
    [SerializeField] private float inertia = 2f;

    [Header("Smoothing Parameters")]
    [SerializeField] private float accelerationValue = 0.05f;
    [SerializeField] private float decelerationValue = 0.05f;
    [SerializeField] private float dampTime = 0.05f;
    [SerializeField] private bool constantStrafe = false;

    [Header("Crouch")]
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float crouchHeight = 1.4f;
    [SerializeField] private float crouchIdleHeight = 1.1f;
    [SerializeField] private Vector3 crouchCenter = new Vector3(0.1f, 0.21f, 0.15f);
    [SerializeField] private Vector3 crouchIdleCenter = new Vector3(0.1f, 0.05f, 0.1f);

    private ShooterAnimationHandler animationHandler;
    private bool crouching = false;
    private bool hadMovement;
    private float originalHeight;
    private Vector3 originalCenter;

    void Start()
    {
        originalHeight = capsule.GetComponent<CapsuleCollider>().height;
        originalCenter = capsule.GetComponent<CapsuleCollider>().center;
        animationHandler = GetComponent<ShooterAnimationHandler>();
    }

    void Update()
    {
        if (zoom == true)
        {
            //Changing the camera's position (if needed)
            cameraPosition.position = Vector3.Lerp(cameraPosition.position, zoomPosition.position, Time.deltaTime * 10);
        }
        if (zoom == false)
        {
            cameraPosition.position = Vector3.Lerp(cameraPosition.position, cameraPositionStart.transform.position, Time.deltaTime * 10);
        }

        if (constantStrafe) { strafe = true; }

    }

    //Unity Event for moving
    public override void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (context.canceled)
        {
            hadMovement = true;
        }
    }

    //Unity Event for running
    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            running = true;
            if(crouching) { crouching = false; playerAnimator.SetBool("Crouch", false); capsule.GetComponent<CapsuleCollider>().height = originalHeight; }
        }
        if (context.canceled)
        {
            running = false;
        }
    }

    //Unity Event for crouching
    public void OnCrouch(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            if(crouching == false)
            {
                crouching = true;
                strafe = true;
                capsule.GetComponent<CapsuleCollider>().height = crouchHeight;
                playerAnimator.SetBool("Crouch", true);
            }
            else
            {
                crouching = false;
                strafe = false;
                capsule.GetComponent<CapsuleCollider>().height = originalHeight;
                capsule.GetComponent<CapsuleCollider>().center = originalCenter;
                playerAnimator.SetBool("Crouch", false);
            }
        }
    }

    //Unity Event for jumping
    public override void OnJump(InputAction.CallbackContext context)
    {
        if (IsTouchingTheGround() && crouching == false)
        {
            if (context.started)
            {
                jumping = true;
                isFalling = false;
                hasJumped = true;
                animationHandler.Jump();
            }
        }

        if (isFalling)
        {
            if (context.started)
                jumpPressed = true;
        }

        if (context.performed || context.canceled)
        {
            jumping = false;
            coyoteJump = false;
            currentJumpForce = jumpForce;
            hasJumped = true;
        }

    }

    //Unity Event for zooming or aiming
    public void OnZoom(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            strafe = true;
            zoom = true;            
            playerAnimator.SetBool("Aim", true);
        }

        if (context.canceled)
        {
            strafe = false;
            zoom = false;
            playerAnimator.SetBool("Aim", false);
        }
    }

    override protected void FixedUpdate()
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
                playerAnimator.SetFloat("MovementY", moveInput.y, accelerationValue, Time.deltaTime);
            }

            if (moveInput.x != 0)
            {
                movement += moveInput.x * rightCamera;
                hasMovement = true;
                playerAnimator.SetFloat("MovementX", moveInput.x, accelerationValue, Time.deltaTime);
            }
        }

        //Moving
        MovePlayer(movement);
        HandleSpeed();
        playerAnimator.SetFloat("Speed", speed, dampTime, Time.deltaTime);

        //Jumping
        Jump();

        //Strafing
        Strafe(movement);

        //Camera zoom
        CameraZoom();

        //Checking if the player is falling
        CheckFall();

        Grounded();

        //Enemy knockback force
        if (isBouncing)
        {
            currentBouncing -= Time.deltaTime;
            playerRigidbody.AddForce(-capsule.transform.forward * knockbackForce, ForceMode.Impulse);
        }

        //Keeping the jump inertia
        if (hadMovement && (hasJumped || jumping))
        {
            playerRigidbody.AddForce(capsule.transform.forward * inertia, ForceMode.Impulse);
        }

        //Apply gravity
        playerRigidbody.AddForce(Physics.gravity, ForceMode.Force);

        //Updating the height and center of the collider
        if(crouching == true)
        {
            if (hasMovement == false)
            {
                capsule.GetComponent<CapsuleCollider>().height = crouchIdleHeight;
                capsule.GetComponent<CapsuleCollider>().center = crouchIdleCenter;
            }
            else
            {
                capsule.GetComponent<CapsuleCollider>().height = crouchHeight;
                capsule.GetComponent<CapsuleCollider>().center = crouchCenter;
            }
        }
    }

    protected override void MovePlayer(Vector3 movement)
    {        
        //Checking if the player is on the ground and moving it
        if (IsTouchingDifferentGround() == false || IsTouchingTheGround())
        {
            movement += movement * movementSpeed;
            playerRigidbody.velocity = new Vector3(movement.x, playerRigidbody.velocity.y, movement.z);
        }
    }

    protected override void Strafe(Vector3 currentMovement)
    {
        if (strafe == true && hasMovement == false)
        {
            Quaternion lookAtRotation = Quaternion.LookRotation(new Vector3(camera.transform.forward.x, currentMovement.y, camera.transform.forward.z));
            capsule.rotation = Quaternion.Lerp(capsule.rotation, lookAtRotation, lerpRotationPct);
        }

        if (hasMovement)
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

    protected override void CheckFall()
    {
        playerAnimator.SetBool("OnGround", IsTouchingTheGround());
        if (playerRigidbody.velocity.y < fallDetection && !IsTouchingTheGround())
        {
            crouching = false;
            capsule.GetComponent<CapsuleCollider>().height = originalHeight;
            isFalling = true;
            playerRigidbody.AddForce(Vector3.down * fallForce, ForceMode.Force);
            currentTime += Time.deltaTime;
            animationHandler.Falling();

            if (IsCloseToTheGround())
            {
                playerAnimator.SetBool("CloseToGround", true);
            }
            else
            {
                playerAnimator.SetBool("CloseToGround", false);
            }
        }
        else
        {
            isFalling = false;
            isInAir = false;
            playerAnimator.SetBool("Falling", false);
        }
    }

    //Check whether the player is grounded or not
    private void Grounded()
    {
        if (IsTouchingTheGround())
        {
            currentTime = 0;
            animationHandler.TouchingGround();
            hadMovement = false;
            hasJumped = false;
            isInAir = false;
            currentBuffer = 0;
        }
        else
        {
            isInAir = true;
        }
    }

    protected override void HandleSpeed()
    {
        //Handle movement speed and the "speed" parameter for animations 
        if (hasMovement)
        {
            if (running == true && zoom == false)
            {
                movementSpeed = runSpeed;
                speed = 1f;
            }
            else if (zoom == true)
            {
                movementSpeed = zoomSpeed;
                speed = 0.5f;
            }
            else if (crouching == true)
            {
                movementSpeed = crouchSpeed;
                speed = 0.5f;
            }
            else
            {
                movementSpeed = walkSpeed;
                speed = 0.5f;
            }

            //Speed for gamepad
            if (playerInputMng.GetGamepadActive() == true)
            {
                if (((moveInput.x >= 0.5f || moveInput.y >= 0.5f) || (moveInput.x <= -0.5f || moveInput.y <= -0.5f)) && zoom == false && crouching == false)
                {
                    movementSpeed = runSpeed;
                    speed = 1f;
                }
                else
                {
                    speed = 0.5f;
                }

                if (crouching)
                {
                    movementSpeed = crouchSpeed;
                }
            }
        }
        else
        {
            //movementSpeed = 0;
            movementSpeed = Mathf.Lerp(movementSpeed, 0, decelerationValue);

            if (speed > 0)
            {
                speed -= 0.05f;
            }
            else
            {
                speed = 0.01f;
                playerAnimator.SetFloat("MovementX", 0.01f, decelerationValue, Time.fixedDeltaTime);
                playerAnimator.SetFloat("MovementY", 0.01f, decelerationValue, Time.fixedDeltaTime);
            }
        }
    }

    protected override void CameraZoom()
    {
        //Camera zoom and crouching
        if (zoom == true || crouching == true)
        {
            camera.fieldOfView -= 5;
            if (camera.fieldOfView <= cameraZoomFOV)
            {
                camera.fieldOfView = cameraZoomFOV;
            }

            if (moveInput.x >= 0.5f || moveInput.x <= -0.5f)
            {
                playerAnimator.SetBool("SideMovement", true);
            }
            else
            {
                playerAnimator.SetBool("SideMovement", false);
                playerAnimator.SetFloat("MovementX", 0.01f, dampTime, Time.deltaTime);
            }

            if (moveInput.y >= 0.5f || moveInput.y <= -0.5f)
            {
                playerAnimator.SetBool("Backwards", true);
            }
            else
            {
                playerAnimator.SetBool("Backwards", false);
                playerAnimator.SetFloat("MovementY", 0.01f, dampTime, Time.deltaTime);
            }
        }

        if (zoom == false)
        {
            camera.fieldOfView += 5;
            if (camera.fieldOfView >= cameraDefaultFOV)
            {
                camera.fieldOfView = cameraDefaultFOV;
            }
        }
    }

    protected override void Jump()
    {
        base.Jump();
    }

    override protected bool IsCloseToTheGround() => Physics.Raycast(feetTransform.position, transform.TransformDirection(Vector3.down), 1.5f, floorMask);
}
