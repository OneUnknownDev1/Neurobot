using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset playerControls;

    [Header("Action Map Name References")]
    [SerializeField] private string actionMapName = "Player";

    [Header("Action Name References")]
    [SerializeField] private string move = "Move";
    [SerializeField] private string look = "Look";
    [SerializeField] private string jump = "Jump";
    [SerializeField] private string sprint = "Sprint";
    [SerializeField] private string flashlight = "Flashlight";
    [SerializeField] private string Interact = "Interact";
   
    [Header("Deadzone Values")]
    [SerializeField] private float leftStickDeadzoneValue;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction flashlightAction;
    private InputAction DoorAction; 
    private InputAction interactableAction;

    private InputAction sprintAction;



    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpTriggered { get; private set; }
    public float SprintValue { get; private set; }

    public bool FlashlightTriggered { get; private set; }
    public bool FlashlightOff { get; private set; }

    public bool Interacting { get; private set; }
   

    public static PlayerInputHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Instance = this; 
        }

        moveAction = playerControls.FindActionMap(actionMapName).FindAction(move);
        lookAction = playerControls.FindActionMap(actionMapName).FindAction(look);
        jumpAction = playerControls.FindActionMap(actionMapName).FindAction(jump);
        sprintAction = playerControls.FindActionMap(actionMapName).FindAction(sprint);
        flashlightAction = playerControls.FindActionMap(actionMapName).FindAction(flashlight);
        interactableAction = playerControls.FindActionMap(actionMapName).FindAction(Interact);
        RegisterInputActions();
      

        InputSystem.settings.defaultDeadzoneMin = leftStickDeadzoneValue;
        PrintDevices();
    }

    void PrintDevices()
    {
        foreach (var device in InputSystem.devices)
        {
            if (device.enabled)
            {
                Debug.Log("Active Device: " + device.name);
            }
        }
    }

    void RegisterInputActions()
    {
        moveAction.performed += context => MoveInput = context.ReadValue<Vector2>(); 
        moveAction.canceled += context => MoveInput = Vector2.zero;
        

        lookAction.performed += context => LookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => LookInput = Vector2.zero;

        jumpAction.performed += context => JumpTriggered = true;
        jumpAction.canceled += context => JumpTriggered = false;

        sprintAction.performed += context => SprintValue = context.ReadValue<float>();
        sprintAction.canceled += context => SprintValue = 0f;

        flashlightAction.performed += context => FlashlightTriggered = true;
        flashlightAction.canceled += context => FlashlightTriggered = false;

        interactableAction.performed += context => Interacting = true;
        interactableAction.canceled += context => Interacting = false;

    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        flashlightAction.Enable();
        interactableAction.Enable();

        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        flashlightAction.Disable();
        interactableAction.Disable();

        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change) 
        {
            case InputDeviceChange.Disconnected:
                Debug.Log("Device Disconnected: " + device.name);
                //HANDLE DISCONNECTION
                break;
            case InputDeviceChange.Reconnected:
                Debug.Log("Device Reconnected: " + device.name);
                //HANDLE DISCONNECTION
                break;

        }
    }
}
