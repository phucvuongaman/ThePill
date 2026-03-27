using UnityEngine;

namespace TheProject
{
    // State machine của enemy. Flow: Idle -> Chase (qua OnEnemyChaseStart) -> Caught.
    // StateManager tự route OnTriggerEnter xuống state hiện tại
    public class AnomalyEnemyStateMachine : StateManager<AnomalyEnemyStateMachine.EEnemyState>
    {
        public enum EEnemyState { Idle, Chase, Caught }

        private AnomalyEnemy _ctx;
        public EEnemyState CurrentStateKey => CurrentState.StateKey;

        private void Awake()
        {
            _ctx = GetComponent<AnomalyEnemy>();
            InitializeStates();
            CurrentState = States[EEnemyState.Idle];
        }

        private void InitializeStates()
        {
            States.Add(EEnemyState.Idle, new EnemyIdleState(_ctx, EEnemyState.Idle));
            States.Add(EEnemyState.Chase, new EnemyChaseState(_ctx, EEnemyState.Chase));
            States.Add(EEnemyState.Caught, new EnemyCaughtState(_ctx, EEnemyState.Caught));
        }

        private void OnEnable() =>
            EventManager.AddObserver<string>(GameEvents.Anomaly.OnEnemyChaseStart, HandleChaseStart);

        private void OnDisable()
        {
            EventManager.RemoveListener<string>(GameEvents.Anomaly.OnEnemyChaseStart, HandleChaseStart);
            _ctx?.ResetToOrigin();
            if (CurrentState != null && CurrentStateKey != EEnemyState.Idle)
                TransitionToState(EEnemyState.Idle);
        }

        private void HandleChaseStart(string eventID)
        {
            if (eventID != _ctx.LinkedEventID) return;
            if (CurrentStateKey == EEnemyState.Chase || CurrentStateKey == EEnemyState.Caught) return;

            if (_ctx.PlayerTransform == null) _ctx.RefreshPlayerRef();
            if (_ctx.PlayerTransform == null)
            {
                Debug.LogWarning($"[Enemy] {name}: không tìm thấy Player khi bắt đầu đuổi");
                return;
            }

            TransitionToState(EEnemyState.Chase);
        }
    }
}
