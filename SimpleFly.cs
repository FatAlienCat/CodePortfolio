//-----------------------------------------------------------------------------------------------------
// FileName: SimpleFly.cs
// Description: Plane movement systems. Designed to be more arcade than simulation. Has a varied of features including acceleration based of whether the plane is diving or climbing. Allow for full customisation of movement.
// Currently working on refactoring to make code more readable. 
// Author: Julian Beiboer
// Date: 10/09/2022
//-----------------------------------------------------------------------------------------------------
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Rewired;

public class SimpleFly : MonoBehaviour
{
    #region Variables
    public int PlayerId = 0;

    [SerializeField]
    private bool _mouseEnabled = false;

    [Header("Lock Variables")]
    public bool PlaneEnabled = true;
    public bool PitchEnabled = true;

    [Header("Movement Variables")]
    [SerializeField]
    private float _maxSpeed;// for camera focal pull
    [SerializeField]
    private float _maxThrust = 25f, _minThrust, _boostThrust = 50f;
    [SerializeField]
    private float _thrustDecceleration = 2.5f, _gravityDecceleration = 2f;
    [SerializeField]
    private float _thrustAcceleration = 2.5f, _gravityAcceleration = 2.5f;
    private float _activeForwardSpeed;

    [Header("Boost Variables")]
    public bool boostOn;
    [SerializeField]
    private int _boostLimit, _boostReductionAmount, _boostIncreaseAmount;
    public int boostNumber = 0; // stored boost tracker

    [Header("Steering Variables")]
    //[SerializeField]
    
    //public float lookRateSpeed = 90f;
    [SerializeField]
    private float _lookRateSpeedJoyX = 45f;
    [SerializeField]
    private float _lookRateSpeedJoyY = 100f;
    [SerializeField]
    private float _levelSpeedY = 0.5f, _turnSpeedMult = 3;
    [SerializeField]
    private Vector2 _minMaxRotateX = new Vector2(-70, 70);
    //Hidden
    private float turnAmountPitch = 0; // Used for animating plane pitch on accent and decent 

    [Header("Landing Variables")]
    [SerializeField]
    private float _distToGround = 3f;
    [SerializeField]
    private LayerMask _isGround;
    //Hidden
    private bool _nearGround;

    [Header("Roll/Pitch On Turn")]
    [SerializeField]
    private float _pitchCameraCorrectionSpeed = 1f;
    [SerializeField]
    private float _pitchMaxAngle = 30.0f, _pitchLevelSpeedX = 9.0f, _pitchSpeedMult = 2.0f;
    [SerializeField]
    private float _rollMaxAngle = 90.0f, _rollLevelSpeedZ = 3.0f, _rollSpeedMult = 2.0f;
    [SerializeField]
    private GameObject _pivot, _pitchPivot, _pitchLevelOutTarget;// animations based on movement
    // Roll and Pitch amounts
    float turnAmountZ = 0;
    float turnAmountX = 0;

    [Header("Altitude")]
    [SerializeField]
    private Transform _lowestPoint;// lowest point of plane
    public int AltitudeLimit = 3000;
    private float _altitudeLastTick = 0;

    [Header("Camera")]
    private Camera _mainCamera;
    [SerializeField]
    private float _fovAdjustmentSpeed = 1;
    [SerializeField]
    private Vector2 _fovMinMax = new Vector2(60, 100);

    [Header("Events")]
    [SerializeField]
    private UnityEvent _boostStart;
    [SerializeField]
    private UnityEvent _boostEnd;

    [Header("Gravity Speed Adjustments")]
    private float _gravityAdjustment = 0;
    [SerializeField]
    private float _gravityAdjusterPos = 4;
    [SerializeField]
    private float _gravityAdjusterThresehold = 0.25f;

    #region Hidden Variables
    private Player _rewiredPlayer;
    private string _joyStickType;
    private Rigidbody _rb;
    Vector3 _rotateAround;
    Vector3 _rotateToAnim;
    int _altitude; // variable updated to check altitude
    private Vector2 _lookInput; // jpystick input
    private GameStateData _gameStateData;
    Vector3 pitchRotateAround;

    #endregion
    #endregion

    void Start()
    {
        _rewiredPlayer = ReInput.players.GetPlayer(PlayerId);
        //gameStateData = FindObjectOfType<GameStateData>();
        _rb = GetComponent<Rigidbody>();
        _mainCamera = FindObjectOfType<Camera>();

#if !UNITY_EDITOR
       // mouseEnabled = true;
#endif

    }
    void Update()
    {
        //Check Input Type
        _joyStickType = HelperScripts.RewiredControllerHelper.GetJoyStickUsed(_rewiredPlayer);
        // Set altitude
        _altitude = (int)_lowestPoint.position.y;

        AdjustCameraFieldOfView(); // Adjusts focal lenght based on speed;



    }

    public void EnablePlane()
    {
        Debug.Log("enable");
        PlaneEnabled = true;
    }
    public void DisablePlane()
    {
        Debug.Log("disnable");
        PlaneEnabled = false;
        _activeForwardSpeed = 0;
    }

