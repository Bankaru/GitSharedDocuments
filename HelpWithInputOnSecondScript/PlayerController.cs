using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour   //Class name must be the same as filename.
{

    #region Variables
    //Public Variables
    public Vector3 TargetPosition;


    //Serialized and Private Variables
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] Ball _ball;
    [SerializeField] Transform _ballPositionTransform;

    //declare reference variables
    private PlayerControls _playerControls;              //Asset Reference name must be the same as the C# Script produced from Input Asset. //AIMSCRIPT
    private CharacterController _characterController;    //Reference to Unity's Character Controller Component
    private Animator _animator;                          //Animator Component

    //variables to store player input values
    private Vector2 _currentMovementInput;
    private Vector3 _currentMovement;
    private Vector3 _currentRunMovement;
    private Vector2 _currentMouseInput;  //AIMSCRIPT
    private bool _currentStance;


    private bool _isMovementPressed;
    private bool _isRunPressed;
    private bool _inQuarterbackStance = true;
    private bool _isThrowing;
    private bool _isBallCarrier;                        //This is managed by methods.  If you add a fumble, make sure to update this.
    private float _rotationFactorPerFrame = 0.02f;
    private float _runMultiplier = 1.15f;                //If you want to have a run/sprint faster than normal movement.

    //Animator Hash variables to optimize animator parameter access.
    private int _isWalkingHash;                          //Boolean parameter for Animator                          
    private int _isRunningHash;                          //Boolean parameter for Animator            
    private int _speedHash;                              //Float parameter for Animator
    private int _isMovementPressedHash;                  //Boolean parameter for Animator

    //Mouse aiming variables
    private Vector3 _throwTarget;

    //Throwing variables
    private Vector3 _startPoint;
    private Vector3 _endPoint;
    private float _throwForce;
    private float _maxThrowTime;

    private float _speed = 4;

    #endregion



    void Awake()
    {
        //initially set reference variables
        _playerControls = new PlayerControls(); //AIMSCRIPT
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        //Assignment of Animator Hash variables to paramaters in the Animator, which are in "quotes".
        _isMovementPressedHash = Animator.StringToHash("isMovementPressed");
        _isWalkingHash = Animator.StringToHash("isWalking");
        _isRunningHash = Animator.StringToHash("isRunning");
        _speedHash = Animator.StringToHash("Speed_f");
    }

    private void Start()
    {
        _ball.GetPossession(_ballPositionTransform);
        _isBallCarrier = true;
    }
    private void OnRun(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }

    private void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _currentMovement.x = _currentMovementInput.x * _speed;
        _currentMovement.z = _currentMovementInput.y * _speed;
        _currentRunMovement.z = _currentMovementInput.y * _speed * _runMultiplier;
        _currentRunMovement.x = _currentMovementInput.x * _speed * _runMultiplier;
        _isMovementPressed = _currentMovementInput.x != 0 || _currentMovementInput.y != 0;
    }





    private void OnThrowStart(InputAction.CallbackContext context)
    {
        if (_inQuarterbackStance)
        {
            _isThrowing = true;
        }
    }
    void OnThrowEnd(InputAction.CallbackContext context)
    {

        if (_isThrowing)
        {
            _ball.ThrowBall(_throwTarget, 1, 1);
            _isThrowing = false;
            _isBallCarrier = false;
            _inQuarterbackStance = false;
        }
    }


    void ToggleStance(InputAction.CallbackContext context)
    {
        if (_isBallCarrier)
        {
            _currentStance = context.ReadValueAsButton();

            if (_inQuarterbackStance)
            {
                _inQuarterbackStance = false;
            }
            else
            {
                _inQuarterbackStance = true;
            }
            if (_ball != null)
            {
                _ball.ThrowingMode = _inQuarterbackStance;
            }
        }
    }



    private void HandleRotation()
    {
        Vector3 positionToLookAt;
        //the change in position our character should point to
        positionToLookAt.x = _currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = _currentMovement.z;
        //the current rotation of our character
        Quaternion currentRotation = transform.rotation;


        if (_isMovementPressed)
        {
            //creates a new rotation based on where the player is currently pressing
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame);
            //Slerp is spherical interpolation.  The closer the 3rd argument is to 1.0, the faster the rotation.
        }

    }

    private void HandleAnimation()
    {

        float speed = Mathf.Abs(_currentMovement.x + _currentMovement.z) * 2;
        _animator.SetFloat(_speedHash, speed);
        _animator.SetBool(_isMovementPressedHash, _isMovementPressed);

    }

    private void HandleGravity()
    {
        if (_characterController.isGrounded)
        {
            float groundedGravity = -.05f;
            _currentMovement.y = groundedGravity;
            _currentRunMovement.y = groundedGravity;
        }
        else
        {
            float gravity = -9.8f;
            _currentMovement.y += gravity;
            _currentRunMovement.y += gravity;
        }
    }

    //private void OnMouseAim(InputAction.CallbackContext context)  //AIMSCRIPT
    //{
    //    _currentMouseInput = context.ReadValue<Vector2>();
    //}

    //private void Aim() //AIMSCRIPT
    //{
    //    var (success, position) = GetMousePosition();
    //    if (success)
    //    {
    //        TargetPosition = position;
    //        Debug.Log("Target Position: " + TargetPosition);
    //        //Calculate the direction
    //        _throwTarget = TargetPosition - transform.position;
    //        //throwTarget.y = 1;
    //        transform.forward = _throwTarget;  //Can you smooth this so the player doesn't instantly change rotation?
    //        Debug.Log("Throw target = " + _throwTarget);
    //    }
    //}


    //private (bool success, Vector3 position) GetMousePosition() // AIMSCRIPT
    //{
    //    var ray = _mainCamera.ScreenPointToRay(_currentMouseInput);

    //    if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, _groundMask))
    //    {
    //        // The Raycast hit something, return the position.
    //        return (success: true, position: hitInfo.point);
    //    }
    //    else
    //    {
    //        // The Raycast did not hit anything.
    //        return (success: false, position: Vector3.zero);
    //    }
    //}

    // Update is called once per frame
    void Update()
    {
        HandleAnimation();
        if (!_inQuarterbackStance)
        {
            HandleRotation();
        }
        //else
        //{
        //    Aim();
        //}


        HandleGravity();
        if (!_inQuarterbackStance && _isRunPressed)
        {
            _characterController.Move(_currentRunMovement * Time.deltaTime);
        }
        else
        {
            _characterController.Move(_currentMovement * Time.deltaTime);
        }

    }

    void OnEnable()
    {
        _playerControls.Quarterback.Enable();
        //set the player input callbacks
        _playerControls.Quarterback.Move.started += OnMovementInput;
        _playerControls.Quarterback.Move.canceled += OnMovementInput;
        _playerControls.Quarterback.Move.performed += OnMovementInput;
        _playerControls.Quarterback.Run.started += OnRun;
        _playerControls.Quarterback.Run.canceled += OnRun;

        _playerControls.Quarterback.ToggleStance.started += ToggleStance;

        //_playerControls.Quarterback.Aim.started += OnMouseAim;  //AIMSCRIPT
        //_playerControls.Quarterback.Aim.canceled += OnMouseAim;  //AIMSCRIPT
        //_playerControls.Quarterback.Aim.performed += OnMouseAim; //AIMSCRIPT

        _playerControls.Quarterback.Throw.started += OnThrowStart;
        _playerControls.Quarterback.Throw.canceled += OnThrowEnd;

    }

    void OnDisable()
    {
        _playerControls.Quarterback.Disable();
        //Unsubscribe from the player callbacks to prevent Memory Leakage
        _playerControls.Quarterback.Move.started -= OnMovementInput;
        _playerControls.Quarterback.Move.canceled -= OnMovementInput;
        _playerControls.Quarterback.Move.performed -= OnMovementInput;
        _playerControls.Quarterback.Run.started -= OnRun;
        _playerControls.Quarterback.Run.canceled -= OnRun;

        _playerControls.Quarterback.ToggleStance.started -= ToggleStance;

        //_playerControls.Quarterback.Aim.started -= OnMouseAim; //AIMSCRIPT
        //_playerControls.Quarterback.Aim.canceled -= OnMouseAim; //AIMSCRIPT
        //_playerControls.Quarterback.Aim.performed -= OnMouseAim; //AIMSCRIPT

        _playerControls.Quarterback.Throw.started -= OnThrowStart;
        _playerControls.Quarterback.Throw.canceled -= OnThrowEnd;
    }
}
