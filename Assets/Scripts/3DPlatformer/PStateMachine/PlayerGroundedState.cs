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

    public void ApplyGravity() 
    {
        Context.CurrentWalkMovementY = Context.Gravity;

        //Debug.Log(string.Format("GroundedState. CurrentWalkMovementY set to {0}", Context.CurrentWalkMovementY));

        Context.AppliedMovementY = Context.Gravity;

        //Debug.Log(string.Format("GroundedState. AppliedMovementY", Context.AppliedMovementY));
    }

    public override void EnterState() 
    {
        //Debug.Log(string.Format("Enter State: GroundState"));
        ApplyGravity();
    }

    public override void UpdateState() 
    {
        CheckSwitchState();
    }

    public override void ExitState() 
    {
        //Debug.Log(string.Format("Exit State: GroundState"));
    }

    public override void CheckSwitchState() 
    {
        if (Context.IsJumpPressed && !Context.RequireNewJumpPress)
        {
            SwitchState(Factory.Jump());
        }
        else if (!Context.CharacterController.isGrounded) 
        {
            SwitchState(Factory.Fall());
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
