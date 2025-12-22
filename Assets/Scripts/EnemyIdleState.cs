using UnityEngine;

public class EnemyIdleState : MonsterState
{
    public EnemyIdleState(EnemyAI enemy, EnemyStateMachine stateMachine, string animBoolName)
        : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        enemy.SetVelocity(0f); // 멈춤
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 플레이어를 발견하면 Chase 상태로 전환
        if (enemy.CheckPlayerInSight())
        {
            stateMachine.ChangeState(enemy.ChaseState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}