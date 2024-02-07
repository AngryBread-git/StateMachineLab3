using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : PlayerBaseState, IRootState
{
    public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
        
    }
    float _previousYVelocity;
    float _maxFallSpeed;

    public override void EnterState()
    {
        InitializeSubState();
        //Debug.Log(string.Format("Enter State: FallState"));
        _maxFallSpeed = Context.MaxFallSpeed;
        Context.Animator.SetBool(Context.IsFallingHash, true);
    }

    public override void UpdateState()
    {
        ApplyGravity();
        CheckSwitchState();
    }

    public override void ExitState()
    {
        //Debug.Log(string.Format("Exit State: FallState"));
        Context.Animator.SetBool(Context.IsFallingHash, false);
    }

    public void ApplyGravity() 
    {
        _previousYVelocity = Context.CurrentWalkMovementY;
        Context.CurrentWalkMovementY = Context.CurrentWalkMovementY + Context.Gravity + Time.deltaTime;
        Context.AppliedMovementY = Mathf.Max((_previousYVelocity + Context.CurrentWalkMovementY) * 0.5f, _maxFallSpeed);
    }

    public override void CheckSwitchState()
    {
        //if the character is grounded, switch to the grounded state.
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



}
