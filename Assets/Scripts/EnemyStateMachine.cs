public class EnemyStateMachine
{
    public MonsterState CurrentState { get; private set; }

    // 초기 상태 설정
    public void Initialize(MonsterState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    // 상태 변경
    public void ChangeState(MonsterState newState)
    {
        CurrentState.Exit();      // 이전 상태 정리
        CurrentState = newState;  // 상태 교체
        CurrentState.Enter();     // 새 상태 시작
    }
}