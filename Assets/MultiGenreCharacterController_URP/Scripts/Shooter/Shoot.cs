using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Shoot : MonoBehaviour
{
    [Header("Gun")]
    [SerializeField] private Transform bulletPosition;  //Can be used to instantiate Bullet Game Object
    [SerializeField] private float shootForce = 25f;
    [SerializeField] private bool canShoot = true;
    public ParticleSystem shootParticles;

    [Header("Camera")]
    public Camera camera;

    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject holePrefab;
    [SerializeField] private float destroyTime = 5f;

    [Header("Cooldown")]
    [SerializeField] private float fireRate = 1.0f;   

    [SerializeField] private LayerMask validSurfacesLayer;


    //Private Variables
    private Animator playerAnimator;
    private float nextFireTime;
    private bool isShooting = false;
    private Vector3 direction;
    private WaitForSeconds firing;

    private void Awake()
    {
        bulletPosition = GameObject.FindGameObjectWithTag("Gun").transform;
        firing = new WaitForSeconds(1 / fireRate);
        playerAnimator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        isShooting = false;

    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void OnChangeWeapon(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            canShoot = !canShoot;
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            playerAnimator.SetBool("Shooting", true);
            StartFire();
        }

        if(context.canceled)
        {
            StopFire();
            playerAnimator.SetBool("Shooting", false);
        }
    }

    void Shooting()
    {        
        Ray cameraToTarget = camera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
        RaycastHit hitTargetPoint;
        Vector3 targetPoint = Vector3.forward;
        Vector3 shootDirection = GetDirection();

        if (Physics.Raycast(cameraToTarget, out hitTargetPoint, Mathf.Infinity, validSurfacesLayer))
            targetPoint = hitTargetPoint.point;


        GameObject bullet = Instantiate(bulletPrefab, bulletPosition.position, Quaternion.identity);
        bullet.GetComponent<Rigidbody>().AddForce(shootDirection * shootForce, ForceMode.Impulse);

        GameObject bulletHole = Instantiate(holePrefab, hitTargetPoint.point, Quaternion.LookRotation(hitTargetPoint.normal));
        StartCoroutine(DestroyDecal(bulletHole));

        shootParticles.Play();

        nextFireTime = Time.time + fireRate;
    }

    private Vector3 GetDirection()
    {
        Vector3 direction;

        Ray raySeekTargetPoint = camera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
        RaycastHit hitTargetPoint;
        Vector3 targetPoint = Vector3.forward;

        if (Physics.Raycast(raySeekTargetPoint, out hitTargetPoint, Mathf.Infinity, validSurfacesLayer))
            targetPoint = hitTargetPoint.point;

        Debug.DrawLine(camera.transform.position, targetPoint, Color.magenta);


        if (targetPoint == Vector3.forward)
            direction = camera.transform.forward;
        else
            direction = (targetPoint - bulletPosition.position).normalized;

        this.direction = direction;

        return direction;
    }


    private void FixedUpdate()
    {

    }

    void StartFire()
    {
        isShooting = true;
        StartCoroutine(Fire());
    }

    void StopFire()
    {
        isShooting = false;
        StopCoroutine(Fire());
    }

    IEnumerator Fire()
    {
        while(isShooting)
        {
            Shooting();
            yield return firing;
        }
    }

    IEnumerator DestroyDecal(GameObject hole)
    {
        yield return new WaitForSeconds(destroyTime);
        GameObject.Destroy(hole.gameObject);
    }
}
