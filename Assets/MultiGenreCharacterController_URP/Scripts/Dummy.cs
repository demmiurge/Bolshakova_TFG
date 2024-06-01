using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour
{
    public float maxHealth;
    public ParticleSystem damage;
    public ParticleSystem death;
    public GameObject parentGameObject;

    private float currentHealth;
   
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentHealth <= 0)
        {
            death.Play();
            Invoke(nameof(Died), 0.1f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Bullet")
        {
            if(currentHealth > 0)
            {
                currentHealth -= collision.gameObject.GetComponent<Bullet>().damage;
                damage.Play();
            }
            else
            {
                death.Play();
                Die();
            }
        }
    }

    void Died()
    {
        if (parentGameObject == null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Destroy(parentGameObject);
        }
    }

    IEnumerator Die()
    {
        yield return new WaitForSeconds(0.1f);
        if (parentGameObject == null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Destroy(parentGameObject);
        }
    }
}
