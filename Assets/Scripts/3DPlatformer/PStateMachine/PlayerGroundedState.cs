using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    private bool _coyoteTimeHasExpired;



    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    :base (currentContext, playerStateFactory){
        IsRootState = true;
        
    }


    public override void EnterState() 
    {
        InitializeSubState();
        //Debug.Log(string.Format("Enter State: GroundState"));
        ApplyGravity();
        _coyoteTimeHasExpired = false;
    }

    public override void UpdateState() 
    {
        CheckCoyoteTime();
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

        //if expired switch to falling.
        if (_coyoteTimeHasExpired) 
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


    public void ApplyGravity()
    {
        Context.CurrentWalkMovementY = Context.Gravity;

        //Debug.Log(string.Format("GroundedState. CurrentWalkMovementY set to {0}", Context.CurrentWalkMovementY));

        Context.AppliedMovementY = Context.Gravity;

        //Debug.Log(string.Format("GroundedState. AppliedMovementY", Context.AppliedMovementY));
    }

    public void CheckCoyoteTime()
    {
        if (!Context.CharacterController.isGrounded)
        {
            //start falling after time.
            //"coyote-ing" could be it's own state.
            Context.CurrentCoyoteTimeRoutine = Context.StartCoroutine(IFallAfterCoyoteTime(Context.CoyoteTime));

            Context.CurrentWalkMovementY = 0;
            Context.AppliedMovementY = 0;
        }
        else if(Context.CurrentCoyoteTimeRoutine != null)
        {
            Context.StopCoroutine(Context.CurrentCoyoteTimeRoutine);
            ApplyGravity();
        }
    }

    IEnumerator IFallAfterCoyoteTime(float coyoteTime)
    {
        yield return new WaitForSeconds(coyoteTime);
        _coyoteTimeHasExpired = true;
    }

}
