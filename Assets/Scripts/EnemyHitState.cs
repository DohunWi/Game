using UnityEngine;

public class EnemyHitState : MonsterState
{
    private int knockbackDir;

    public EnemyHitState(EnemyAI enemy, EnemyStateMachine stateMachine, string animBoolName)
        : base(enemy, stateMachine, animBoolName)
    {
    }

    // 넉백 방향 설정 (EnemyAI에서 호출)
    public void SetKnockbackDirection(int direction)
    {
        knockbackDir = direction;
    }

    public override void Enter()
    {
        base.Enter();
        // 넉백 적용: 위로 살짝 뜨면서 뒤로 밀려남
        enemy.SetVelocity(0); // 기존 속도 초기화
        enemy.Rb.AddForce(new Vector2(knockbackDir * enemy.knockbackSpeed.x, enemy.knockbackSpeed.y), ForceMode2D.Impulse);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 넉백 시간(스턴 시간)이 지나면 다시 추적 상태로 복귀
        if (Time.time >= enemy.startTime + enemy.knockbackDuration)
        {
            // 착지했는지 체크하고 싶으면 여기에 isGrounded 체크 추가
            stateMachine.ChangeState(enemy.ChaseState);
        }
    }
}