using UnityEngine;

namespace TheProject
{
    /// <summary>
    /// ScriptableObject định nghĩa 1 loại Anomaly.
    ///
    /// CÁCH TẠO:
    /// Project window → chuột phải → Create → DarkHome → Anomaly Event
    ///
    /// LƯU Ý:
    /// Không lưu scene object trực tiếp trong SO này.
    /// Thay vào đó, AnomalyVariantGroup trong scene sẽ tự đăng ký theo eventID.
    /// </summary>
    [CreateAssetMenu(fileName = "AnomalyEvent_", menuName = "SO/Anomaly Event")]
    public class AnomalyEventSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("ID duy nhất. Phải khớp với linkedEventID trong AnomalyVariantGroup ngoài scene.")]
        public string eventID;

        [TextArea(2, 4)]
        [Tooltip("Mô tả nội bộ — để dev dễ nhớ, không hiện cho player.")]
        public string description;

        [Header("Behavior")]
        [Tooltip("Nếu true: anomaly này có AI rượt player. Nếu AI chạm player = Wrong.")]
        public bool isThreat;
    }
}
