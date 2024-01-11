using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementController : MonoBehaviour
{
    public PlatformerPlayerInputs platformerPlayerInput;
    private CharacterController _characterController;
    private Animator _animator;

    private int _isWalkingHash;
    private int _isRunningHash;

    [Header("Player Control")]
    [SerializeField] private float rotationFactor;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    [SerializeField] private float gravityWhileAirborne;
    [SerializeField] private float gravityWhenGrounded;

    [Space(1.0f)]
    //input values
    [Header("Debug Values")]
    [SerializeField] private Vector2 currentMovementInput;
    [SerializeField] private Vector3 currentWalkMovement;
    [SerializeField] private Vector3 currentRunMovement;

    [SerializeField] private bool _isMovementPressed;
    [SerializeField] private bool _isRunPressed;


    void Awake()
    {
        platformerPlayerInput = new PlatformerPlayerInputs();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        _isWalkingHash = Animator.StringToHash("isWalking");
        _isRunningHash = Animator.StringToHash("isRunning");

        platformerPlayerInput.CharacterControls.Move.started += context => {
            //Debug.Log(context.ReadValue<Vector2>());
            platformerPlayerInput.CharacterControls.Move.started += OnMovementInput;
            platformerPlayerInput.CharacterControls.Move.canceled += OnMovementInput;
            platformerPlayerInput.CharacterControls.Move.performed += OnMovementInput;
            platformerPlayerInput.CharacterControls.Run.started += OnRun;
            platformerPlayerInput.CharacterControls.Run.canceled += OnRun;
        };
    }

    void OnMovementInput(InputAction.CallbackContext context) 
    {
        currentMovementInput = context.ReadValue<Vector2>();
        //set the walking speed. this will get fixed in the StateMachine version.
        currentWalkMovement.x = currentMovementInput.x * walkSpeed;
        currentWalkMovement.z = currentMovementInput.y * walkSpeed;

        //set the runnning speed. yeah, no shot this should be calculated every frame.
        currentRunMovement.x = currentMovementInput.x * runSpeed;
        currentRunMovement.z = currentMovementInput.y * runSpeed;

        //check if there is any input, and assign to bool.
        _isMovementPressed = currentWalkMovement.x != 0 || currentMovementInput.y != 0;
    }

    void OnRun(InputAction.CallbackContext context) 
    {
        _isRunPressed = context.ReadValueAsButton();
    }

    private void ChangeAnimation() 
    {
        //get parameters from animator
        bool isWalking = _animator.GetBool(_isWalkingHash);
        bool isRunning = _animator.GetBool(_isRunningHash);

        //Debug.Log(string.Format("isWalking är: {0}", isWalking));

        if (_isMovementPressed && !isWalking)
        {
            _animator.SetBool(_isWalkingHash, true);
        }
        else if (!_isMovementPressed && isWalking) 
        {
            _animator.SetBool(_isWalkingHash, false);
        }

        //not very pretty, but will be fixed in StateMachine.
        if (_isRunPressed && !isRunning)
        {
            _animator.SetBool(_isRunningHash, true);
        }
        else if (!_isRunPressed && isRunning)
        {
            _animator.SetBool(_isRunningHash, false);
        }

    }

    private void RotateCharacter() 
    {
        Vector3 positionToLookAt;
        //the change in position hte character should point to.
        positionToLookAt.x = currentWalkMovement.x;
        positionToLookAt.y = 0;
        positionToLookAt.z = currentWalkMovement.z;

        Quaternion currentRotation = transform.rotation;

        if (_isMovementPressed) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation,rotationFactor * Time.deltaTime);
        }

    }

    private void ApplyGravity() 
    {
        if (_characterController.isGrounded)
        {
            currentWalkMovement.y = -gravityWhenGrounded;
            currentRunMovement.y = -gravityWhenGrounded;
        }
        else 
        {
            currentWalkMovement.y = -gravityWhileAirborne;
            currentRunMovement.y = -gravityWhileAirborne;
        } 
    }

    // Update is called once per frame
    void Update()
    {
        ChangeAnimation();
        RotateCharacter();
        ApplyGravity();

        if (_isRunPressed)
        {
            _characterController.Move(currentRunMovement * Time.deltaTime);
        }
        else
        {
            _characterController.Move(currentWalkMovement * Time.deltaTime);
        }
    }

    private void OnEnable()
    {
        platformerPlayerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        platformerPlayerInput.CharacterControls.Disable();
    }
}
