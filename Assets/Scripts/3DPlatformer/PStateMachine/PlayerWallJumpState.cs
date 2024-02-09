using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerBaseState, IRootState
{

    public PlayerWallJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }

    public override void EnterState()
    {
        InitializeSubState();
        Debug.Log(string.Format("Enter State: WallJumpState"));
        PerformWallJump();
    }

    public override void UpdateState()
    {
        ApplyGravity();
        CheckSwitchState();
    }

    public override void ExitState()
    {
        Context.IsOnWall = false;
    }

    public override void CheckSwitchState()
    {
        if (Context.CharacterController.isGrounded)
        {
            SwitchState(Factory.Grounded());
        }
        else if (Context.IsOnWall)
        {
            SwitchState(Factory.WallRun());
        }

        //timer för att byta till fall efter några sekunder.
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

    public void PerformWallJump() 
    {
        Vector3 wallSurfaceNormal = Context.WallSurfaceNormal;

        //idk
        Context.AppliedMovementX = Context.CurrentMovementInput.x * Context.RunSpeed * wallSurfaceNormal.x;
        Context.AppliedMovementZ = Context.CurrentMovementInput.y * Context.RunSpeed * wallSurfaceNormal.z;
        Context.CurrentWalkMovementY = Context.InitialJumpVelocities[1];
    }

    public void ApplyGravity()
    {
        float previousYVelocity = Context.CurrentWalkMovementY;

        //add the old velocity and the new velocity, average them and set that value.
        //called VelocityVerlet
        Context.CurrentWalkMovementY = Context.CurrentWalkMovementY + (Context.WallRunGravity * Time.deltaTime);
        Context.AppliedMovementY = (previousYVelocity + Context.CurrentWalkMovementY) * 0.5f;
    }


}
