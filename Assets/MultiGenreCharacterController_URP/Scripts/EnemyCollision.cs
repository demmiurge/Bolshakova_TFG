using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyCollision : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private BaseMovement playerMovement;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<BaseMovement>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Enemy" || collision.transform.tag == "EnemyBullet")
        {
            playerHealth.TakeDamage(1f);
            playerMovement.CollidedWithEnemy();
        }

        if (collision.transform.tag == "Death")
        {
            playerHealth.Die();
        }

    }
}
