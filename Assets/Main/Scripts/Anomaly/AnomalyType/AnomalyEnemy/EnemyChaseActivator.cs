using System.Collections;
using UnityEngine;

namespace TheProject
{
    // Trigger collider này kích hoạt enemy đuổi khi player bước vào.
    // Để GO luôn active — logic bên trong tự check state.
    public class EnemyChaseActivator : MonoBehaviour
    {
        [Tooltip("Phải khớp với _linkedEventID của AnomalyEnemy cần kích hoạt.")]
        [SerializeField] private string _enemyEventID;

        [SerializeField] private string _playerTag = "Player";

        [Tooltip("true = chỉ kích hoạt 1 lần mỗi ngày.")]
        [SerializeField] private bool _triggerOnce = true;

        private bool _triggered = false;

        private void OnEnable() =>
            EventManager.AddObserver<string>(GameEvents.Anomaly.OnDayStart, HandleDayStart);

        private void OnDisable() =>
            EventManager.RemoveListener<string>(GameEvents.Anomaly.OnDayStart, HandleDayStart);

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(_playerTag)) return;
            if (_triggerOnce && _triggered) return;

            _triggered = true;
            EventManager.Notify(GameEvents.Anomaly.OnEnemyChaseStart, _enemyEventID);
        }

        private void HandleDayStart(string _) => _triggered = false;
    }
}
