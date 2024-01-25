public abstract class PlayerBaseState
{

    public abstract void EnterState();

    public abstract void UpdateState();

    public abstract void ExitState();

    public abstract void CheckSwitchState();

    public abstract void InitializeSubState();

    void UpdateStates() { }

    void SwitchStates() { }

    void SetSuperState() { }

    void SetSubState() { }

}
