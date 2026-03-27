using TMPro;
using UnityEngine;

namespace TheProject
{
    // Gắn lên GameObject có TextMeshPro (3D hoặc UI).
    // Tự cập nhật text khi mỗi ngày bắt đầu.
    // Format mặc định: "DAY 3" — đổi _format trong Inspector.
    public class DayDisplayProp : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _text;   // 3D world text
        // Nếu dùng trong Canvas thì đổi thành TextMeshProUGUI

        [Tooltip("Format string. {0} = day, {1} = phase. Ví dụ: 'DAY {0}' hoặc 'P{1} - D{0}'")]
        [SerializeField] private string _format = "DAY {0}";

        private void Awake()
        {
            if (_text == null) _text = GetComponent<TextMeshPro>();
        }

        private void OnEnable()
        {
            EventManager.AddObserver<string>(GameEvents.Anomaly.OnDayStart, HandleDayStart);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<string>(GameEvents.Anomaly.OnDayStart, HandleDayStart);
        }

        private void HandleDayStart(string _)
        {
            if (AnomalyManager.Instance == null || _text == null) return;

            int day = AnomalyManager.Instance.CurrentDay + 1;
            int phase = AnomalyManager.Instance.CurrentPhase + 1;
            _text.text = string.Format(_format, day, phase);
        }
    }
}
