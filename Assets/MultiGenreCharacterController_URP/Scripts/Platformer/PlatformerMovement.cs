using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformerMovement : BaseMovement
{
    //All the inherited variables are in the script called BaseMovement
    //You can add any variables you need :)
    [Header("Double Jump")]
    [SerializeField] private float doubleJumpForce = 2f;

    [Header("Dash Parameters")]
    [SerializeField] private float dashHorizontalForce;
    [SerializeField] private float dashVerticalForce;
    [SerializeField] private float maxDashTime;

    [Header("Crouch")]
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float crouchHeight;
    [SerializeField] private Vector3 crouchCenter;

    [Header("Smooth")]
    [SerializeField] private float dampTime = 0.05f;

    [Header("General")]
    [SerializeField] private float inertia = 2f;
    [SerializeField] private bool constantStrafe = false;

    private PlatformerAnimationHandler animationHandler;
    private bool hadMovement;
    private bool doubleJumping = false;
    private bool hasDoubleJumped = false;
    private bool dashing = false;
    private bool isDashing = false;
    private float currentDashing = 0;

    private bool crouching = false;
    private float originalHeight;
    private Vector3 originalCenter;

    void Start()
    {
        animationHandler = GetComponent<PlatformerAnimationHandler>();
        originalHeight = capsule.GetComponent<CapsuleCollider>().height;
        originalCenter = capsule.GetComponent<CapsuleCollider>().center;
    }

    // Update is called once per frame
    void Update()
    {
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

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if(running == false)
            {
                running = true;
                if (crouching) { crouching = false; playerAnimator.SetBool("Crouch", false); capsule.GetComponent<CapsuleCollider>().height = originalHeight; }
            }
            else
            {
                running = false;
            }
        }
    }

    public override void OnJump(InputAction.CallbackContext context)
    {
        if (IsTouchingTheGround())
        {
            if (context.started)
            {
                jumping = true;
                isFalling = false;
                animationHandler.Jump();
            }
        }

        if (hasJumped == true && isFalling == false && hasDoubleJumped == false)
        {
            if (context.started)
            {
                doubleJumping = true;
                isFalling = false;
                hasDoubleJumped = true;
                animationHandler.DoubleJump();
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
                animationHandler.Jump();
            }
        }

        if (context.performed || context.canceled)
        {
            jumping = false;
            doubleJumping = false;
            coyoteJump = false;
            currentJumpForce = jumpForce;
            hasJumped = true;
            if(doubleJumping)
            {
                currentJumpForce = doubleJumpForce;
            }
        }

    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            dashing = true;
        }

        if(context.canceled || context.performed)
        {
            dashing = false;
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (crouching == false)
            {
                crouching = true;
                running = false;
                capsule.GetComponent<CapsuleCollider>().height = crouchHeight;
                //capsule.GetComponent<CapsuleCollider>().center = crouchCenter;
                playerAnimator.SetBool("Crouch", true);
            }
            else
            {
                crouching = false;
                strafe = false;
                capsule.GetComponent<CapsuleCollider>().height = originalHeight;
                //capsule.GetComponent<CapsuleCollider>().center = originalCenter;
                playerAnimator.SetBool("Crouch", false);
            }
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
                playerAnimator.SetFloat("MovementY", moveInput.y);
            }
            if (moveInput.x != 0)
            {
                movement += moveInput.x * rightCamera;
                hasMovement = true;
                playerAnimator.SetFloat("MovementX", moveInput.x);
            }
        }

        //Movement 
        MovePlayer(movement);
        HandleSpeed();
        playerAnimator.SetFloat("Speed", speed, dampTime, Time.deltaTime);

        //Jumping
        Jump();

        //Dashing
        Dashing();

        if(isDashing)
        {
            currentDashing -= Time.fixedDeltaTime;
            playerRigidbody.AddForce(transform.forward * dashHorizontalForce, ForceMode.Impulse);
        }

        //Strafing
        Strafe(movement);

        //Checking if the player is falling
        CheckFall();

        Grounded();

        //Enemy knockback force
        if (isBouncing)
        {
            currentBouncing -= Time.fixedDeltaTime;
            playerRigidbody.AddForce(transform.forward * knockbackForce, ForceMode.Impulse);
        }

        //Keeping the jump inertia
        if (hadMovement && (hasJumped || jumping))
        {
            playerRigidbody.AddForce(playerRigidbody.transform.forward * inertia, ForceMode.Impulse);
        }

        //Apply gravity
        playerRigidbody.AddForce(Physics.gravity, ForceMode.Force);

        //Updating center of mass
        if (crouching == false)
        {
            capsule.GetComponent<CapsuleCollider>().center = new Vector3(playerRigidbody.centerOfMass.x, originalCenter.y, playerRigidbody.centerOfMass.z);
        }
        else
        {
            capsule.GetComponent<CapsuleCollider>().center = new Vector3(playerRigidbody.centerOfMass.x, crouchCenter.y, playerRigidbody.centerOfMass.z);
        }
    }

    protected override void MovePlayer(Vector3 movement)
    {
        //Checking if the player is on the ground
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
                transform.rotation = Quaternion.Lerp(transform.rotation, lookAtRotation, lerpRotationPct);
            }

            if (strafe == true)
            {
                Quaternion lookAtRotation = Quaternion.LookRotation(new Vector3(camera.transform.forward.x, currentMovement.y, camera.transform.forward.z));
                transform.rotation = Quaternion.Lerp(transform.rotation, lookAtRotation, lerpRotationPct);
            }
        }
    }

    protected override void Jump()
    {
        //Jumping
        if (jumping == true)
        {
            playerRigidbody.AddForce(Vector3.up * currentJumpForce, ForceMode.Impulse);
            playerRigidbody.AddForce(playerRigidbody.transform.forward * currentJumpForce, ForceMode.Impulse);

            if (playerRigidbody.velocity.x != 0 || playerRigidbody.velocity.z != 0)
            {
                playerRigidbody.AddForce(playerRigidbody.transform.forward * currentJumpForce, ForceMode.Impulse);
            }

            if (currentJumpForce > 0)
            {
                currentJumpForce -= jumpDecrement * Time.fixedDeltaTime;
            }
        }
        else if (doubleJumping == true)
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

    private void Dashing()
    {
        if (dashing == true)
        {
            Vector3 dashForce = transform.forward * dashHorizontalForce + transform.up * dashVerticalForce;
            playerRigidbody.AddForce(dashForce, ForceMode.Impulse);
            isDashing = true;
            currentDashing = 2f;
            StartCoroutine(StopDasing(maxDashTime));
        }

    }

    //Check whether the player is grounded or not
    private void Grounded()
    {
        if (IsTouchingTheGround() == true)
        {
            currentTime = 0;
            animationHandler.TouchingGround();
            hadMovement = false;
            hasJumped = false;
            hasDoubleJumped = false;
            isInAir = false;
            currentBuffer = 0;
        }
        else
        {
            isInAir = true;
        }
    }

    override protected void CheckFall()
    {
        playerAnimator.SetBool("OnGround", IsTouchingTheGround());
        if (playerRigidbody.velocity.y < fallDetection && !IsTouchingTheGround())
        {
            isFalling = true;
            crouching = false;
            capsule.GetComponent<CapsuleCollider>().height = originalHeight;
            playerRigidbody.AddForce(Vector3.down * fallForce, ForceMode.Force);
            currentTime += Time.fixedDeltaTime;
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

    protected override void HandleSpeed()
    {
        //Handle movement speed and the "speed" parameter for animations 
        if (hasMovement)
        {
            if (running == true)
            {
                movementSpeed = runSpeed;
                speed = 1f;
            }
            else if(crouching == true)
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
                if (crouching == false)
                {
                    if (((moveInput.x >= 0.5f || moveInput.y >= 0.5f) || (moveInput.x <= -0.5f || moveInput.y <= -0.5f)) && zoom == false)
                    {
                        movementSpeed = runSpeed;
                        speed = 1f;
                    }
                    else
                    {
                        speed = 0.5f;

                    }
                }
                else
                {
                    movementSpeed = crouchSpeed;
                    speed = 0.5f;
                }
            }
        }
        else
        {
            movementSpeed = 0;

            if (speed > 0)
            {
                speed -= 0.05f;
            }
            else
            {
                speed = 0.01f;
                playerAnimator.SetFloat("MovementX", 0);
                playerAnimator.SetFloat("MovementY", 0);
            }
        }
    }

    private IEnumerator StopDasing(float time)
    {
        yield return new WaitForSeconds(time);
        isDashing = false;
    }

    override protected bool IsCloseToTheGround() => Physics.Raycast(feetTransform.position, transform.TransformDirection(Vector3.down), 1.5f, floorMask);
}

