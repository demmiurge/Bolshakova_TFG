using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] private float crouchHeight;
    [SerializeField] private float crouchIdleHeight;
    [SerializeField] private float originalHeight;
    [SerializeField] private Transform capsule;
    // Start is called before the first frame update
    void Start()
    {
        originalHeight = capsule.GetComponent<CapsuleCollider>().height;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ReduceCollider()
    {
        capsule.GetComponent<CapsuleCollider>().height = crouchIdleHeight;
    }

    void ColliderMovement()
    {
        capsule.GetComponent<CapsuleCollider>().height = crouchHeight;
    }

    void ResetCollider()
    {
        capsule.GetComponent<CapsuleCollider>().height = originalHeight;
    }
}
