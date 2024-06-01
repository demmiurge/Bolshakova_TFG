using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachController : MonoBehaviour
{
    public Transform cameraPivot;
    private GameObject parentObject;
    private BaseMovement playerMovement;
    private Rigidbody rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        playerMovement = GetComponent<BaseMovement>();
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        CheckSurface(collision);
        transform.up = Vector3.up;
    }

    void OnCollisionStay(Collision collision)
    {
        CheckSurface(collision);
        transform.up = Vector3.up;
    }

    private void CheckSurface(Collision collision)
    {

        if (collision.transform.tag == "Platform")
        {
            if (parentObject == null)
            {
                transform.SetParent(collision.transform, true);
                parentObject = collision.gameObject;
            }
            if (parentObject != collision.gameObject)
            {
                transform.SetParent(collision.transform, true);
                parentObject = collision.gameObject;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        transform.SetParent(null);
        parentObject = null;
    }
}
