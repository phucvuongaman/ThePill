using UnityEngine;

namespace TheProject
{
    // Enemy đứng yên, phát tiếng ambient
    // Chờ StateMachine kích hoạt Chase
    public class EnemyIdleState : BaseState<AnomalyEnemyStateMachine.EEnemyState>
    {
        private readonly AnomalyEnemy _ctx;

        public EnemyIdleState(AnomalyEnemy ctx, AnomalyEnemyStateMachine.EEnemyState key) : base(key)
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

            if (_ctx.IdleAudioClip != null && _ctx.EnemyAudioSource != null)
            {
                _ctx.EnemyAudioSource.clip = _ctx.IdleAudioClip;
                _ctx.EnemyAudioSource.loop = true;
                _ctx.EnemyAudioSource.Play();
            }
        }

        public override void UpdateState() { }

        public override void ExitState()
        {
            _ctx.EnemyAudioSource?.Stop();
        }

        public override AnomalyEnemyStateMachine.EEnemyState GetNextState() => StateKey;
    }
}
