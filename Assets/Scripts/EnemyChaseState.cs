using UnityEngine;

public class EnemyChaseState : MonsterState
{
    public EnemyChaseState(EnemyAI enemy, EnemyStateMachine stateMachine, string animBoolName)
        : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        // 추적 시작할 때 소리를 내거나 느낌표 이펙트를 띄울 수 있음
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. 공격 사거리 안에 들어왔는지 체크
        if (enemy.CheckPlayerInAttackRange())
        {
            if (Time.time >= enemy.lastAttackTime + enemy.attackCooldown)
            {
                // 공격 가능 -> 공격 상태로
                stateMachine.ChangeState(enemy.AttackState);
            }
            else
            {
                // 쿨타임 중 -> 멈춰서 노려보기
                enemy.SetVelocity(0f);
                
                // [수정] 제자리걸음 방지! 
                // "Chase 상태지만 멈춰있으니 Idle 애니메이션을 보여줘"
                // (주의: 애니메이터에 있는 이름과 똑같아야 함. 예: "Enemy_Idle")
                enemy.PlayAnim("Idle"); 
            }
            return;
        }

        // 2. 감지 범위 밖 -> Idle 상태로 복귀
        if (!enemy.CheckPlayerInSight())
        {
            stateMachine.ChangeState(enemy.IdleState);
            return;
        }

        // 3. 추적 이동 로직
        if (enemy.Target != null)
        {
            float direction = (enemy.Target.position.x > enemy.transform.position.x) ? 1f : -1f;
            enemy.SetVelocity(enemy.moveSpeed * direction);
            enemy.Flip(direction);
            
            // [수정] 다시 움직이니까 Run 애니메이션 재생
            // (주의: 애니메이터 이름과 똑같아야 함. 예: "Enemy_Run")
            enemy.PlayAnim("Chase");
        }
    }
}