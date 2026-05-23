public class BossStateMachine
{
    public IBossState CurrentState { get; private set; }

    public void Initialize(IBossState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(IBossState newState)
    {
        string from = CurrentState?.GetType().Name ?? "null";
        string to = newState?.GetType().Name ?? "null";
        UnityEngine.Debug.Log($"[Boss State] {from} -> {to}");

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
