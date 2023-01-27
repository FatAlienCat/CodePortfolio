//-----------------------------------------------------------------------------------------------------
// FileName: HoverCraft.cs
// Description: Hovercraft movement functions, these functions are called from a PlayerController.cs to allow for intergrations with different movement types.
// Author: Julian Beiboer
// Date: 07/01/2023
//-----------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using System;

public class HoverCraft : MonoBehaviour
{
    #region Variables
    private Player _rewiredPlayer;
    [SerializeField]
    private int _playerId = 0;
    [SerializeField]
    private bool _mouseEnabled = true;
    [SerializeField]
    private Rigidbody _rb;
    [SerializeField]
    private Transform _pivot;

    [Header("Movement")]
    [SerializeField]
    private float _liftMultiplier;
    [SerializeField]
    private float _hoverMultiplier, _hoverAcceleration;
    [SerializeField]
    private float _turnMultipliers;

    [Header("Roll/Pitch")]
    // Roll and Pitch amounts
    float _rollAmount = 0;
    float _pitchAmount = 0;
    public float RollIncrementRate;
    public float PitchIncrementRate;
    private Vector3 _rotateToAnim;
    public int RollMaxAngle;
    public float PitchMaxAngle;
    public float RollSpeed;
    public float PitchSpeed;
    public Transform PivotPoint;
    [SerializeField]
    private float _mouseDeadZone = 0.2f;

    [SerializeField]
    private float _currentForwardSpeed;
    [SerializeField]
    private float _currentStrafeSpeed;
    [SerializeField]
    private float _currentLiftSpeed;
    [SerializeField]
    public bool HeightLocked;


    #endregion
    private void Start()
    {
        _rewiredPlayer = ReInput.players.GetPlayer(_playerId);
    }

    internal void TurnPlane(Vector2 lookInput)
    {
        // Turn Plane
        transform.Rotate(0f, lookInput.x * _turnMultipliers * Time.deltaTime, 0f, Space.Self);
    }

    internal void Lift(float axisValue)
    {
        _currentLiftSpeed = Mathf.Lerp(_currentLiftSpeed, axisValue * _liftMultiplier, _hoverAcceleration * Time.deltaTime);
        Vector3 force = _pivot.up * _currentLiftSpeed;
        Thrust(force);
    }

    internal void Drive(float axisValue)
    {
        _currentForwardSpeed = Mathf.Lerp(_currentForwardSpeed, axisValue * _hoverMultiplier, _hoverAcceleration * Time.deltaTime);
        Vector3 force = _pivot.forward * _currentForwardSpeed;
        Thrust(force);
    }

    internal void Strafe(float axisValue)
    {
        _currentStrafeSpeed = Mathf.Lerp(_currentStrafeSpeed, axisValue * _hoverMultiplier, _hoverAcceleration * Time.deltaTime);
        Vector3 force = _pivot.right * _currentStrafeSpeed;
        Thrust(force);
    }


    void Thrust(Vector3 force)
    {
        _rb.AddForce(force, ForceMode.Force);
    }

    void Turn(Vector3 force)
    {
        _rb.AddTorque(force, ForceMode.Force);
    }

    internal void ToggleHeightLocked(bool value)
    {
        if (value)
        {
            if (!HeightLocked)
            {
                HeightLocked = true;
            }
            else
            {
                HeightLocked = false;
            }
        }
    }

    internal void CheckIfHeightLocked()
    {
        if (HeightLocked)
        {
            _rb.useGravity = false;
        }
        else
        {
            _rb.useGravity = true;
        }
    }

    internal void TiltPlaneBasedOnMovement(float roll, float pitch, Vector2 mouseInput)
    {
        //Add ease in for keyboard controls
        if (HelperScripts.RewiredControllerHelper.GetJoyStickUsed(_rewiredPlayer) == "Keyboard")
        {
            _rollAmount = IncrementTurnAmount(_rollAmount, RollIncrementRate, roll); // roll
            _pitchAmount = IncrementTurnAmount(_pitchAmount, PitchIncrementRate, pitch); // pitch
        }
        else
        {
            _rollAmount = roll;
            _pitchAmount = pitch;
        }
         _rollAmount += mouseInput.x;
        // Lerp to Angle
        _rotateToAnim = HelperScripts.RotationHelper.GetSignedEulerAngles(PivotPoint.localRotation.eulerAngles);
        _rotateToAnim.z = Mathf.Lerp(_rotateToAnim.z, -RollMaxAngle * _rollAmount, RollSpeed * Time.deltaTime);
        _rotateToAnim.x = Mathf.Lerp(_rotateToAnim.x, PitchMaxAngle * _pitchAmount, PitchSpeed * Time.deltaTime);
        //Apply to tranform pivot
        PivotPoint.localRotation = Quaternion.Euler(_rotateToAnim);
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

}
