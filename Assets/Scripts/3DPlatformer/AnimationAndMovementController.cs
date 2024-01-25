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
    private bool _isJumpAnimating;
    [SerializeField] private int _jumpCount = 0;

    private Dictionary<int, float> _initialJumpVelocities = new Dictionary<int, float>();
    private Dictionary<int, float> _jumpGravityValues = new Dictionary<int, float>();
    private Coroutine _currentResetJumpRoutine = null;

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


        Debug.Log(string.Format("Setup Vars, _gravityWhileAirborne: {0}", _gravityWhileAirborne));
        Debug.Log(string.Format("Setup Vars, _initialJumpVelocity: {0}", _initialJumpVelocity));

        Debug.Log(string.Format("Setup Vars, secondJumpGravity: {0}", secondJumpGravity));
        Debug.Log(string.Format("Setup Vars, secondJumpVelocity: {0}", secondJumpVelocity));

        Debug.Log(string.Format("Setup Vars, thirdJumpGravity: {0}", thirdJumpGravity));
        Debug.Log(string.Format("Setup Vars, thirdJumpVelocity: {0}", thirdJumpVelocity));

    }

    public void PerformJump() 
    {
        //Debug.Log(string.Format("Perform jump, _isJumpPressed: {0}", _isJumpPressed));
        //check input and grounded state, to jump 
        if (!_isJumping && _characterController.isGrounded && _isJumpPressed)
        {
            if (_jumpCount < 3 && _currentResetJumpRoutine != null)
            {
                StopCoroutine(_currentResetJumpRoutine);
            }


            _animator.SetBool(_isJumpingHash, true);
            _isJumping = true;
            _isJumpAnimating = true;
            _jumpCount += 1;
            _animator.SetInteger(_jumpCountHash, _jumpCount);

            Debug.Log(string.Format("Perform jump, _jumpCount: {0}", _jumpCount));

            //Debug.Log(string.Format("Perform jump, apply y velocity: {0}", _initialJumpVelocity));

            //the video multiplies these by 0.5f. but that seems very odd. means that maxjumpheight becomes incorrect.
            _currentWalkMovement.y = _initialJumpVelocities[_jumpCount];
            _appliedMovement.y = _initialJumpVelocities[_jumpCount];
            Debug.Log(string.Format("Perform jump, walk y velocity: {0}", _currentWalkMovement.y));
            
        }
        //check input and grounded state, to set that it is not jumping.
        else if (_isJumping && _characterController.isGrounded && !_isJumpPressed) 
        {
            _isJumping = false;
        }
    }

    private IEnumerator ResetJumpCount() 
    {
        yield return new WaitForSeconds(_TimeToResetJumpCount);
        _jumpCount = 0;
        Debug.Log(string.Format("RestJumpCount: {0}", _jumpCount));
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
        positionToLookAt.x = _currentWalkMovement.x;
        positionToLookAt.y = 0;
        positionToLookAt.z = _currentWalkMovement.z;

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
        if (_currentWalkMovement.y < -0.5f || !_isJumpPressed)
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
                //reset jump count after time
                _currentResetJumpRoutine = StartCoroutine(ResetJumpCount());

                //reset jump count if the player has performed a triple jump
                if (_jumpCount == 3) 
                {
                    _jumpCount = 0;
                    _animator.SetInteger(_jumpCountHash, _jumpCount);
                }

            }
            _currentWalkMovement.y = _gravityWhenGrounded;
            _appliedMovement.y = _gravityWhenGrounded;
            _isFalling = false;
            
        }
        else if (_isFalling) 
        {
            float previousYVelocity = _currentWalkMovement.y;
            //Verlet
            _currentWalkMovement.y = _currentWalkMovement.y + (_jumpGravityValues[_jumpCount] * _gravityFallMultiplier * Time.deltaTime);
            _appliedMovement.y = Mathf.Max((previousYVelocity + _currentWalkMovement.y) * 0.5f, -20.0f);

        }

        else
        {
            float previousYVelocity = _currentWalkMovement.y;

            //add the old velocity and the new velocity, average them and set that value.
            //called VelocityVerlet
            _currentWalkMovement.y = _currentWalkMovement.y + (_jumpGravityValues[_jumpCount] * Time.deltaTime);
            _appliedMovement.y = (previousYVelocity + _currentWalkMovement.y) * 0.5f;
        } 
    }

    // Update is called once per frame
    void Update()
    {
        ChangeAnimation();
        RotateCharacter();
        

        if (_isRunPressed)
        {
            _appliedMovement.x = _currentRunMovement.x;
            _appliedMovement.z = _currentRunMovement.z;
            //Debug.Log(string.Format("running, y velocity: {0}", currentWalkMovement.y));
        }
        else
        {
            //Debug.Log(string.Format("walking, y velocity: {0}", currentWalkMovement.y));
            _appliedMovement.x = _currentWalkMovement.x;
            _appliedMovement.z = _currentWalkMovement.z;
        }

        _characterController.Move(_appliedMovement * Time.deltaTime);

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
