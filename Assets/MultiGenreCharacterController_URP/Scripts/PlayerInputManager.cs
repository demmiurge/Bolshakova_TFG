using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class PlayerInputManager : MonoBehaviour
{
    public PlayerInput playerInputMap;
    public CameraMovement cameraMovement;

    private bool gamepadActive = false;

    //private ControllerManager controllerManager;

    private void Awake()
    {
        CheckManagersInstance();

        playerInputMap = GetComponent<PlayerInput>();
        if (Gamepad.all.Count > 0)
        {
            playerInputMap.SwitchCurrentActionMap("Gamepad");
            gamepadActive = true;
        }
        else
        {
            playerInputMap.SwitchCurrentActionMap("Keyboard");
            gamepadActive = false;
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        //CheckManagersInstance();

        if (Gamepad.all.Count > 0)
        {
            playerInputMap.SwitchCurrentActionMap("Gamepad");
            gamepadActive = true;
        }
        else
        {
            playerInputMap.SwitchCurrentActionMap("Keyboard");
            gamepadActive = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //CheckManagersInstance();
    }

    public void OnDeviceLost()
    {
        playerInputMap.SwitchCurrentActionMap("Keyboard");
        gamepadActive = false;
        if (cameraMovement)
        {
            cameraMovement.SetYaw(cameraMovement.yawRotationalSpeedMouse);
            cameraMovement.SetPitch(cameraMovement.pitchRotationalSpeedMouse);
        }
    }

    public void OnDeviceRegained()
    {
        playerInputMap.SwitchCurrentActionMap("Gamepad");
        gamepadActive = true;
        if (cameraMovement)
        {
            cameraMovement.SetYaw(cameraMovement.yawRotationalSpeedGamepad);
            cameraMovement.SetPitch(cameraMovement.pitchRotationalSpeedGamepad);
        }
    }

    public void OnControlsChanged()
    {
        if (Gamepad.all.Count > 0)
        {
            playerInputMap.SwitchCurrentActionMap("Gamepad");
            gamepadActive = true;
            if (cameraMovement)
            {
                cameraMovement.SetYaw(cameraMovement.yawRotationalSpeedGamepad);
                cameraMovement.SetPitch(cameraMovement.pitchRotationalSpeedGamepad);
            }
        }
        else
        {
            playerInputMap.SwitchCurrentActionMap("Keyboard");
            gamepadActive = false;
            if (cameraMovement)
            {
                cameraMovement.SetYaw(cameraMovement.yawRotationalSpeedMouse);
                cameraMovement.SetPitch(cameraMovement.pitchRotationalSpeedMouse);
            }
        }
    }

    void CheckManagersInstance()
    {
        if(Gamepad.all.Count > 0 && gamepadActive)
        {
            if(Input.anyKeyDown)
            {
                playerInputMap.SwitchCurrentActionMap("Keyboard");
                gamepadActive = false;
                if (cameraMovement)
                {
                    cameraMovement.SetYaw(cameraMovement.yawRotationalSpeedMouse);
                    cameraMovement.SetPitch(cameraMovement.pitchRotationalSpeedMouse);
                }
            }

            if(Gamepad.current.IsActuated())
            {
                playerInputMap.SwitchCurrentActionMap("Gamepad");
                gamepadActive = true;
                if (cameraMovement)
                {
                    cameraMovement.SetYaw(cameraMovement.yawRotationalSpeedGamepad);
                    cameraMovement.SetPitch(cameraMovement.pitchRotationalSpeedGamepad);
                }
            }
        }
    }

    public bool GetGamepadActive()
    {
        return gamepadActive;
    }

    public Gamepad GetGamepad()
    {
        return Gamepad.current;
    }
}
