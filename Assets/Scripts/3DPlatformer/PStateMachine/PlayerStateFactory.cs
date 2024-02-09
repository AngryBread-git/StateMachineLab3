using System.Collections.Generic;

enum PlayerStates 
{
    idle,
    walk,
    run,
    grounded,
    jump,
    fall,
    wallRun,
    wallJump
}

public class PlayerStateFactory 
{
    PlayerStateMachine _context;

    //Use this to save the instance of each state. no need to create a new one each time.
    //This does mean that the constructor funcitons can't have any logic that is done when it is constructed.
    //in this case I moved "InitializeSubState();" from the constructed to Enter.
    Dictionary<PlayerStates, PlayerBaseState> _states = new Dictionary<PlayerStates, PlayerBaseState>();

    public PlayerStateFactory(PlayerStateMachine currentContext) 
    {
        _context = currentContext;
        _states[PlayerStates.idle] = new PlayerIdleState(_context, this);
        _states[PlayerStates.walk] = new PlayerWalkState(_context, this);
        _states[PlayerStates.run] = new PlayerRunState(_context, this);
        _states[PlayerStates.grounded] = new PlayerGroundedState(_context, this);
        _states[PlayerStates.jump] = new PlayerJumpState(_context, this);
        _states[PlayerStates.fall] = new PlayerFallState(_context, this);
        _states[PlayerStates.wallRun] = new PlayerWallRunState(_context, this);
        _states[PlayerStates.wallJump] = new PlayerWallJumpState(_context, this);
    }

    public PlayerBaseState Idle() 
    {
        return _states[PlayerStates.idle];
    }

    public PlayerBaseState Walk()
    {
        return _states[PlayerStates.walk];
    }

    public PlayerBaseState Run()
    {
        return _states[PlayerStates.run];
    }

    public PlayerBaseState Grounded()
    {
        return _states[PlayerStates.grounded];
    }

    public PlayerBaseState Jump()
    {
        return _states[PlayerStates.jump];
    }

    public PlayerBaseState Fall()
    {
        return _states[PlayerStates.fall];
    }

    public PlayerBaseState WallRun()
    {
        return _states[PlayerStates.wallRun];
    }

    public PlayerBaseState WallJump()
    {
        return _states[PlayerStates.wallJump];
    }

}
