public abstract class PlayerBaseState
{
    //all concrete states inherit these values.
    private bool _isRootState = false;
    private PlayerStateMachine _context;
    private PlayerStateFactory _factory;

    private PlayerBaseState _currentSubState;
    private PlayerBaseState _currentSuperState;

    #region GettersAndSetters

    protected bool IsRootState
    {
        set { _isRootState = value; }
    }

    protected PlayerStateMachine Context
    {
        get { return _context; }
    }

    protected PlayerStateFactory Factory
    {
        get { return _factory; }
    }

    #endregion GettersAndSetters

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

    public void UpdateStates() 
    {
        UpdateState();
        if (_currentSubState != null) 
        {
            _currentSubState.UpdateStates();
        }
    }

    public void ExitStates()
    {
        ExitState();
        if (_currentSubState != null)
        {
            _currentSubState.ExitStates();
        }
    }

    protected void SwitchState(PlayerBaseState stateToEnter) 
    {
        //exit current state
        ExitState();

        //enter the new state
        stateToEnter.EnterState();

        if (_isRootState)
        {
            //switch current state.
            _context.CurrentState = stateToEnter;
        }
        else if (_currentSuperState != null) 
        {
            //transfer the current substate to the new superstate
            _currentSuperState.SetSubState(stateToEnter);
        }
        
    }

    protected void SetSuperState(PlayerBaseState superState) 
    {
        _currentSuperState = superState;
    }

    protected void SetSubState(PlayerBaseState subState) 
    {
        _currentSubState = subState;
        subState.SetSuperState(this);
    }

}
