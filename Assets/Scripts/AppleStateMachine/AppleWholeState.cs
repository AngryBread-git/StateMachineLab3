using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleWholeState : AppleBaseState
{
    public override void EnterState(AppleStateMachine apple)
    {
        Debug.Log("Entered AppleGrowingState");
    }

    public override void UpdateState(AppleStateMachine apple)
    {
        throw new System.NotImplementedException();
    }
}
