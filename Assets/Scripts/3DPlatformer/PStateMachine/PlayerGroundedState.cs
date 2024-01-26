using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    :base (currentContext, playerStateFactory){
        IsRootState = true;
        InitializeSubState(); 
    }

    public override void EnterState() 
    {
        Context.CurrentWalkMovementY = Context.GravityWhileGrounded;
        Context.AppliedMovementY = Context.GravityWhileGrounded;
    }

    public override void UpdateState() 
    {
        CheckSwitchState();
    }

    public override void ExitState() { }

    public override void CheckSwitchState() 
    {
        if (Context.IsJumpPressed && !Context.RequireNewJumpPress) 
        {
            SwitchState(Factory.Jump());
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
