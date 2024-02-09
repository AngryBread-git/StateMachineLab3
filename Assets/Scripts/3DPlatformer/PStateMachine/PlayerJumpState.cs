using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{

    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) {
        IsRootState = true;
    }

    private bool _isFalling;

    public override void EnterState() 
    {
        InitializeSubState();
        PerformJump();
    }

    public override void UpdateState() 
    {

        ApplyGravity();

        CheckSwitchState();
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
        //arghhhh
        else if (Context.IsOnWall)
        {
            SwitchState(Factory.WallRun());
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

    IEnumerator IResetJumpRoutine(float timeToResetJumpCount)
    {
        yield return new WaitForSeconds(timeToResetJumpCount);
        Context.JumpCount = 0;

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

        //Debug.Log(string.Format("Perform jump, _context.JumpCount: {0}", Context.JumpCount));

        //Debug.Log(string.Format("Perform jump, apply y velocity: {0}", _initialJumpVelocity));


        Context.CurrentWalkMovementY = Context.InitialJumpVelocities[Context.JumpCount];
        Context.AppliedMovementY = Context.InitialJumpVelocities[Context.JumpCount];
        //Debug.Log(string.Format("Perform jump, _context.CurrentWalkMovementY velocity: {0}", Context.CurrentWalkMovementY));

    }

    public void ApplyGravity() 
    {
        _isFalling = false;

        //check if the character is falling
        //not a big fan of this, seems like falling gravity gets applied very late in the jump.
        if (Context.CurrentWalkMovementY < -0.25f || !Context.IsJumpPressed)
        {
            _isFalling = true;
            //Debug.Log(string.Format("ApplyGravity, _isFalling", _isFalling));
            //Debug.Log(string.Format("Perform jump, walk y velocity: {0}", currentWalkMovement.y));
        }

        if (_isFalling)
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
