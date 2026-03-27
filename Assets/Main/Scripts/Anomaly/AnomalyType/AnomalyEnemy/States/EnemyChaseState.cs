using UnityEngine;

namespace TheProject
{
    // Enemy chạm player -> chuyển sang Caught. Cập nhật Speed animator mỗi frame.
    public class EnemyChaseState : BaseState<AnomalyEnemyStateMachine.EEnemyState>
    {
        private readonly AnomalyEnemy _ctx;
        private bool _caughtTriggered = false;

        public EnemyChaseState(AnomalyEnemy ctx, AnomalyEnemyStateMachine.EEnemyState key) : base(key)
        {
            _ctx = ctx;
        }

        public override void EnterState()
        {
            _caughtTriggered = false;

            if (_ctx.Agent.isOnNavMesh)
                _ctx.Agent.isStopped = false;

            if (_ctx.ChaseAudioClip != null && _ctx.EnemyAudioSource != null)
            {
                _ctx.EnemyAudioSource.clip = _ctx.ChaseAudioClip;
                _ctx.EnemyAudioSource.loop = true;
                _ctx.EnemyAudioSource.Play();
            }
        }

        public override void UpdateState()
        {
            if (_ctx.PlayerTransform == null) return;
            if (!_ctx.Agent.isOnNavMesh) return;

            _ctx.Agent.SetDestination(_ctx.PlayerTransform.position);
            _ctx.Anim?.SetFloat(_ctx.AnimSpeed, _ctx.Agent.velocity.magnitude);
        }

        public override void ExitState()
        {
            _ctx.Anim?.SetFloat(_ctx.AnimSpeed, 0f);
            _ctx.EnemyAudioSource?.Stop();
        }

        public override void OnTriggerEnter(Collider other)
        {
            if (_caughtTriggered) return;
            if (!other.CompareTag(_ctx.PlayerTag)) return;

            _caughtTriggered = true;
            Debug.Log($"[Enemy] {_ctx.name} bắt được player");
        }

        public override AnomalyEnemyStateMachine.EEnemyState GetNextState()
        {
            return _caughtTriggered
                ? AnomalyEnemyStateMachine.EEnemyState.Caught
                : StateKey;
        }
    }
}