    internal void PlaneThrustSystem(float inputThrust)
    {
        _activeForwardSpeed = Thrust(_activeForwardSpeed, inputThrust);

        _activeForwardSpeed = ThrustGravityAdjustments(_activeForwardSpeed, inputThrust);

        //_activeForwardSpeed = BoostSystem(_activeForwardSpeed, inputThrust); // boost system needs rework
    }
    internal void DeactivatePlaneSystem()
    {
        _activeForwardSpeed = 0;
    }

    private float BoostSystem(float currentForwardSpeed)
    {
        // Invoke Boost effects
        if (_rb.velocity.magnitude > 150)
        {
            _boostStart.Invoke();
        }
        if (_rb.velocity.magnitude < 150)
        {
            _boostEnd.Invoke();
        }
        if (boostOn)
        {
            if (_rewiredPlayer.GetButton("Boost") && _boostLimit > 0)
            {
                currentForwardSpeed = Mathf.Lerp(currentForwardSpeed, _rewiredPlayer.GetAxisRaw("Thrust") * _boostThrust, _thrustAcceleration * 2 * Time.deltaTime);
                _boostLimit -= _boostReductionAmount;
            }
        }
        return currentForwardSpeed;
    }

    private float ThrustGravityAdjustments(float currentForwardSpeed, float inputThrust)
    {
        // Add extra speed when plane is diving.
        float differenceInAlt = (_altitudeLastTick - transform.position.y);
        differenceInAlt = Mathf.Clamp(differenceInAlt, -1f, 1f);
        if (differenceInAlt < _gravityAdjusterThresehold)
        {
            _gravityAdjustment = Mathf.Lerp(_gravityAdjustment, 0, _gravityDecceleration * Time.deltaTime);

        }
        else if (differenceInAlt > _gravityAdjusterThresehold)
        {
            _gravityAdjustment = Mathf.Lerp(_gravityAdjustment, _gravityAdjusterPos * differenceInAlt, _gravityAcceleration * Time.deltaTime);
        }
        if (currentForwardSpeed > 0 &&
            inputThrust > 0.1f && 
            _altitude < AltitudeLimit)
        {
            currentForwardSpeed += _gravityAdjustment;
        }
        _altitudeLastTick = transform.position.y;

        return currentForwardSpeed;
    }

    private float Thrust(float currentForwardSpeed, float inputThrust)
    {
        if (inputThrust > 0.1) // If thrust is down and less than altitude limit
        {
            if (boostNumber > 0)//Boost 
            {
                currentForwardSpeed = Mathf.Lerp(currentForwardSpeed, inputThrust * _boostThrust, _thrustAcceleration * 2 * Time.deltaTime);
            }

            else
            {
                currentForwardSpeed = Mathf.Lerp(currentForwardSpeed, inputThrust * _maxThrust, _thrustAcceleration * Time.deltaTime);
            }
        }

        else
        {
            currentForwardSpeed = Mathf.Lerp(currentForwardSpeed, _minThrust, _thrustDecceleration * Time.deltaTime);
        }
        return currentForwardSpeed;
    }

    void FixedUpdate()
    {
        if (PlaneEnabled)
        {
            AddForceToPlane();

            TurnPlane();
        }

    }

    private void TurnPlane()
    {
        // Turn Plane
        if (HelperScripts.RewiredControllerHelper.GetJoyStickUsed(_rewiredPlayer) == "Keyboard") //Smoothes out keyboard input to replicate joystick
        {
            turnAmountPitch = IncrementTurnAmount(turnAmountPitch, _turnSpeedMult, _lookInput.y); // needs to be for keyboard only
        }
        else
        {
            turnAmountPitch = _lookInput.y;
        }

        if (_altitude > AltitudeLimit)//Block pitch up when altitude limit exceeded.
        {
            if (turnAmountPitch < 0)
            {
                turnAmountPitch = 0;
            }
        }
        // Turn Plane
        transform.Rotate(0f, _lookInput.x * _lookRateSpeedJoyX * Time.deltaTime, 0f, Space.Self);

        _rotateAround = HelperScripts.RotationHelper.GetSignedEulerAngles(transform.rotation.eulerAngles);
        //Levels out the pitch automatically
        _rotateAround.z = 0;// Gimbal lock
        transform.rotation = Quaternion.Euler(_rotateAround);

        _pitchPivot.transform.Rotate(_lookInput.y * _lookRateSpeedJoyY * Time.deltaTime, 0f, 0f, Space.Self);
        //Loop the loop system

        if (turnAmountPitch >= -0.5f && _pitchPivot.transform.eulerAngles.z != 0)
        {
            HelperScripts.RotationHelper.RotateTowards(_pitchLevelOutTarget.transform.position, _pitchPivot.transform, _pitchCameraCorrectionSpeed);
            pitchRotateAround = HelperScripts.RotationHelper.GetSignedEulerAngles(_pitchPivot.transform.eulerAngles);
            //Levels out the pitch automatically
            pitchRotateAround.x = Mathf.Lerp(pitchRotateAround.x, turnAmountPitch * _lookRateSpeedJoyY, _levelSpeedY * Time.deltaTime);
            pitchRotateAround.x = Mathf.Clamp(pitchRotateAround.x, _minMaxRotateX.x, _minMaxRotateX.y); // clamps rotation between min and max values
            _pitchPivot.transform.rotation = Quaternion.Euler(pitchRotateAround);
        }
    }

