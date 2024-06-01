using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float destroyTime = 1f;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] public float damage;

    private float elapsedTime;
    private float maxTime = 2f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //GameObject.Destroy(this.gameObject);
        if (collision.gameObject.tag == "Shooter" || collision.gameObject.tag == "Gun")
        {
            Physics.IgnoreCollision(collision.collider, this.gameObject.GetComponent<SphereCollider>());
        }
        else
        {
            rb.velocity = Vector3.zero;
            StartCoroutine(DestroyBullet());
        }
    }

    void Start()
    {

    }

    void Update()
    {

        elapsedTime += Time.deltaTime;
        if(elapsedTime > maxTime)
        {
            StartCoroutine(DestroyBullet());
        }
    }

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(destroyTime);
        GameObject.Destroy(this.gameObject);

    }
}
