using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    public PlatformerPlayerInputs platformerPlayerInput;
    private CharacterController _characterController;
    private Animator _animator;

    //animation refs
    private int _isWalkingHash;
    private int _isRunningHash;
    private int _isJumpingHash;
    private int _jumpCountHash;

    [Header("Player Control Variables")]
    [Range(0, 1)] [SerializeField] private float _rotationFactor = 0.6f;
    [SerializeField] private float _walkSpeed = 1.5f;
    [SerializeField] private float _runSpeed = 3.0f;


    [SerializeField] private float _gravityWhenGrounded = 0.05f;
    [SerializeField] private float _gravityFallMultiplier = 1.5f;

    [Space(1.0f)]
    [Header("Jump Variables")]
    [SerializeField] private float _maxJumpHeight = 1.0f;
    [SerializeField] private float _secondJumpHeight = 1.25f;
    [SerializeField] private float _thirdJumpHeight = 1.5f;

    [SerializeField] private float _jumpTimeToApex = 0.25f;
    [SerializeField] private float _secondJumpTimeToApex = 0.35f;
    [SerializeField] private float _thirdJumpTimeTimeToApex = 0.45f;

    [SerializeField] private float _TimeToResetJumpCount = 0.5f;

    //jumping properties.
    private bool _isJumpPressed;
    private float _initialJumpVelocity;
    private bool _isJumping;
    private bool _requireNewJumpPress;
    [SerializeField] private int _jumpCount = 0;

    private Dictionary<int, float> _initialJumpVelocities = new Dictionary<int, float>();
    private Dictionary<int, float> _jumpGravityValues = new Dictionary<int, float>();
    private Coroutine _currentResetJumpRoutine = null;

    //state vriables
    private PlayerBaseState _currentState;
    private PlayerStateFactory _states;

    //Getters and setters

    //This says that PlayerBaseState can both get and set the current state.
    
    #region GettersAndSetters
    public PlayerBaseState CurrentState 
    {
        get { return _currentState; }
        set { _currentState = value; }
    }

    public CharacterController CharacterController
    {
        get { return _characterController; }
    }

    public Animator Animator
    {
        get { return _animator; }
    }

    public Coroutine CurrentResetJumpRoutine
    {
        get { return _currentResetJumpRoutine; }
        set { _currentResetJumpRoutine = value; }
    }

    public Dictionary<int, float> InitialJumpVelocities
    {
        get { return _initialJumpVelocities; }
    }

    public Dictionary<int, float> JumpGravityValues
    {
        get { return _jumpGravityValues; }
    }

    public float GravityFallMultipier 
    {
        get { return _gravityFallMultiplier; }
    }

    public int JumpCount 
    {
        get { return _jumpCount; }
        set { _jumpCount = value; }
    }

    public int IsJumpingHash
    {
        get { return _isJumpingHash; }
    }

    public int JumpCountHash
    {
        get { return _jumpCountHash; }
    }

    public bool RequireNewJumpPress
    {
        get { return _requireNewJumpPress; }
        set { _requireNewJumpPress = value; }
    }
    public bool IsJumping
    {
        set { _isJumping = value; }
    }

    public bool IsJumpPressed 
    {
        get { return _isJumpPressed; } 
    }

    public float TimeToResetJumpCount 
    {
        get { return _TimeToResetJumpCount; }
    }

    public float GravityWhileGrounded
    {
        get { return _gravityWhenGrounded; }
    }

    public float CurrentWalkMovementY
    {
        get { return _currentWalkMovement.y; }
        set { _currentWalkMovement.y = value; }
    }

    public float AppliedMovementY
    {
        get { return _appliedMovement.y; }
        set { _appliedMovement.y = value; }
    }

    #endregion GettersAndSetters


    [Space(1.0f)]
    //input values. should have a fold-out
    [Header("Display for Debug Values")]
    [SerializeField] private Vector2 _currentMovementInput;
    [SerializeField] private Vector3 _currentWalkMovement;
    [SerializeField] private Vector3 _currentRunMovement;
    [SerializeField] private Vector3 _appliedMovement;

    [SerializeField] private float _gravityWhileAirborne;

    [SerializeField] private bool _isMovementPressed;
    [SerializeField] private bool _isRunPressed;
    [SerializeField] private bool _isFalling;


    void Awake()
    {
        platformerPlayerInput = new PlatformerPlayerInputs();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();


        Debug.Log(string.Format("Setup Vars, _currentState: {0}", _currentState));

        _isWalkingHash = Animator.StringToHash("isWalking");
        _isRunningHash = Animator.StringToHash("isRunning");
        _isJumpingHash = Animator.StringToHash("isJumping");
        _jumpCountHash = Animator.StringToHash("jumpCount");

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

        //declare a new PS factory using this PS machine.
        _states = new PlayerStateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();

    }


    private void SetUpJumpVariables()
    {
        //calc all the jump velocities.

        _gravityWhileAirborne = (-2 * _maxJumpHeight) / Mathf.Pow(_jumpTimeToApex, 2);
        _initialJumpVelocity = -_gravityWhileAirborne * _jumpTimeToApex;

        float secondJumpGravity = (-1 * _secondJumpHeight) / Mathf.Pow(_secondJumpTimeToApex, 2);
        float secondJumpVelocity = (1 * _secondJumpHeight) / _secondJumpTimeToApex;

        float thirdJumpGravity = (-1 * _thirdJumpHeight) / Mathf.Pow(_thirdJumpTimeTimeToApex, 2);
        float thirdJumpVelocity = (1 * _thirdJumpHeight) / _thirdJumpTimeTimeToApex;

        //assign them to the dictionaries.
        _initialJumpVelocities.Add(1, _initialJumpVelocity);
        _initialJumpVelocities.Add(2, secondJumpVelocity);
        _initialJumpVelocities.Add(3, thirdJumpVelocity);

        _jumpGravityValues.Add(0, _gravityWhileAirborne);
        _jumpGravityValues.Add(1, _gravityWhileAirborne);
        _jumpGravityValues.Add(2, secondJumpGravity);
        _jumpGravityValues.Add(3, thirdJumpGravity);


        /*Debug.Log(string.Format("Setup Vars, _gravityWhileAirborne: {0}", _gravityWhileAirborne));
        Debug.Log(string.Format("Setup Vars, _initialJumpVelocity: {0}", _initialJumpVelocity));

        Debug.Log(string.Format("Setup Vars, secondJumpGravity: {0}", secondJumpGravity));
        Debug.Log(string.Format("Setup Vars, secondJumpVelocity: {0}", secondJumpVelocity));

        Debug.Log(string.Format("Setup Vars, thirdJumpGravity: {0}", thirdJumpGravity));
        Debug.Log(string.Format("Setup Vars, thirdJumpVelocity: {0}", thirdJumpVelocity));
        */
    }



    // Update is called once per frame
    void Update()
    {
        RotateCharacter();
        _currentState.UpdateState();
        _characterController.Move(_appliedMovement * Time.deltaTime);
    }


    private void RotateCharacter()
    {
        Vector3 positionToLookAt;
        //the change in position hte character should point to.
        positionToLookAt.x = _currentWalkMovement.x;
        positionToLookAt.y = 0;
        positionToLookAt.z = _currentWalkMovement.z;

        Quaternion currentRotation = transform.rotation;

        if (_isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactor * Time.deltaTime);
        }

    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        //set the walking speed. this will get fixed in the StateMachine version.
        _currentWalkMovement.x = _currentMovementInput.x * _walkSpeed;
        _currentWalkMovement.z = _currentMovementInput.y * _walkSpeed;

        //set the runnning speed. yeah, no shot this should be calculated every frame.
        _currentRunMovement.x = _currentMovementInput.x * _runSpeed;
        _currentRunMovement.z = _currentMovementInput.y * _runSpeed;

        //check if there is any input, and assign to bool.
        _isMovementPressed = _currentWalkMovement.x != 0 || _currentMovementInput.y != 0;
    }

    void OnRun(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();
        //Debug.Log(_isJumpPressed);
        _requireNewJumpPress = false;
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
