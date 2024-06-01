using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class AdventureMovement : BaseMovement
{
    //All the inherited variables are in the script called BaseMovement
    //You can add any variables you need :)
    [Header("Climbing Parameters")]
    [SerializeField] private float climbingSpeed = 2f;

    [Header("Gliding")]
    [SerializeField] private float glideSpeed;
    [SerializeField] private float glideDrag;

    [Header("General")]
    [SerializeField] private float dampTime = 0.05f;
    [SerializeField] private float inertia = 2f;

    private bool hadMovement;
    private AdventureAnimationHandler animationHandler;
    private GameObject glider;
    private float initialDrag;
    private bool isGliding;

    private AdventureClimbing adventureClimbing;
    private bool wallJumping = false;
    private bool hasClimbingMovement = false;


    void Start()
    {
        adventureClimbing = GetComponent<AdventureClimbing>();
        animationHandler = GetComponent<AdventureAnimationHandler>();
        initialDrag = playerRigidbody.drag;
        glider = GameObject.FindGameObjectWithTag("Glider");
        glider.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Unity Event for moving
    public override void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if(context.canceled)
        {
            hadMovement = true;
        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (running == false)
            {
                running = true;
            }
            else
            {
                running = false;
            }
        }
    }

    public override void OnJump(InputAction.CallbackContext context)
    {
        if (IsTouchingTheGround() && adventureClimbing.IsClimbing() == false)
        {
            if (context.started)
            {
                jumping = true;
                isFalling = false;
                hasJumped = true;
                animationHandler.Jump();
            }

        }

        if(adventureClimbing.IsClimbing() == true)
        {
            if (context.started)
            {
                wallJumping = true;
                isFalling = false;
                animationHandler.WallJump();
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

    public void OnGlide(InputAction.CallbackContext context)
    {
        if (isInAir && adventureClimbing.IsClimbing() == false)
        {
            if (context.performed)
            {
                isGliding = true;
                jumping = false;
                coyoteJump = false;
                isInAir = true;
                isFalling = false;
                animationHandler.Glide();
                glider.SetActive(true);
            }

            if (context.canceled)
            {
                isGliding = false;
                isFalling = true;
                playerAnimator.SetBool("Gliding", false);
                glider.SetActive(false);
            }          
        }

        if (adventureClimbing.IsClimbing() == true)
        {
            isGliding = false;
            isInAir = false;
            hasJumped = false;
            jumpPressed = false;
            isFalling = false;
            playerAnimator.SetBool("Gliding", false);
            glider.SetActive(false);
        }
    }

    override protected void FixedUpdate()
    {
        hasMovement = false;
        hasClimbingMovement = false;

        Vector3 forwardCamera = camera.transform.forward;
        Vector3 rightCamera = camera.transform.right;

        forwardCamera.y = 0.0f;
        rightCamera.y = 0.0f;

        forwardCamera.Normalize();
        rightCamera.Normalize();

        Vector3 movement = Vector3.zero;
        Vector3 climbingMovement = Vector3.zero;

        //Registering input
        if (canMove && adventureClimbing.IsClimbing() == false && adventureClimbing.ClimbToTop() == false)
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

        //Climbing Input
        else if(canMove && adventureClimbing.IsClimbing() == true && adventureClimbing.ClimbToTop() == false)
        {
            if (moveInput.y != 0)
            {
                climbingMovement += moveInput.y * capsule.transform.up;
                hasClimbingMovement = true;
                hasMovement = true;
                playerAnimator.SetFloat("MovementY", moveInput.y);
            }
            if (moveInput.x != 0)
            {
                climbingMovement += moveInput.x * capsule.transform.right;
                hasClimbingMovement = true;
                hasMovement = true;
                playerAnimator.SetFloat("MovementX", moveInput.x);
            }

        }

        //Movement
        MovePlayer(movement);
        HandleSpeed();
        playerAnimator.SetFloat("Speed", speed, dampTime, Time.deltaTime);

        //Climbing
        Climb(climbingMovement);

        //Strafing
        Strafe(movement);

        //Jumping
        Jump();

        //Gliding
        if (isGliding)
        {
            playerRigidbody.drag = glideDrag;
            movementSpeed = glideSpeed;           
        }
        else
        {
            playerRigidbody.drag = initialDrag;
            //movementSpeed = walkSpeed;
        }      

        if (hasMovement == false && hasClimbingMovement == false)
        {
            playerAnimator.SetFloat("MovementX", 0);
            playerAnimator.SetFloat("MovementY", 0);
        }

        CheckFall();

        Grounded();

        if (isBouncing)
        {
            currentBouncing -= Time.fixedDeltaTime;
            playerRigidbody.AddForce(-capsule.transform.forward * knockbackForce, ForceMode.Impulse);
        }

        //Keeping the jump inertia
        if(hadMovement && wallJumping == false && (hasJumped || jumping))
        {
            playerRigidbody.AddForce(playerRigidbody.transform.forward * inertia, ForceMode.Impulse);
        }

        //playerRigidbody.AddForce(Physics.gravity, ForceMode.Force);
    }

    protected override void MovePlayer(Vector3 currentMovement)
    {
        //Moving the player on the ground
        if ((IsTouchingDifferentGround() == false || IsTouchingTheGround()) && adventureClimbing.IsClimbing() == false)
        {
            currentMovement += currentMovement * movementSpeed;
            playerRigidbody.velocity = new Vector3(currentMovement.x, playerRigidbody.velocity.y, currentMovement.z);
        }
    }

    private void Climb(Vector3 climbing)
    {
        //Sticking the player to the wall
        if (adventureClimbing.IsClimbing() && wallJumping == false)
        {
            playerRigidbody.useGravity = false;
            climbing += climbing * climbingSpeed;
            playerRigidbody.velocity = new Vector3(climbing.x, climbing.y, climbing.z);

            //Climbing to the top of the wall
            if (adventureClimbing.ClimbToTop() && wallJumping == false)
            {
                //transform.position = Vector3.MoveTowards(transform.position, adventureClimbing.TopPosition(), 5f);
                StartCoroutine(GetPlayerUp(0.5f, adventureClimbing.TopPosition()));
                climbing = Vector3.zero;
            }
        }
        else
        {
            playerRigidbody.useGravity = true;
            playerRigidbody.AddForce(Physics.gravity, ForceMode.Force);
            climbing = Vector3.zero;
        }

        if(moveInput.x >= 0.5 || moveInput.x <= -0.5)
        {
            playerAnimator.SetBool("SideMovement", true);
        }
        else
        {
            playerAnimator.SetBool("SideMovement", false);
        }
    }

    protected override void Jump()
    {
        //Jumping
        if (jumping == true)
        {
            Vector3 jumpVector = Vector3.up * currentJumpForce;
            playerRigidbody.AddForce(jumpVector, ForceMode.Impulse);
            if (currentJumpForce > 0)
            {
                currentJumpForce -= jumpDecrement * Time.fixedDeltaTime;
            }

        }

        //Wall jump
        if (wallJumping == true)
        {
            playerRigidbody.AddForce(adventureClimbing.OffWallJump(), ForceMode.Impulse);
            if (currentJumpForce > 0)
            {
                currentJumpForce -= jumpDecrement * Time.fixedDeltaTime;
            }
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
            if (hasClimbingMovement == false)
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
    }

    override protected void CheckFall()
    {
        playerAnimator.SetBool("OnGround", IsTouchingTheGround());
        if (playerRigidbody.velocity.y < fallDetection && !IsTouchingTheGround() && isGliding == false)
        {
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
            glider.SetActive(false);
            hadMovement = false;
            hasJumped = false;
            isInAir = false;
            isGliding = false;
            wallJumping = false;
            currentBuffer = 0;
        }
        else
        {
            isInAir = true;
        }
    }

    protected override void HandleSpeed()
    {
        //Handle currentMovement speed and the "speed" parameter for animations 
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
            else
            {
                movementSpeed = walkSpeed;
                speed = 0.5f;
            }

            //Speed for gamepad
            if (playerInputMng.GetGamepadActive() == true)
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

    IEnumerator GetPlayerUp(float timeToMove, Vector3 posToMove)
    {
        float time = 0;
        Vector3 currentPosition = transform.position;
        while(time < timeToMove)
        {
            //transform.position = Vector3.MoveTowards(transform.position, adventureClimbing.TopPosition(), 5f);
            transform.position = Vector3.Lerp(currentPosition, posToMove, time/timeToMove);
            time += Time.fixedDeltaTime;
            yield return null;
        }
        transform.position = posToMove;
    }

    override protected bool IsCloseToTheGround() => Physics.Raycast(feetTransform.position, transform.TransformDirection(Vector3.down), 1.5f, floorMask);
    override protected bool IsTouchingTheGround() => Physics.CheckSphere(feetTransform.position, 0.25f, floorMask);
}
