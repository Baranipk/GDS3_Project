using UnityEngine;

public class PlayerStateMachine 
{
    public IplayerState CurrentState;

    public void Initialize(IplayerState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(IplayerState newState)
    {
        //if (CurrentState == newState) return;

        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void ExecuteFixedUpdate()
    {
        CurrentState?.FixedUpdate();
    }
}
