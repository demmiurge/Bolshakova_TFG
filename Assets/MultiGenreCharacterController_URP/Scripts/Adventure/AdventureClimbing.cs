using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static Unity.Burst.Intrinsics.X86;

public class AdventureClimbing : MonoBehaviour
{
    private Rigidbody body;
    private Animator playerAnimator;
    [Header("Detection Point")]
    public Transform detectionPosition;
    [Header("Parameters")]
    [SerializeField] private float maxClimbingTime = 10f;
    [SerializeField] private float wallDetectionDistance = 5f;
    [SerializeField] private float sphereCastRadius = 1f;
    [SerializeField] private float wallDetectionAngle = 45f;
    [SerializeField] private float climbJumpUp = 2f;
    [SerializeField] private float climbJumpBack = 3f;

    [Header("Layer")]
    [SerializeField] private LayerMask rockMask;

    private bool climbTop = false;
    private bool climbing = false;
    private bool wasClimbing = false;
    private float climbTimer;
    private float lookAngle;
    private RaycastHit hitWall;
    private bool wallInFront;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckWall();
        if(wallInFront && lookAngle <= wallDetectionAngle)
        {
            climbing = true;
            wasClimbing = true;
            playerAnimator.SetBool("Climbing", true);
        }
        else
        {
            climbing = false;
            climbTop = false;
            playerAnimator.SetBool("Climbing", false);
        }

        if(wasClimbing)
        {
            if(wallInFront == false)
            {
                playerAnimator.SetBool("ClimbingTop", true);
                climbTop = true;
                StartCoroutine(ClimbTop());
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(hitWall.point, sphereCastRadius);
        Gizmos.DrawRay(hitWall.point, hitWall.normal);
        Ray ray = new Ray(new Vector3(detectionPosition.position.x, detectionPosition.position.y, detectionPosition.position.z), transform.forward);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(ray);

        RaycastHit hitPosition, hitGround;
        Ray rayy = new Ray(new Vector3(detectionPosition.position.x, detectionPosition.position.y, detectionPosition.position.z), transform.forward);
        if (Physics.Raycast(rayy, out hitPosition, 1f))
        {
            Ray fray = new Ray((hitPosition.point + transform.forward * 0.3f + Vector3.up * 1.1f), Vector3.down);
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(fray);
            if(Physics.Raycast(fray, out hitGround, 1f))
            {
                Gizmos.DrawSphere(hitGround.point, sphereCastRadius);
            }
        }
        Gizmos.color = Color.green;
        Gizmos.DrawRay(rayy);
    }

    private void CheckWall()
    {
        Ray ray = new Ray(new Vector3(detectionPosition.position.x, detectionPosition.position.y, detectionPosition.position.z), transform.forward);
        if(Physics.Raycast(ray, out hitWall, wallDetectionDistance, rockMask))
        {
            wallInFront = true;
        }
        else { wallInFront = false; }
        lookAngle = Vector3.Angle(detectionPosition.forward, -hitWall.normal);
    }

    public bool IsClimbing()
    {
        return climbing;
    }

    public bool WallInFront()
    {
        return wallInFront;
    }

    public Vector3 OffWallJump()
    {
        Vector3 jumpForce = transform.up * climbJumpUp + hitWall.normal * climbJumpBack;
        return jumpForce;
    }

    public Vector3 TopPosition()
    {
        Vector3 position = Vector3.zero;
        RaycastHit  hitPosition, hitGround;
        if(Physics.Raycast(new Vector3(detectionPosition.position.x, detectionPosition.position.y, detectionPosition.position.z), transform.forward, out hitPosition))
        {
            if (Physics.Raycast(hitPosition.point + transform.forward * 0.3f + Vector3.up * 1.1f, Vector3.down, out hitGround))
            {
                position = hitGround.point;
            }
        }
        else
        {
            position = Vector3.zero;
        }

        return position;
    }

    public bool ClimbToTop()
    {
        return climbTop;
    }

    IEnumerator ClimbTop()
    {
        yield return new WaitForSeconds(1);
        wasClimbing = false;
        climbTop = false;
        playerAnimator.SetBool("ClimbingTop", false);
    }
}
