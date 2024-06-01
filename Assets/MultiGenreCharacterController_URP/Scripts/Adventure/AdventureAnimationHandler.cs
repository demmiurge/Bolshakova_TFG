using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdventureAnimationHandler : AnimationHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Jump()
    {
        playerAnimator.SetBool("Jump", true);
        playerAnimator.SetBool("Falling", false);
        playerAnimator.SetBool("OnGround", false);
        playerAnimator.SetBool("CloseToGround", false);
    }

    public void WallJump()
    {
        playerAnimator.SetTrigger("JumpOffWall");
        playerAnimator.SetBool("OnGround", false);
        playerAnimator.SetBool("CloseToGround", false);
    }

    public void Glide()
    {
        playerAnimator.SetBool("Jump", false);
        playerAnimator.SetBool("Falling", false);
        playerAnimator.SetBool("OnGround", false);
        playerAnimator.SetBool("Gliding", true);
    }

    public void TouchingGround()
    {
        playerAnimator.SetBool("OnGround", true);       
        playerAnimator.SetBool("Gliding", false);
    }

    public void Falling()
    {
        playerAnimator.SetBool("Jump", false);
        playerAnimator.SetBool("Falling", true);
    }
}
