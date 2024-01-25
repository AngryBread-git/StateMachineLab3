using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    :base (currentContext, playerStateFactory){ }

    public override void EnterState() 
    {
        _context.CurrentWalkMovementY = _context.GravityWhileGrounded;
        _context.AppliedMovementY = _context.GravityWhileGrounded;
    }

    public override void UpdateState() 
    {
        CheckSwitchState();
    }

    public override void ExitState() { }

    public override void CheckSwitchState() 
    {
        if (_context.IsJumpPressed && !_context.RequireNewJumpPress) 
        {
            SwitchState(_factory.Jump());
        }
    }

    public override void InitializeSubState() { }
}
