public abstract class PlayerBaseState
{
    //all concrete states inherit these values.
    protected PlayerStateMachine _context;
    protected PlayerStateFactory _factory;

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory) 
    {
        _context = currentContext;
        _factory = playerStateFactory;
    }

    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();

    public abstract void CheckSwitchState();

    public abstract void InitializeSubState();

    void UpdateStates() { }

    protected void SwitchState(PlayerBaseState stateToEnter) 
    {
        //exit current state
        ExitState();

        //enter the new state
        stateToEnter.EnterState();

        _context.CurrentState = stateToEnter;
    }

    void SetSuperState() { }

    void SetSubState() { }

}
