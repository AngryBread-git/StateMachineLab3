using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    IEnumerator IResetJumpRoutine(float timeToResetJumpCount) 
    {
        yield return new WaitForSeconds(timeToResetJumpCount);
        Context.JumpCount = 0;

    }

    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) {
        IsRootState = true;
        InitializeSubState();
    }

    public override void EnterState() 
    {
        PerformJump();
    }

    public override void UpdateState() 
    {
        CheckSwitchState();
        ApplyGravity();
    }

    public override void ExitState() 
    {
        Context.Animator.SetBool(Context.IsJumpingHash, false);
        if(Context.IsJumpPressed)
        {
            Context.RequireNewJumpPress = true;
        }
        //reset jump count after time
        Context.CurrentResetJumpRoutine = Context.StartCoroutine(IResetJumpRoutine(Context.TimeToResetJumpCount));

        //reset jump count if the player has performed a triple jump
        if (Context.JumpCount == 3)
        {
            Context.JumpCount = 0;
            Context.Animator.SetInteger(Context.JumpCountHash, Context.JumpCount);
        }
    }

    public override void CheckSwitchState() 
    {
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

    void PerformJump() 
    {
        if (Context.JumpCount < 3 && Context.CurrentResetJumpRoutine != null)
        {
            Context.StopCoroutine(Context.CurrentResetJumpRoutine);
        }

        Context.Animator.SetBool(Context.IsJumpingHash, true);
        Context.IsJumping = true;
        Context.JumpCount += 1;
        Context.Animator.SetInteger(Context.JumpCountHash, Context.JumpCount);

        Debug.Log(string.Format("Perform jump, _context.JumpCount: {0}", Context.JumpCount));

        //Debug.Log(string.Format("Perform jump, apply y velocity: {0}", _initialJumpVelocity));

        //the video multiplies these by 0.5f. but that seems very odd. means that maxjumpheight becomes incorrect.
        Context.CurrentWalkMovementY = Context.InitialJumpVelocities[Context.JumpCount];
        Context.AppliedMovementY = Context.InitialJumpVelocities[Context.JumpCount];
        Debug.Log(string.Format("Perform jump, _context.CurrentWalkMovementY velocity: {0}", Context.CurrentWalkMovementY));

    }

    void ApplyGravity() 
    {
        bool isFalling = false;

        //check if the character is falling
        //not a big fan of this, seems like falling gravity gets applied very late in the jump.
        if (Context.CurrentWalkMovementY < -0.5f || !Context.IsJumpPressed)
        {
            isFalling = true;
            //Debug.Log(string.Format("ApplyGravity, _isFalling", _isFalling));
            //Debug.Log(string.Format("Perform jump, walk y velocity: {0}", currentWalkMovement.y));
        }

        if (isFalling)
        {
            float previousYVelocity = Context.CurrentWalkMovementY;
            //Verlet
            Context.CurrentWalkMovementY = Context.CurrentWalkMovementY + (Context.JumpGravityValues[Context.JumpCount] * Context.GravityFallMultipier * Time.deltaTime);
            Context.AppliedMovementY = Mathf.Max((previousYVelocity + Context.CurrentWalkMovementY) * 0.5f, -20.0f);

        }

        else
        {
            float previousYVelocity = Context.CurrentWalkMovementY;

            //add the old velocity and the new velocity, average them and set that value.
            //called VelocityVerlet
            Context.CurrentWalkMovementY = Context.CurrentWalkMovementY + (Context.JumpGravityValues[Context.JumpCount] * Time.deltaTime);
            Context.AppliedMovementY = (previousYVelocity + Context.CurrentWalkMovementY) * 0.5f;
        }
    }

}
