using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("General")]
    public float maxHealth = 3;
    public float minHealth = 1;
    public bool autoHeal = true;

    private BaseMovement playerMovement;
    private Animator playerAnimator;
    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        playerAnimator = GetComponent<Animator>();
        playerMovement = GetComponent<BaseMovement>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (autoHeal)
        {
            RestoreHealth();
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth < minHealth)
        {
            Die();
        }
        else
        {
            //lifeUI.SetDamage(damage);

        }
    }

    public void Die()
    {
        currentHealth = 0;
        playerAnimator.SetTrigger("Death");
        playerAnimator.SetBool("Dead", true);
    }

    private void RestoreHealth()
    {
        if(currentHealth < maxHealth)
        {
            currentHealth += 0.1f;
        }
    }

}
