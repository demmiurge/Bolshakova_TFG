using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject shooterUI;
    [SerializeField] private GameObject platformerUI;
    [SerializeField] private GameObject adventureUI;

    //[SerializeField] private GameObject shooter;
    //[SerializeField] private GameObject platformer;
    //[SerializeField] private GameObject adventure;
    [SerializeField] private GameObject currentPlayer;

    private GameObject currentUI;
    private GameObject previousUI;
    private GameObject currentDevice;
    private GameObject previousDevice;
    private void Awake()
    {
        /*if (shooter.gameObject.activeInHierarchy == true)
        {
            currentUI = shooterUI;
            currentPlayer = shooter;
            previousUI = currentUI;
        }
        else if (platformer.gameObject.activeInHierarchy == true)
        {
            currentUI = platformerUI;
            currentPlayer = platformer;
            previousUI = currentUI;
        }
        else if (adventure.gameObject.activeInHierarchy == true)
        {
            currentUI = adventureUI;
            currentPlayer = adventure;
            previousUI = currentUI;
        }*/

        if(currentPlayer.gameObject.tag == "Shooter")
        {
            currentUI = shooterUI;
            previousUI = currentUI;
        }
        else if(currentPlayer.gameObject.tag == "Platformer")
        {
            currentUI = platformerUI;
            previousUI = currentUI;
        }
        else if( currentPlayer.gameObject.tag == "Adventure")
        {
            currentUI = adventureUI;
            previousUI = currentUI;
        }

        if (currentPlayer.GetComponent<PlayerInputManager>().GetGamepadActive())
        {
            currentDevice = currentUI.transform.GetChild(1).gameObject;
            previousDevice = currentDevice;
        }
        else
        {
            currentDevice = currentUI.transform.GetChild(0).gameObject;
            previousDevice = currentDevice;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        
    }

    // Update is called once per frame
    void Update()
    {

        if(currentUI != previousUI)
        {
            previousUI.SetActive(false);
        }
        currentUI.SetActive(true);

        InputSystem.onDeviceChange += (device, change) =>
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    previousDevice.SetActive(false);
                    currentDevice = currentUI.transform.GetChild(1).gameObject;
                    previousDevice = currentDevice;
                    break;
                case InputDeviceChange.Removed:
                    previousDevice.SetActive(false);
                    currentDevice = currentUI.transform.GetChild(0).gameObject;
                    previousDevice = currentDevice;
                    break;
            }
        };

        currentDevice.SetActive(true);
    }
}
