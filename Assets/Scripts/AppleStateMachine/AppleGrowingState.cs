using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleGrowingState : AppleBaseState
{
    Vector3 startingSize = new Vector3(0.2f, 0.2f, 0.2f);
    Vector3 growingRate = new Vector3(0.1f, 0.1f, 0.1f);

    public override void EnterState(AppleStateMachine apple)
    {
        Debug.Log("Entered AppleGrowingState");
        apple.transform.localScale = startingSize;
    }

    public override void UpdateState(AppleStateMachine apple)
    {
        if (apple.transform.localScale.x < 1)
        {
            apple.transform.localScale += growingRate * Time.deltaTime;

        }
        else 
        {
            apple.SwitchState(apple.WholeState);
        }
    }
}
