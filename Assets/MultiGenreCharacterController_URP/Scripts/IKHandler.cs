using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKHandler : MonoBehaviour
{
    [SerializeField] private LayerMask detectionLayerMask;
    [SerializeField] private float distanceToGround;
    [SerializeField] private float ikWeight = 1.0f;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, ikWeight);

            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, ikWeight);

            // Left foot
            RaycastHit hit;
            Ray ray = new Ray(animator.GetIKPosition(AvatarIKGoal.LeftFoot), Vector3.down);
            Debug.DrawRay(animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down, Color.red);
            if (Physics.Raycast(ray, out hit, distanceToGround + 1f, detectionLayerMask))
            {
                if (hit.transform.tag == "Floor")
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += distanceToGround;
                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, new Vector3(animator.GetIKPosition(AvatarIKGoal.LeftFoot).x, footPosition.y, animator.GetIKPosition(AvatarIKGoal.LeftFoot).z));
                    //animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }
            }

            // Right foot
            ray = new Ray(animator.GetIKPosition(AvatarIKGoal.RightFoot), Vector3.down);
            if (Physics.Raycast(ray, out hit, distanceToGround + 1f, detectionLayerMask))
            {
                if (hit.transform.tag == "Floor")
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += distanceToGround;
                    animator.SetIKPosition(AvatarIKGoal.RightFoot, new Vector3(animator.GetIKPosition(AvatarIKGoal.RightFoot).x, footPosition.y, animator.GetIKPosition(AvatarIKGoal.RightFoot).z));
                    //animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }
            }
        }
    }
}
