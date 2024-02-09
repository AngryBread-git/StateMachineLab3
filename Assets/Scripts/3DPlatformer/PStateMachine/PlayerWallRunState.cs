using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallRunState : PlayerBaseState, IRootState
{

    public PlayerWallRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }
    float _previousYVelocity;
    private float _wallRunGravity;
    bool _hasLowMovement;

    private float _absAppliedMovementX;

    private float _absAppliedMovementZ;

    public override void EnterState()
    {
        InitializeSubState();
        Debug.Log(string.Format("Enter State: WallRunState"));
        _wallRunGravity = Context.WallRunGravity;
        //turn on animation
        //set other movement to 0.
        _hasLowMovement = false;
    }

    public override void UpdateState()
    {
        ApplyGravity();
        CheckMovementSpeed();
        CheckSwitchState();

    }

    public override void ExitState()
    {
        //Debug.Log(string.Format("Exit State: WallSlideState"));

        //turn off animation
        //set other movement to org values.
        Context.IsOnWall = false;
    }

    public override void CheckSwitchState()
    {
        //or go to jump. I suppose.
        if (Context.IsJumpPressed)
        {
            //SwitchState(Factory.WallJump());
        }

        else if (_hasLowMovement) 
        {
            SwitchState(Factory.Fall());
        }

        else if (Context.CharacterController.isGrounded)
        {
            SwitchState(Factory.Grounded());
        }
    }


    public override void InitializeSubState()
    {
        
       SetSubState(Factory.Idle());
       
    }

    public void CheckMovementSpeed() 
    {
        //make vars.
        _absAppliedMovementX = Mathf.Abs(Context.AppliedMovementX);

        _absAppliedMovementZ = Mathf.Abs(Context.AppliedMovementZ);

        Debug.Log(string.Format("WallRunState. absAppX is: {0}", _absAppliedMovementX.ToString()));
        Debug.Log(string.Format("WallRunState. absAppZ is: {0}", _absAppliedMovementZ.ToString()));

        if (_absAppliedMovementX + _absAppliedMovementZ < 1.0f) 
        {
            Debug.Log(string.Format("WallRunState. slow movement"));
            _hasLowMovement = true;
        }
    }


    public void ApplyGravity()
    {

        //apply lower gravity
        _previousYVelocity = Context.CurrentWalkMovementY;
        Context.CurrentWalkMovementY = Context.CurrentWalkMovementY + Context.Gravity + Time.deltaTime;
        Context.AppliedMovementY = Mathf.Max((_previousYVelocity + Context.CurrentWalkMovementY) * 0.5f, _wallRunGravity);
    }

}
