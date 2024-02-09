using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    public PlayerWalkState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) { }


    public override void EnterState() 
    {
        Context.Animator.SetBool(Context.IsWalkingHash, true);
        Context.Animator.SetBool(Context.IsRunningHash, false);
    }

    public override void UpdateState() 
    {
        CheckSwitchState();

        //Lerp to it if lower. or set it if higher.
        //aka I want some acceleration here.

        Context.AppliedMovementX = Context.CurrentMovementInput.x * Context.WalkSpeed;
        Context.AppliedMovementZ = Context.CurrentMovementInput.y * Context.WalkSpeed;

    }

    public override void ExitState() { }

    public override void CheckSwitchState() 
    {
        if (!Context.IsMovementPressed)
        {
            SwitchState(Factory.Idle());
        }
        else if (Context.IsMovementPressed && Context.IsRunPressed)
        {
            SwitchState(Factory.Run());
        }
        
    }

    public override void InitializeSubState() { }
}
