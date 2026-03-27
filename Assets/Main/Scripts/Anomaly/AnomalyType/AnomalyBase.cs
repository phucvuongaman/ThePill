using UnityEngine;

namespace TheProject
{
    /// <summary>
    /// [ANOMALY BASE — Abstract]
    /// Class cha cho TẤT CẢ các loại Anomaly behavior.
    ///
    /// CÁCH MỞ RỘNG:
    /// 1. Tạo script mới kế thừa AnomalyBase (xem Handlers/ để lấy ví dụ)
    /// 2. Override Activate()   → hành vi khi event này đang active
    /// 3. Override Deactivate() → hành vi khi reset về trạng thái bình thường
    /// 4. KHÔNG cần tự đăng ký event — base class lo hết
    ///
    /// SETUP TRONG UNITY EDITOR:
    /// - Gán _linkedEventID = eventID của AnomalyEventSO tương ứng
    /// - KHÔNG điền gì thêm ở class cha — các trường riêng nằm ở class con
    /// </summary>
    public abstract class AnomalyBase : MonoBehaviour
    {
        [Tooltip("Phải khớp với eventID trong AnomalyEventSO tương ứng.")]
        [SerializeField] protected string _linkedEventID; // _eventID

        // ============================================================
        #region Unity Lifecycle — Event Registration
        // ============================================================

        /// <summary>
        /// Override nếu class con cần thêm logic OnEnable.
        /// Luôn gọi base.OnEnable() đầu tiên để giữ phần đăng ký event.
        /// </summary>
        protected virtual void OnEnable()
        {
            EventManager.AddObserver<string>(GameEvents.Anomaly.OnDayStart, HandleDayStart);
            EventManager.AddObserver(GameEvents.Anomaly.OnGameReset, Deactivate);
        }

        /// <summary>
        /// Override nếu class con cần thêm logic OnDisable.
        /// Luôn gọi base.OnDisable() đầu tiên.
        /// </summary>
        protected virtual void OnDisable()
        {
            EventManager.RemoveListener<string>(GameEvents.Anomaly.OnDayStart, HandleDayStart);
            EventManager.RemoveListener(GameEvents.Anomaly.OnGameReset, Deactivate);
        }

        #endregion

        // ============================================================
        #region Core Logic
        // ============================================================

        private void HandleDayStart(string activeEventID)
        {
            // Bước 1: LUÔN reset về trạng thái bình thường trước
            // → Đảm bảo object ngày cũ không còn ở trạng thái anomaly
            Deactivate();

            // Bước 2: Nếu event hôm nay KHỚP → mới bật lên
            if (activeEventID == _linkedEventID)
                Activate();
        }

        #endregion

        // ============================================================
        #region Abstract — Class con PHẢI implement
        // ============================================================

        /// <summary>
        /// Gọi khi event của ngày hôm nay KHỚP với _linkedEventID.
        /// → Bật hiệu ứng, bắt đầu AI, phát âm thanh...
        /// </summary>
        protected abstract void Activate();

        /// <summary>
        /// Gọi khi event của ngày KHÔNG khớp, hoặc khi game reset.
        /// → Tắt hiệu ứng, reset về trạng thái mặc định.
        /// </summary>
        protected abstract void Deactivate();

        #endregion
    }
}
