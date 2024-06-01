using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    //Variables accessible from the inspector
    [Header("General")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform cameraPosition;
    [SerializeField] private Transform cameraWalls;
    [SerializeField] private Transform zoomPosition;
    [SerializeField] private Transform cameraPositionStart;
    [SerializeField] private GameObject player;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float minPitch = -20;
    [SerializeField] private float maxPitch = 90;

    [Header("Invert Controls")]
    [SerializeField] private bool pitchInverted;
    [SerializeField] private bool yawInverted;

    [Header("Mouse")]
    [SerializeField] public float yawRotationalSpeedMouse = 40;
    [SerializeField] public float pitchRotationalSpeedMouse = 40;

    [Header("Gamepad")]
    [SerializeField] public float yawRotationalSpeedGamepad = 60;
    [SerializeField] public float pitchRotationalSpeedGamepad = 60;

    [Header("Player")]
    [SerializeField] private LayerMask avoidObjectsLayerMask;
    [SerializeField] private float offset = 0.1f;
    [SerializeField] private PlayerInputManager playerInputMng;
    [SerializeField] private BaseMovement playerMovement;

    //Private Variables
    private Camera playerCamera;
    private float pitch = 0.0f;
    float mouseXpos;
    float mouseYpos;
    private float yawRotationalSpeed;
    private float pitchRotationalSpeed;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = GetComponent<Camera>();
        if (playerInputMng.GetGamepadActive() == true)
        {
            yawRotationalSpeed = yawRotationalSpeedGamepad;
            pitchRotationalSpeed = pitchRotationalSpeedGamepad;
        }
        else
        {
            yawRotationalSpeed = yawRotationalSpeedMouse;
            pitchRotationalSpeed = pitchRotationalSpeedMouse;
        }

        transform.forward = player.transform.forward;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnCamera(InputAction.CallbackContext context)
    {
        mouseXpos = context.ReadValue<Vector2>().x;
        mouseYpos = context.ReadValue<Vector2>().y;

    }

    void LateUpdate()
    {
        float mouseX = yawRotationalSpeed * mouseXpos * Time.deltaTime;
        float mouseY = pitchRotationalSpeed * mouseYpos * Time.deltaTime;

        Vector3 cameraRotation = playerCamera.transform.rotation.eulerAngles;
        float yaw = cameraRotation.y;
        cameraRotation.x -= mouseY;
        cameraRotation.y += mouseX;

        if(pitchInverted == false)
        {
            pitch -= mouseY;
        }
        else
        {
            pitch += mouseY;
        }

        if(yawInverted == false)
        {
            yaw += mouseX;
        }
        else
        {
            yaw -= mouseX;
        }


        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        cameraPivot.transform.rotation = Quaternion.Euler(pitch, yaw, cameraRotation.z);
        playerCamera.transform.rotation = Quaternion.Euler(pitch, yaw, cameraRotation.z);

        float distance = Vector3.Distance(transform.position, cameraPosition.position);
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
        Vector3 desiredPosition = cameraPosition.transform.position - playerCamera.transform.forward * distance;


        float camDistance = Vector3.Distance(playerCamera.transform.position, cameraWalls.position);
        camDistance = Mathf.Clamp(camDistance, 1f, 4.5f);
        Ray ray = new Ray(cameraWalls.position, -transform.forward);
        RaycastHit raycastHit;
        Debug.DrawRay(cameraWalls.position, -transform.forward, Color.red);

        if (Physics.Raycast(ray, out raycastHit, camDistance, avoidObjectsLayerMask.value) /*&& playerMovement.GetZoom() == false*/)
        {
            desiredPosition = raycastHit.point - transform.forward * offset;
        }

        transform.position = desiredPosition;
    }


    public void SetRotation()
    {
        transform.forward = player.transform.forward;
    }

    //Setters
    public void SetYaw(float yaw)
    {
        yawRotationalSpeed = yaw;
    }

    public void SetPitch(float pitch)
    {
        pitchRotationalSpeed = pitch;
    }

    //Getter

    public float GetPitch()
    {
        float retPitch;
        retPitch = Mathf.InverseLerp(minPitch, maxPitch, pitch);
        return retPitch;
    }
}
