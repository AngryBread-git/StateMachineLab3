using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    IEnumerator IResetJumpRoutine(float timeToResetJumpCount) 
    {
        yield return new WaitForSeconds(timeToResetJumpCount);
        _context.JumpCount = 0;

    }

    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) { }

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
        _context.Animator.SetBool(_context.IsJumpingHash, false);
        if(_context.IsJumpPressed)
        { 
            _context.RequireNewJumpPress = true;
        }
        //reset jump count after time
        _context.CurrentResetJumpRoutine = _context.StartCoroutine(IResetJumpRoutine(_context.TimeToResetJumpCount));

        //reset jump count if the player has performed a triple jump
        if (_context.JumpCount == 3)
        {
            _context.JumpCount = 0;
            _context.Animator.SetInteger(_context.JumpCountHash, _context.JumpCount);
        }
    }

    public override void CheckSwitchState() 
    {
        if (_context.CharacterController.isGrounded) 
        {
            SwitchState(_factory.Grounded());
        }
    }

    public override void InitializeSubState() { }

    void PerformJump() 
    {
        if (_context.JumpCount < 3 && _context.CurrentResetJumpRoutine != null)
        {
            _context.StopCoroutine(_context.CurrentResetJumpRoutine);
        }

        _context.Animator.SetBool(_context.IsJumpingHash, true);
        _context.IsJumping = true;
        _context.JumpCount += 1;
        _context.Animator.SetInteger(_context.JumpCountHash, _context.JumpCount);

        Debug.Log(string.Format("Perform jump, _context.JumpCount: {0}", _context.JumpCount));

        //Debug.Log(string.Format("Perform jump, apply y velocity: {0}", _initialJumpVelocity));

        //the video multiplies these by 0.5f. but that seems very odd. means that maxjumpheight becomes incorrect.
        _context.CurrentWalkMovementY = _context.InitialJumpVelocities[_context.JumpCount];
        _context.AppliedMovementY = _context.InitialJumpVelocities[_context.JumpCount];
        Debug.Log(string.Format("Perform jump, _context.CurrentWalkMovementY velocity: {0}", _context.CurrentWalkMovementY));

    }

    void ApplyGravity() 
    {
        bool isFalling = false;

        //check if the character is falling
        //not a big fan of this, seems like falling gravity gets applied very late in the jump.
        if (_context.CurrentWalkMovementY < -0.5f || !_context.IsJumpPressed)
        {
            isFalling = true;
            //Debug.Log(string.Format("ApplyGravity, _isFalling", _isFalling));
            //Debug.Log(string.Format("Perform jump, walk y velocity: {0}", currentWalkMovement.y));
        }

        if (isFalling)
        {
            float previousYVelocity = _context.CurrentWalkMovementY;
            //Verlet
            _context.CurrentWalkMovementY = _context.CurrentWalkMovementY + (_context.JumpGravityValues[_context.JumpCount] * _context.GravityFallMultipier * Time.deltaTime);
            _context.AppliedMovementY = Mathf.Max((previousYVelocity + _context.CurrentWalkMovementY) * 0.5f, -20.0f);

        }

        else
        {
            float previousYVelocity = _context.CurrentWalkMovementY;

            //add the old velocity and the new velocity, average them and set that value.
            //called VelocityVerlet
            _context.CurrentWalkMovementY = _context.CurrentWalkMovementY + (_context.JumpGravityValues[_context.JumpCount] * Time.deltaTime);
            _context.AppliedMovementY = (previousYVelocity + _context.CurrentWalkMovementY) * 0.5f;
        }
    }

}
