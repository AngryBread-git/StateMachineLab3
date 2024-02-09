using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    public PlatformerPlayerInputs platformerPlayerInput;
    private CharacterController _characterController;
    private Animator _animator;
    private Camera _playerCamera;


    //animation refs
    private int _isWalkingHash;
    private int _isRunningHash;
    private int _isJumpingHash;
    private int _jumpCountHash;
    private int _isFallingHash;

    [Header("Player Control Variables")]
    [Range(4, 8)] [SerializeField] private float _rotationFactor = 0.6f;
    [SerializeField] private float _walkSpeed = 1.5f;
    [SerializeField] private float _runSpeed = 3.0f;

    [SerializeField] private float _gravity = -9.82f;
    [SerializeField] private float _gravityFallMultiplier = 1.5f;
    [SerializeField] private float _maxFallSpeed = -20.0f;
    [SerializeField] private float _wallSlideSpeed = -4.0f;

    [Space(1.0f)]
    [Header("Jump Variables")]
    [SerializeField] private float _firstJumpHeight = 1.0f;
    [SerializeField] private float _secondJumpHeight = 1.25f;
    [SerializeField] private float _thirdJumpHeight = 1.5f;

    [SerializeField] private float _firstjumpTimeToApex = 0.25f;
    [SerializeField] private float _secondJumpTimeToApex = 0.35f;
    [SerializeField] private float _thirdJumpTimeTimeToApex = 0.45f;

    [SerializeField] private float _timeToResetJumpCount = 0.5f;
    [SerializeField] private float _coyoteTime = 0.5f;
    private Coroutine _currentCoyoteTimeRoutine = null;

    //jumping properties.
    private bool _isJumpPressed;
    private float _firstJumpVelocity;
    private bool _isJumping;
    private bool _requireNewJumpPress;
    private int _jumpCount = 0;

    [SerializeField] private bool _isOnWall;

    private Dictionary<int, float> _initialJumpVelocities = new Dictionary<int, float>();
    private Dictionary<int, float> _jumpGravityValues = new Dictionary<int, float>();
    private Coroutine _currentResetJumpRoutine = null;

    //state vriables
    private PlayerBaseState _currentState;
    private PlayerStateFactory _states;



    [Space(1.0f)]
    //input values. should have a fold-out
    [Header("Display for Debug Values")]
    [SerializeField] private Vector2 _currentMovementInput;
    [SerializeField] private Vector3 _currentWalkMovement;
    [SerializeField] private Vector3 _currentRunMovement;
    [SerializeField] private Vector3 _appliedMovement;
    private Vector3 _cameraRelativeAppliedMovement;

    [SerializeField] private float _firstJumpGravity;

    [SerializeField] private bool _isMovementPressed;
    [SerializeField] private bool _isRunPressed;
    [SerializeField] private bool _isFalling;

    float EPSILON_SQR = 0.0001f;

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

    public Coroutine CurrentCoyoteTimeRoutine
    {
        get { return _currentCoyoteTimeRoutine; }
        set { _currentCoyoteTimeRoutine = value; }
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

    public float MaxFallSpeed
    {
        get { return _maxFallSpeed; }
    }

    public float WallSlideSpeed
    {
        get { return _wallSlideSpeed; }
    }

    public int JumpCount 
    {
        get { return _jumpCount; }
        set { _jumpCount = value; }
    }

    public bool IsOnWall 
    {
        get { return _isOnWall; }
        set { _isOnWall = value; }
    }

    public int IsWalkingHash
    {
        get { return _isWalkingHash; }
    }

    public int IsRunningHash
    {
        get { return _isRunningHash; }
    }

    public int IsJumpingHash
    {
        get { return _isJumpingHash; }
    }

    public int IsFallingHash
    {
        get { return _isFallingHash; }
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
        get { return _timeToResetJumpCount; }
    }

    public float CoyoteTime
    {
        get { return _coyoteTime; }
    }

    public float Gravity
    {
        get { return _gravity; }
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

    public float AppliedMovementX
    {
        get { return _appliedMovement.x; }
        set { _appliedMovement.x = value; }
    }

    public float AppliedMovementZ
    {
        get { return _appliedMovement.z; }
        set { _appliedMovement.z = value; }
    }

    public float RunSpeed 
    {
        get { return _runSpeed; }
    }

    public Vector2 CurrentMovementInput 
    {
        get {return _currentMovementInput;}
    }

    public bool IsMovementPressed 
    {
        get { return _isMovementPressed; }
    }

    public bool IsRunPressed
    {
        get { return _isRunPressed; }
    }

    #endregion GettersAndSetters


    private void Start()
    {
        _characterController.Move(_appliedMovement * Time.deltaTime);
    }

    void Awake()
    {
        platformerPlayerInput = new PlatformerPlayerInputs();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _playerCamera = Camera.main;

        Debug.Log(string.Format("Setup Vars, _currentState: {0}", _currentState));

        _isWalkingHash = Animator.StringToHash("isWalking");
        _isRunningHash = Animator.StringToHash("isRunning");
        _isJumpingHash = Animator.StringToHash("isJumping");
        _jumpCountHash = Animator.StringToHash("jumpCount");
        _isFallingHash = Animator.StringToHash("isFalling");

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

        _firstJumpGravity = (-2 * _firstJumpHeight) / Mathf.Pow(_firstjumpTimeToApex, 2);
        _firstJumpVelocity = -_firstJumpGravity * _firstjumpTimeToApex;

        float secondJumpGravity = (-2 * _secondJumpHeight) / Mathf.Pow(_secondJumpTimeToApex, 2);
        float secondJumpVelocity = (2 * _secondJumpHeight) / _secondJumpTimeToApex;

        float thirdJumpGravity = (-2 * _thirdJumpHeight) / Mathf.Pow(_thirdJumpTimeTimeToApex, 2);
        float thirdJumpVelocity = (2 * _thirdJumpHeight) / _thirdJumpTimeTimeToApex;

        //assign them to the dictionaries.
        _initialJumpVelocities.Add(1, _firstJumpVelocity);
        _initialJumpVelocities.Add(2, secondJumpVelocity);
        _initialJumpVelocities.Add(3, thirdJumpVelocity);

        _jumpGravityValues.Add(0, _firstJumpGravity);
        _jumpGravityValues.Add(1, _firstJumpGravity);
        _jumpGravityValues.Add(2, secondJumpGravity);
        _jumpGravityValues.Add(3, thirdJumpGravity);


        //Debug.Log(string.Format("Setup Vars, _gravityWhileAirborne: {0}", _gravityWhileAirborne));
        Debug.Log(string.Format("Setup Vars, _initialJumpVelocity: {0}", _firstJumpVelocity));

        //Debug.Log(string.Format("Setup Vars, secondJumpGravity: {0}", secondJumpGravity));
        Debug.Log(string.Format("Setup Vars, secondJumpVelocity: {0}", secondJumpVelocity));

        //Debug.Log(string.Format("Setup Vars, thirdJumpGravity: {0}", thirdJumpGravity));
        Debug.Log(string.Format("Setup Vars, thirdJumpVelocity: {0}", thirdJumpVelocity));
        
    }



    // Update is called once per frame
    void Update()
    {
        RotateCharacter();
        //run the current states update. and it's substates update.
        _currentState.UpdateStates();


        _cameraRelativeAppliedMovement = ConverToCameraSpace(_appliedMovement);
        _characterController.Move(_cameraRelativeAppliedMovement * Time.deltaTime);
    }

    Vector3 ConverToCameraSpace(Vector3 vectorToRotate) 
    {
        float currentYValue = vectorToRotate.y;

        Vector3 cameraForward = _playerCamera.transform.forward;
        Vector3 cameraRight = _playerCamera.transform.right;

        //remove y values to ignore upward/downward angles.
        cameraForward.y = 0;
        cameraRight.y = 0;

        //re-normalize so they have a value of 1.
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        //rotate the x and z values to camera space.
        Vector3 cameraForwardZProduct = vectorToRotate.z * cameraForward;
        Vector3 cameraRightZProduct = vectorToRotate.x * cameraRight;

        Vector3 result = cameraForwardZProduct + cameraRightZProduct;
        result.y = currentYValue;

        //return the product of the rotated Vector3s and the saved Y-value
        return result;
    }

    private void RotateCharacter()
    {
        //the change in position the character should point to.
        Vector3 positionToLookAt = _cameraRelativeAppliedMovement;
        positionToLookAt.y = 0;
        positionToLookAt.Normalize();

        Quaternion currentRotation = transform.rotation;

        if (_isMovementPressed)
        {
            if (positionToLookAt.sqrMagnitude <= 0) 
            {
                return;
            }
          
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, _rotationFactor * Time.deltaTime);
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


    private void OnTriggerStay(Collider other)
    {
        //CharacterController.attachedRigidbody
    }


    /*private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.CompareTag("JumpWall")) 
        {
            Debug.Log(string.Format("Hit Jump wall"));
            _isOnWall = true;
        }
    }
    */

    

    private void OnEnable()
    {
        platformerPlayerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        platformerPlayerInput.CharacterControls.Disable();
    }
}