    void AddForceToPlane()
    {
        _rb.AddForce(_pitchPivot.transform.forward * _activeForwardSpeed, ForceMode.Force);
    }
  
   
   
    void ResetPlaneRotations()//reset Pivot rotation on controller input change.
    {
        _pivot.transform.localRotation = Quaternion.Euler(Vector3.zero);
        _pitchPivot.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }

    public void TiltPlaneBasedOnMovement()
    {
        _lookInput.x = _rewiredPlayer.GetAxis("LookHor");
        _lookInput.y = _rewiredPlayer.GetAxis("LookVert");

        //If Mouse being used override input
        if (_joyStickType == "Mouse" && _mouseEnabled)
        {
            _lookInput = HelperScripts.CursorHelper.MouseInput(PlayerId, _correctInversion, _mouseDeadZone);
        }
        //Anim
        if (HelperScripts.RewiredControllerHelper.GetJoyStickUsed(_rewiredPlayer) == "Keyboard")
        {
            turnAmountZ = IncrementTurnAmount(turnAmountZ, _rollSpeedMult, _lookInput.x); // roll
            turnAmountX = IncrementTurnAmount(turnAmountX, _pitchSpeedMult, _lookInput.y); // pitch
        }
        else
        {
            turnAmountZ = _lookInput.x;
            turnAmountX = _lookInput.y;
        }
        if (_nearGround) // when ground lock roll 
        {
            turnAmountZ = 0;
        }
        if (_altitude > AltitudeLimit) //Block pitch up when altitude limit exceeded.
        {
            if(turnAmountX < 0)
            {
                turnAmountX = 0;
            }
        }

        _rotateToAnim = HelperScripts.RotationHelper.GetSignedEulerAngles(_pivot.transform.localRotation.eulerAngles);
        _rotateToAnim.z = Mathf.Lerp(_rotateToAnim.z, -_rollMaxAngle * turnAmountZ, _rollLevelSpeedZ * Time.deltaTime);
        _rotateToAnim.x = Mathf.Lerp(_rotateToAnim.x, _pitchMaxAngle * turnAmountX, _pitchLevelSpeedX * Time.deltaTime);
        _pivot.transform.localRotation = Quaternion.Euler(_rotateToAnim);
    }


    private float IncrementTurnAmount(float turnValue, float turnMultiplier, float inputValue)
    {
        //Increments input global variables smoothly based off input from controls axis
        if (inputValue > 0)
        {
            turnValue += 0.01f * turnMultiplier;

        }
        if (inputValue < 0)
        {
            turnValue -= 0.01f * turnMultiplier;
        }
        if (inputValue == 0)
        {
            if (turnValue < 0)
            {
                turnValue += 0.01f * turnMultiplier;
            }
            if (turnValue > 0)
            {
                turnValue -= 0.01f * turnMultiplier;
            }
        }
        turnValue = Mathf.Clamp(turnValue, -1, 1);
        return turnValue;
    }

   
    public bool GroundCheck(float distToGround)
    {
        RaycastHit hit;
        float _slopeAngle;
        bool isGrounded = false;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, distToGround + 0.1f, _isGround))
        {
            _slopeAngle = (Vector3.Angle(hit.normal, transform.forward) - 90);
            //Debug.Log("Grounded on " + hit.transform.name + "\nSlope Angle: " + _slopeAngle.ToString("N0") + "�");
            isGrounded = true;
        }
        return isGrounded;
    }
    public float DistanceToGround()
    {
        RaycastHit hit;
        float _slopeAngle;
        float _distance = 1000;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1000, _isGround))
        {
            _slopeAngle = (Vector3.Angle(hit.normal, transform.forward) - 90);
            _distance = hit.distance;
        }
        return _distance;
    }

    void AdjustCameraFieldOfView()
    {
        // Adjust feild of veiw based on speed
        _mainCamera.fieldOfView = Mathf.Lerp(_fovMinMax.x, _fovMinMax.y, Mathf.Clamp(_rb.velocity.magnitude, 0, _maxSpeed) / _maxSpeed);
    }

    public void RingBoost(Vector3 direction)
    {
        _activeForwardSpeed = Mathf.Lerp(_activeForwardSpeed, _rewiredPlayer.GetAxisRaw("Thrust") * _boostThrust, _thrustAcceleration * 2 * Time.deltaTime);
        if (boostNumber == 0)
        {
            boostNumber++;
            StartCoroutine(DisableBoost(Time.deltaTime));
        }
        else boostNumber++;
    }
    
    IEnumerator DisableBoost(float sec)
    {
        while (boostNumber > 0)
        {
            yield return new WaitForSeconds(sec);
            boostNumber--;
        }
        
    }
 
}



