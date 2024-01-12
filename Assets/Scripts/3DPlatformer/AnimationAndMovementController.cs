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

    [Header("Player Control Variables")]
    [SerializeField] private float _rotationFactor = 0.6f;
    [SerializeField] private float _walkSpeed = 1.5f;
    [SerializeField] private float _runSpeed = 3.0f;

    private float _gravityWhileAirborne;
    [SerializeField] private float _gravityWhenGrounded = 0.05f;

    [Space(1.0f)]
    [Header("Jump Variables")]
    [SerializeField] private float _maxJumpHeight = 1.0f;
    [SerializeField] private float _jumpTimeToApex = 0.25f;

    private bool _isJumpPressed;
    private float _initialJumpVelocity;
    private bool _isJumping;

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

            platformerPlayerInput.CharacterControls.Jump.started += OnJump;
            platformerPlayerInput.CharacterControls.Jump.canceled += OnJump;

        };
        SetUpJumpVariables();
    }


    private void SetUpJumpVariables() 
    {
        _gravityWhileAirborne = (2 * _maxJumpHeight) / Mathf.Pow(_jumpTimeToApex, 2);
        _initialJumpVelocity = (2 * _maxJumpHeight) / _jumpTimeToApex;
    }

    public void PerformJump() 
    {
        //check input and grounded state, to jump 
        if (!_isJumping && _characterController.isGrounded && _isJumpPressed)
        {
            _isJumping = true;
            currentWalkMovement.y += _initialJumpVelocity;
            currentRunMovement.y += _initialJumpVelocity;
        }
        //check input and grounded state, to set that it is not jumping.
        else if (_isJumping && _characterController.isGrounded && !_isJumpPressed) 
        {
            _isJumping = false;
        }
    }

    void OnMovementInput(InputAction.CallbackContext context) 
    {
        currentMovementInput = context.ReadValue<Vector2>();
        //set the walking speed. this will get fixed in the StateMachine version.
        currentWalkMovement.x = currentMovementInput.x * _walkSpeed;
        currentWalkMovement.z = currentMovementInput.y * _walkSpeed;

        //set the runnning speed. yeah, no shot this should be calculated every frame.
        currentRunMovement.x = currentMovementInput.x * _runSpeed;
        currentRunMovement.z = currentMovementInput.y * _runSpeed;

        //check if there is any input, and assign to bool.
        _isMovementPressed = currentWalkMovement.x != 0 || currentMovementInput.y != 0;
    }

    void OnRun(InputAction.CallbackContext context) 
    {
        _isRunPressed = context.ReadValueAsButton();
    }

    private void OnJump(InputAction.CallbackContext context) 
    {
        _isJumpPressed = context.ReadValueAsButton();
        Debug.Log(_isJumpPressed);
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
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation,_rotationFactor * Time.deltaTime);
        }

    }

    private void ApplyGravity() 
    {
        if (_characterController.isGrounded)
        {
            currentWalkMovement.y = -_gravityWhenGrounded;
            currentRunMovement.y = -_gravityWhenGrounded;
        }
        else 
        {
            currentWalkMovement.y = -_gravityWhileAirborne;
            currentRunMovement.y = -_gravityWhileAirborne;
        } 
    }

    // Update is called once per frame
    void Update()
    {
        ChangeAnimation();
        RotateCharacter();
        

        if (_isRunPressed)
        {
            _characterController.Move(currentRunMovement * Time.deltaTime);
        }
        else
        {
            _characterController.Move(currentWalkMovement * Time.deltaTime);
        }
        //Aplly gravity after the character has moved to the new location.
        ApplyGravity();
        PerformJump();
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
