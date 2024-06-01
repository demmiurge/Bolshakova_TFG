using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterAnimationHandler : AnimationHandler
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

    public void TouchingGround()
    {
        playerAnimator.SetBool("OnGround", true);
    }

    public void Falling()
    {
        playerAnimator.SetBool("Jump", false);
        playerAnimator.SetBool("Falling", true);
        playerAnimator.SetBool("Crouch", false);
    }
}
