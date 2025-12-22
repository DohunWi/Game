using UnityEngine;

public abstract class MonsterState
{
    protected EnemyAI enemy;
    protected EnemyStateMachine stateMachine;
    protected string animName; // 변수명 변경 (animBoolName -> animName)

    public MonsterState(EnemyAI enemy, EnemyStateMachine stateMachine, string animName)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
        this.animName = animName;
    }

    public virtual void Enter()
    {
        enemy.startTime = Time.time;
        
        // [변경] SetBool 대신 Play 사용!
        // 이 상태에 들어오면 무조건 해당 애니메이션을 처음부터 재생함
        enemy.Anim.Play(animName); 
    }

    public virtual void Exit()
    {
        // Play 방식은 Exit에서 끌 필요가 없습니다. 
        // 다음 상태가 Enter되면서 알아서 다른 걸 틀어버리니까요.
    }

    public virtual void LogicUpdate() { }
    public virtual void PhysicsUpdate() { }
}