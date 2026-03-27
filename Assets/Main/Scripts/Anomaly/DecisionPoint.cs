using UnityEngine;

namespace TheProject
{
    /// <summary>
    /// [DECISION POINT]
    /// Đặt component này lên một GameObject có Collider (Is Trigger = true).
    /// Khi Player bước vào vùng trigger -> có thể gọi quyết định Bất thường / Bình thường.
    ///
    /// SETUP TRONG UNITY EDITOR:
    /// 1. Tạo GameObject, ví dụ "DecisionPoint"
    /// 2. Gắn Collider bất kỳ -> tick "Is Trigger"
    /// 3. Kéo script này vào
    /// 4. Gán nút "Bất thường" -> gọi ReportAnomaly()
    ///    Gán nút "Bình thường" -> gọi ReportNormal()
    ///    (Hoặc gọi 2 hàm này từ UI component bên ngoài)
    ///
    /// LƯU Ý:
    /// DecisionPoint KHÔNG tự vẽ UI. Nó chỉ bật/tắt _decisionUI GameObject
    /// và gọi AnomalyManager khi player quyết định.
    /// Kéo panel UI chứa 2 nút vào _decisionUI trong Inspector.
    /// </summary>
    public class DecisionPoint : MonoBehaviour
    {
        [Tooltip("Tag của Player để nhận biết khi nào bước vào trigger.")]
        [SerializeField] private string _playerTag = "Player";

        [Tooltip("GameObject chứa UI 2 nút (Bất thường / Bình thường). Sẽ được bật/tắt tự động.")]
        [SerializeField] private GameObject _decisionUI;

        // Cờ tránh player vào ra trigger liên tục spam quyết định
        private bool _hasDecided = false;
        private bool _playerInside = false;

        // ============================================================
        #region Unity Lifecycle
        // ============================================================

        private void OnEnable()
        {
            // Mỗi ngày mới bắt đầu -> reset trạng thái để player có thể quyết định lại
            EventManager.AddObserver<string>(GameEvents.Anomaly.OnDayStart, HandleDayStart);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<string>(GameEvents.Anomaly.OnDayStart, HandleDayStart);
        }

        private void Start()
        {
            // Ẩn UI khi bắt đầu
            HideUI();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(_playerTag)) return;
            if (_hasDecided) return; // Ngày này đã quyết rồi, không cho quyết lại

            _playerInside = true;
            Debug.Log("[DecisionPoint] Player entered -> Awaiting decision.");
            ShowUI();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(_playerTag)) return;

            _playerInside = false;

            // Nếu chưa quyết định mà bước ra -> ẩn UI (player có thể quay lại)
            if (!_hasDecided) HideUI();
        }

        #endregion

        // ============================================================
        #region PUBLIC API — Gán vào Button.OnClick() trong Inspector
        // ============================================================

        /// <summary>
        /// Gọi khi player bấm nút "Bất thường".
        /// Gán hàm này vào Button.OnClick() trong Inspector.
        /// </summary>
        public void ReportAnomaly()
        {
            SubmitDecision(playerSaysAnomalyExists: true);
        }

        /// <summary>
        /// Gọi khi player bấm nút "Bình thường".
        /// Gán hàm này vào Button.OnClick() trong Inspector.
        /// </summary>
        public void ReportNormal()
        {
            SubmitDecision(playerSaysAnomalyExists: false);
        }

        #endregion

        // ============================================================
        #region PRIVATE
        // ============================================================

        private void SubmitDecision(bool playerSaysAnomalyExists)
        {
            if (_hasDecided)
            {
                Debug.LogWarning("[DecisionPoint] Đã quyết định rồi, bỏ qua.");
                return;
            }

            if (AnomalyManager.Instance == null)
            {
                Debug.LogError("[DecisionPoint] AnomalyManager.Instance là null! Kiểm tra scene.");
                return;
            }

            _hasDecided = true;
            HideUI();

            Debug.Log($"[DecisionPoint] Decision submitted: {(playerSaysAnomalyExists ? "BẤT THƯỜNG" : "BÌNH THƯỜNG")}");
            AnomalyManager.Instance.OnDecision(playerSaysAnomalyExists);
        }

        private void HandleDayStart(string eventID)
        {
            // Ngày mới -> cho phép quyết định lại
            _hasDecided = false;
            _playerInside = false;
            HideUI();
            Debug.Log($"[DecisionPoint] Reset cho ngày mới. Event: {eventID}");
        }

        private void ShowUI()
        {
            if (_decisionUI != null)
                _decisionUI.SetActive(true);
        }

        private void HideUI()
        {
            if (_decisionUI != null)
                _decisionUI.SetActive(false);
        }

        #endregion
    }
}
