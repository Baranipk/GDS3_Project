using UnityEngine;

public class EnemyStateMachine
{
    public IEnemyState CurrentState { get; private set; }

    public void Initialize(IEnemyState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(IEnemyState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
