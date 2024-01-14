using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationAndMovementController : MonoBehaviour
{
    public PlatformerPlayerInputs platformerPlayerInput;
    private CharacterController _characterController;
    private Animator _animator;

    //animation refs
    private int _isWalkingHash;
    private int _isRunningHash;
    private int _isJumpingHash;

    [Header("Player Control Variables")]
    [SerializeField] private float _rotationFactor = 0.6f;
    [SerializeField] private float _walkSpeed = 1.5f;
    [SerializeField] private float _runSpeed = 3.0f;

    
    [SerializeField] private float _gravityWhenGrounded = 0.05f;
    [SerializeField] private float _gravityFallMultiplier = 1.5f;

    [Space(1.0f)]
    [Header("Jump Variables")]
    [SerializeField] private float _maxJumpHeight = 1.0f;
    [SerializeField] private float _jumpTimeToApex = 0.25f;

    //jumping properties.
    private bool _isJumpPressed;
    private float _initialJumpVelocity;
    private bool _isJumping;
    private bool _isJumpAnimating;


    [Space(1.0f)]
    //input values
    [Header("Display for Debug Values")]
    [SerializeField] private Vector2 currentMovementInput;
    [SerializeField] private Vector3 currentWalkMovement;
    [SerializeField] private Vector3 currentRunMovement;

    [SerializeField] private float _gravityWhileAirborne;

    [SerializeField] private bool _isMovementPressed;
    [SerializeField] private bool _isRunPressed;
    [SerializeField] private bool _isFalling;


    void Awake()
    {
        platformerPlayerInput = new PlatformerPlayerInputs();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        _isWalkingHash = Animator.StringToHash("isWalking");
        _isRunningHash = Animator.StringToHash("isRunning");
        _isJumpingHash = Animator.StringToHash("isJumping");

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
        //I'm using timeToApex which means I don't multiply by 2 in the numerator.
        _gravityWhileAirborne = (-1 * _maxJumpHeight) / Mathf.Pow(_jumpTimeToApex, 2);
        _initialJumpVelocity = (1 * _maxJumpHeight) / _jumpTimeToApex;

        Debug.Log(string.Format("Setup Vars, _gravityWhileAirborne: {0}", _gravityWhileAirborne));
        Debug.Log(string.Format("Setup Vars, _initialJumpVelocity: {0}", _initialJumpVelocity));
    }

    public void PerformJump() 
    {
        //Debug.Log(string.Format("Perform jump, _isJumpPressed: {0}", _isJumpPressed));
        //check input and grounded state, to jump 
        if (!_isJumping && _characterController.isGrounded && _isJumpPressed)
        {
            _animator.SetBool(_isJumpingHash, true);
            _isJumping = true;
            //Debug.Log(string.Format("Perform jump, apply y velocity: {0}", _initialJumpVelocity));
            _isJumpAnimating = true;

            //the video multiplies these by 0.5f. but that seems very odd. means that maxjumpheight becomes incorrect.
            currentWalkMovement.y = _initialJumpVelocity;
            currentRunMovement.y = _initialJumpVelocity;
            Debug.Log(string.Format("Perform jump, walk y velocity: {0}", currentWalkMovement.y));
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
        //check if the character is falling
        //not a big fan of this, seems like falling gravity gets applied very late in the jump.
        if (currentWalkMovement.y < -0.5f || !_isJumpPressed)
        {
            _isFalling = true;
            //Debug.Log(string.Format("ApplyGravity, _isFalling", _isFalling));
            //Debug.Log(string.Format("Perform jump, walk y velocity: {0}", currentWalkMovement.y));
        }
        else 
        {
            _isFalling = false;
        }


        if (_characterController.isGrounded)
        {
            if (_isJumpAnimating) 
            {
                _animator.SetBool(_isJumpingHash, false);
                _isJumpAnimating = false;
            }
            currentWalkMovement.y = _gravityWhenGrounded;
            currentRunMovement.y = _gravityWhenGrounded;
            _isFalling = false;
            
        }
        else if (_isFalling) 
        {
            float previousYVelocity = currentWalkMovement.y;
            //Verlet
            float nextYVelocity = (previousYVelocity + (currentWalkMovement.y + (_gravityWhileAirborne *_gravityFallMultiplier * Time.deltaTime))) * 0.5f;
            currentWalkMovement.y = nextYVelocity;
            currentRunMovement.y = nextYVelocity;
        }

        //?
        else
        {
            float previousYVelocity = currentWalkMovement.y;


            //add the old velocity and the new velocity, average them and set that value.
            //called VelocityVerlet
            //float newYVelocity = currentWalkMovement.y + (_gravityWhileAirborne * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + (currentWalkMovement.y + (_gravityWhileAirborne * Time.deltaTime))) * 0.5f;

            currentWalkMovement.y = nextYVelocity;
            currentRunMovement.y = nextYVelocity;
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
            //Debug.Log(string.Format("running, y velocity: {0}", currentWalkMovement.y));
        }
        else
        {
            //Debug.Log(string.Format("walking, y velocity: {0}", currentWalkMovement.y));
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
