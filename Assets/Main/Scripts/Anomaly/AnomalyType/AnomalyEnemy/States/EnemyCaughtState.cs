using UnityEngine;

namespace TheProject
{
    // Dừng lại, play animation tấn công, báo AnomalyManager xử lý tiếp.
    // State này không tự thoát — AnomalyManager sẽ disable GO sau khi xử lý xong.
    public class EnemyCaughtState : BaseState<AnomalyEnemyStateMachine.EEnemyState>
    {
        private readonly AnomalyEnemy _ctx;

        public EnemyCaughtState(AnomalyEnemy ctx, AnomalyEnemyStateMachine.EEnemyState key) : base(key)
        {
            _ctx = ctx;
        }

        public override void EnterState()
        {
            if (_ctx.Agent.isOnNavMesh)
            {
                _ctx.Agent.isStopped = true;
                _ctx.Agent.ResetPath();
            }

            _ctx.Anim?.SetFloat(_ctx.AnimSpeed, 0f);
            _ctx.Anim?.SetTrigger(_ctx.AnimCaught);

            EventManager.Notify(GameEvents.Anomaly.OnPlayerCaught);
        }

        public override void UpdateState() { }
        public override void ExitState() { }
        public override AnomalyEnemyStateMachine.EEnemyState GetNextState() => StateKey;
    }
}
