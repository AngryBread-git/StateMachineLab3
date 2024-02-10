using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallRunState : PlayerBaseState, IRootState
{

    public PlayerWallRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }
    float _previousYVelocity;
    private float _wallRunGravity;
    bool _hasLowMovement;
    bool _isAwayFromWall;

    private Vector3 _wallSurfaceNormal;

    private float _appliedMovementX;

    private float _appliedMovementZ;

    public override void EnterState()
    {
        InitializeSubState();
        Debug.Log(string.Format("Enter State: WallRunState"));
        _wallRunGravity = Context.WallRunGravity;
        _wallSurfaceNormal = Context.WallSurfaceNormal;

        //turn on animation
        //set other movement to 0.
        _hasLowMovement = false;
        _isAwayFromWall = false;
    }

    public override void UpdateState()
    {
        ApplyGravity();

        _appliedMovementX = Context.AppliedMovementX;
        _appliedMovementZ = Context.AppliedMovementZ;

        CheckMovementDirection();
        CheckMovementSpeed();
        CheckSwitchState();

    }

    public override void ExitState()
    {
        //Debug.Log(string.Format("Exit State: WallSlideState"));

        //turn off animation
        //set other movement to org values.
        Context.IsOnWall = false;
    }

    public override void CheckSwitchState()
    {
        if (Context.IsJumpPressed)
        {
            //SwitchState(Factory.WallJump());
        }

        else if (_hasLowMovement || _isAwayFromWall) 
        {
            SwitchState(Factory.Fall());
        }

        else if (Context.CharacterController.isGrounded)
        {
            SwitchState(Factory.Grounded());
        }
    }


    public override void InitializeSubState()
    {
        
       SetSubState(Factory.Idle());
       
    }

    public void CheckMovementSpeed() 
    {

        //Debug.Log(string.Format("WallRunState. absAppX is: {0}", _absAppliedMovementX.ToString()));
        //Debug.Log(string.Format("WallRunState. absAppZ is: {0}", _absAppliedMovementZ.ToString()));

        if (Mathf.Abs(_appliedMovementX) + Mathf.Abs(_appliedMovementZ) < 1.0f) 
        {
            Debug.Log(string.Format("WallRunState. slow movement"));
            _hasLowMovement = true;
        }
    }

    public void CheckMovementDirection() 
    {

        //if _appliedMovementX and _wallSurfaceNormal.x, or Z and .z, are both pos, or neg, then the player is moving away from the wall.
        if ((_appliedMovementX > 0 && _wallSurfaceNormal.x > 0 ) || (_appliedMovementX < 0 && _wallSurfaceNormal.x < 0)
            || (_appliedMovementZ > 0 && _wallSurfaceNormal.z > 0) || (_appliedMovementZ < 0 && _wallSurfaceNormal.z < 0)) 
        {
            Debug.Log(string.Format("WallRunState. Moved away from wall"));
            _isAwayFromWall = true;
        }
        
    }


    public void ApplyGravity()
    {

        //apply lower gravity
        _previousYVelocity = Context.CurrentWalkMovementY;
        Context.CurrentWalkMovementY = Context.CurrentWalkMovementY + Context.Gravity + Time.deltaTime;
        Context.AppliedMovementY = Mathf.Max((_previousYVelocity + Context.CurrentWalkMovementY) * 0.5f, _wallRunGravity);
    }

}
