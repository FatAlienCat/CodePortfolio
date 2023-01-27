//-----------------------------------------------------------------------------------------------------
// FileName: PlayerController.cs
// Description: Player Input Controller which allows for seamless transistion between flying and hovering
// Author: Julian Beiboer
// Date: 14/01/2023
//-----------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Yarn.Unity;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    enum MovementModes { Hovercraft, Plane}
    [SerializeField]
    [ReadOnly]
    MovementModes _movementModes;

    [SerializeField]
    private HoverCraft _hoverCraft;
    [SerializeField]
    private SimpleFly _simpleFly;
    [SerializeField]
    private EngineRotationController _engineRotationController;
    [SerializeField]
    private PlaneEffectsController _planeEffectsController;
    [SerializeField]
    private Rigidbody _rb;
    [SerializeField]
    private int _playerId = 0;
    [SerializeField]
    private float _hoverCraftSwitchThresholdSpeed;
    [SerializeField]
    private float _mouseDeadZone = 0.2f;

    private Player _rewiredPlayer;
    [SerializeField]
    private bool _enablePlane = true;
    private MovementModes _previousMode;
    [SerializeField]
    public UnityEvent PlaneModeTriggered;
    [SerializeField]
    public UnityEvent HoverModeTriggered;
    private void Start()
    {
        _rewiredPlayer = ReInput.players.GetPlayer(_playerId);
    }
    private void Update()
    {
        if (_enablePlane)
        {

            if(_rewiredPlayer.GetAxisRaw("Thrust") > 0.1f)
            {
                _movementModes = MovementModes.Plane;
                _simpleFly.PlaneThrustSystem(_rewiredPlayer.GetAxisRaw("Thrust"));
                //_simpleFly.PlaneEnabled = true;
                SimpleFlyInputs();
                _rb.useGravity = false;
            }
            else
            {
                //_simpleFly.PlaneEnabled = false;
                _movementModes = MovementModes.Hovercraft;
                //_simpleFly.DeactivatePlaneSystem();
                HoverCraftInputs();
                
                //_rb.useGravity = true;
            }
            if(_movementModes != _previousMode)
            {
                // trigger
                _previousMode = _movementModes;

                if(_movementModes == MovementModes.Plane)
                {
                    PlaneModeTriggered.Invoke();
                }
                else
                {
                    HoverModeTriggered.Invoke();
                }
            }
        }
        
    }

    void SimpleFlyInputs()
    {
        _simpleFly.PlaneThrustSystem(_rewiredPlayer.GetAxisRaw("Thrust"));
        _engineRotationController.PlaneMode(_rewiredPlayer.GetAxisRaw("Thrust"));
        // turns off hover particles
        _engineRotationController.DisableHoverParticles();
        _simpleFly.TiltPlaneBasedOnMovement();

    }

    void HoverCraftInputs()
    {
        _hoverCraft.CheckIfHeightLocked();
        _hoverCraft.Lift(_rewiredPlayer.GetAxis("Lift"));
        _hoverCraft.Strafe(_rewiredPlayer.GetAxis("StrafeMovement"));
        _hoverCraft.Drive(_rewiredPlayer.GetAxis("ForwardMovement"));

        Vector2 lookInput = Vector2.zero;
        if (HelperScripts.RewiredControllerHelper.GetJoyStickUsed(_rewiredPlayer) == "Mouse"|| HelperScripts.RewiredControllerHelper.GetJoyStickUsed(_rewiredPlayer) == "Keyboard")
        {
            lookInput = HelperScripts.CursorHelper.MouseInput(_playerId, false, _mouseDeadZone);

        }
        else
        {
            lookInput = new Vector2(_rewiredPlayer.GetAxis("CameraHorizontal"), _rewiredPlayer.GetAxis("CameraVertical"));
        }
        _hoverCraft.TurnPlane(lookInput);
        _hoverCraft.TiltPlaneBasedOnMovement(_rewiredPlayer.GetAxis("StrafeMovement"), _rewiredPlayer.GetAxis("ForwardMovement"), lookInput);
        _engineRotationController.HoverCraftMode(_rewiredPlayer.GetAxis("StrafeMovement"), _rewiredPlayer.GetAxis("ForwardMovement"), lookInput);


        _hoverCraft.ToggleHeightLocked(_rewiredPlayer.GetButtonDown("LockAltitude"));
        _engineRotationController.HeightLocked = _hoverCraft.HeightLocked;
        _engineRotationController.ToggleEngineParticles(_rewiredPlayer.GetAxis("Lift"));

    }


    [YarnCommand("TogglePlane")]
    void EnablePlane(bool value)
    {
        _enablePlane = value;
    }
    
}
