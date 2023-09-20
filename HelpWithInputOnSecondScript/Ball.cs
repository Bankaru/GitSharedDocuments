using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class Ball : MonoBehaviour
{
/*  THANK YOU FOR REVIEWING THE CODE FOR ME.  MOUSE INPUT IS NOT REGISTERING AT ALL IN THIS SCRIPT.  THE SAME CODE WORKS IN THE PLAYERCONTROLLER SCRIPT.  I'M NOT SURE WHY IT WON'T WORK HERE...
    LINE 30, 44: InputAsset variable declared on line 30, instantiated in line 44.
    LINES 143-157: Callbacks subscribed and unsubscribed to in OnEnable/OnDisable methods in lines 143-157.
    LINES 34, 107-111: Mouse-Input holder variable declared in line 34.  Input context read and assigned to the variable in onMouseAim() lines 107-111.
    LINES 113-141: Methods for returning mouse position on the screen and adjusting player orientation to it.
    LINE 66: Aim() method based on booleans.  The booleans are met because the Start method makes ThrowingMode True, and the PlayerController's Start() method calls this script's GetPossession() method.
*/
    //Public Variables
    public bool PlayerBallPossession;
    public bool ThrowingMode;
    public Vector3 TargetPosition;

    //External Public and Serialized References
    [SerializeField] FootballGameManager _footballGameManager;
    //[SerializeField] Transform _quarterbackBallPosition;
    [SerializeField] GameObject _quarterback;
    [SerializeField] Camera _mainCamera;
    [SerializeField] private LayerMask _groundMask;

    //Component References
    private Rigidbody _projectileRb;
    private Transform _ballCarrier;
    PlayerControls _playerControls; //AIMSCRIPT


    //Private Variables
    private Vector2 _currentMouseInput; //AIMSCRIPT

    private Vector3 _throwTarget;

    private float _force;
    public float JourneyLength;
    public float StartTime;

    private void Awake()
    {
        _playerControls = new PlayerControls();
    }

    void Start()
    {
        //_ballCarrier = _quarterbackBallPosition; //Remove once you have a hiking system...  Or start

        PlayerBallPossession = true;
        _projectileRb = GetComponent<Rigidbody>();
        ThrowingMode = true; // delete or fix logic.


    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerBallPossession)
        {
            CarryBall(_ballCarrier);
            if (ThrowingMode)
            {
                Aim(); //AIMSCRIPT
            }
        }
    }

    public Transform GetPossession(Transform PlayerPossessionTransform)
    {
        PlayerBallPossession = true;
        _ballCarrier = PlayerPossessionTransform;
        return _ballCarrier;
    }

    private void CarryBall(Transform carryposition)
    {
        {
            _projectileRb.useGravity = false;
            _projectileRb.velocity = Vector3.zero;
            _projectileRb.angularVelocity = Vector3.zero;
            transform.SetParent(carryposition, worldPositionStays: true);
            transform.SetLocalPositionAndRotation(transform.localPosition = Vector3.zero, transform.localRotation = Quaternion.Euler(0f, 0f, 0f));
        }
    }


    public void ThrowBall(Vector3 throwDirection, float force, float archForce)
    {
        PlayerBallPossession = false;
        _projectileRb.useGravity = true;
        transform.SetParent(null);

        _projectileRb.AddForce((throwDirection * force) + (Vector3.up * archForce), ForceMode.Impulse);
        UnityEngine.Debug.Log("THROW DIRECTION, FORCE: " + (throwDirection * force + Vector3.up * archForce));
    }

    public void ProjectTrajectory(Vector3 startPos, Vector3 endPos, float chargeTime)
    {
        JourneyLength = Vector3.Distance(startPos, endPos);
        StartTime = Time.time;

    }
//  INPUT INFORMATION FOR BALL.
    void onMouseAim(InputAction.CallbackContext context) //AIMSCRIPT
    {
        _currentMouseInput = context.ReadValue<Vector2>();
        Debug.Log("CurrentMouseInput = " + _currentMouseInput);  //<<<<<<<THIS NEVER PRINTS TO SCREEN.  CONTEXT IS NEVER READ...
    }

    private void Aim() //AIMSCRIPT
    {
        var (success, position) = GetMousePosition();
        if (success)
        {
            TargetPosition = position;
            //Calculate the direction
            _throwTarget = TargetPosition - _quarterback.transform.position;
            //throwTarget.y = 1;
            _quarterback.transform.forward = _throwTarget;
        }
    }


    private (bool success, Vector3 position) GetMousePosition() //AIMSCRIPT
    {
        var ray = _mainCamera.ScreenPointToRay(_currentMouseInput);
        
        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, _groundMask))
        {
            // The Raycast hit something, return the position.
            return (success: true, position: hitInfo.point);
        }
        else
        {
            // The Raycast did not hit anything.
            return (success: false, position: Vector3.zero);
        }
    }

    private void OnEnable() //AIMSCRIPT
    {
        Debug.Log("Enabled");
        _playerControls.Quarterback.Aim.started += onMouseAim;
        _playerControls.Quarterback.Aim.canceled += onMouseAim;
        _playerControls.Quarterback.Aim.performed += onMouseAim;
    }

    private void OnDisable() //AIMSCRIPT
    {
        Debug.Log("Disabled");
        _playerControls.Quarterback.Aim.started -= onMouseAim;
        _playerControls.Quarterback.Aim.canceled -= onMouseAim;
        _playerControls.Quarterback.Aim.performed -= onMouseAim;
    }
    


}
