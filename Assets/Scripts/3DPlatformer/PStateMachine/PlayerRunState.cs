using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) { }

    public override void EnterState() 
    {
        Context.Animator.SetBool(Context.IsWalkingHash, true);
        Context.Animator.SetBool(Context.IsRunningHash, true);
    }

    public override void UpdateState() 
    {
        CheckSwitchState();
        //_context.AppliedMovementX = _context.RunSpeed
        Context.AppliedMovementX = Context.CurrentMovementInput.x * Context.RunSpeed;
        Context.AppliedMovementZ = Context.CurrentMovementInput.y * Context.RunSpeed;
    }

    public override void ExitState() { }

    public override void CheckSwitchState() 
    {
        if (!Context.IsMovementPressed)
        {
            SwitchState(Factory.Idle());
        }
        else if (Context.IsMovementPressed && !Context.IsRunPressed)
        {
            SwitchState(Factory.Walk());
        }
    }

    public override void InitializeSubState() { }
}
