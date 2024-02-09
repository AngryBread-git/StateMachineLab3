using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallSlideState : PlayerBaseState, IRootState
{

    public PlayerWallSlideState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }
    float _previousYVelocity;
    private float _wallSlideSpeed;

    public override void EnterState()
    {
        InitializeSubState();
        Debug.Log(string.Format("Enter State: WallSlideState"));
        _wallSlideSpeed = Context.WallSlideSpeed;
        //turn on animation
    }

    public override void UpdateState()
    {
        ApplyGravity();
        CheckSwitchState();

    }

    public override void ExitState()
    {
        //Debug.Log(string.Format("Exit State: WallSlideState"));

        //turn off animation
        Context.IsOnWall = false;
    }

    public override void CheckSwitchState()
    {
        //or go to jump. I suppose.

        if (Context.CharacterController.isGrounded)
        {
            SwitchState(Factory.Grounded());
        }
    }


    public override void InitializeSubState()
    {
        if (!Context.IsMovementPressed && !Context.IsRunPressed)
        {
            SetSubState(Factory.Idle());
        }
        else if (Context.IsMovementPressed && !Context.IsRunPressed)
        {
            SetSubState(Factory.Walk());
        }
        else if (Context.IsMovementPressed && Context.IsRunPressed)
        {
            SetSubState(Factory.Run());
        }
    }


    public void ApplyGravity()
    {

        //apply lower gravity
        _previousYVelocity = Context.CurrentWalkMovementY;
        Context.CurrentWalkMovementY = Context.CurrentWalkMovementY + Context.Gravity + Time.deltaTime;
        Context.AppliedMovementY = Mathf.Max((_previousYVelocity + Context.CurrentWalkMovementY) * 0.5f, _wallSlideSpeed);
    }

}
