using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleStateMachine : MonoBehaviour
{
    AppleBaseState currentState;

    public AppleGrowingState GrowingState = new AppleGrowingState();
    public AppleRottenState RottenState = new AppleRottenState();
    public AppleWholeState WholeState = new AppleWholeState();
    //etc

    // Start is called before the first frame update
    void Start()
    {
        //set default state
        currentState = GrowingState;
        //enter the default state and call it with the statemachine
        currentState.EnterState(this);
    }

    // Update is called once per frame
    void Update()
    {
        currentState.UpdateState(this);
    }

    public void SwitchState(AppleBaseState state) 
    {
        currentState = state;
        state.EnterState(this);
    }

}
