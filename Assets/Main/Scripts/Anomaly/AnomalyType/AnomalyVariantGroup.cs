using System.Collections.Generic;
using UnityEngine;

namespace TheProject
{
    /// <summary>
    /// [ANOMALY VARIANT GROUP]
    /// Kế thừa AnomalyBase.
    /// Bật/tắt các GameObject trong scene để thể hiện sự thay đổi bất thường.
    ///
    /// NGUYÊN TẮC QUAN TRỌNG:
    /// Scene trong Editor = trạng thái "Normal Day" (ngày bình thường).
    /// Script này snapshot trạng thái đó lúc Start() làm "ground truth".
    /// Deactivate() LUÔN restore về snapshot — không toggle mù quáng.
    /// → Tránh conflict khi nhiều group cùng quản lý 1 object.
    ///
    /// SETUP TRONG UNITY EDITOR:
    /// 1. Tạo GameObject rỗng, ví dụ "Variant_Cabinet_Open"
    /// 2. Kéo script này vào
    /// 3. Set _linkedEventID = eventID của AnomalyEventSO
    /// 4. objectsToEnable  → kéo object cần BẬT khi anomaly vào
    /// 5. objectsToDisable → kéo object cần TẮT khi anomaly vào
    /// </summary>
    public class AnomalyVariantGroup : AnomalyBase
    {
        [Header("Khi anomaly này ACTIVE")]
        [Tooltip("Các object được BẬT khi anomaly này được chọn.")]
        public List<GameObject> objectsToEnable = new();

        [Tooltip("Các object bị TẮT khi anomaly này được chọn.")]
        public List<GameObject> objectsToDisable = new();

        // Snapshot trạng thái ban đầu (scene editor = Normal Day = ground truth)
        private Dictionary<GameObject, bool> _initialStates = new();

        private void Awake()
        {
            // Snapshot phải chạy trong Awake() — trước Start() và trước mọi event
            // Nếu để ở Start(): Start() có thể chạy SAU khi Activate() đã thay đổi trạng thái object
            // → snapshot sẽ ghi lại trạng thái anomaly thay vì Normal Day → Deactivate() sai hoàn toàn
            SnapshotInitialStates(objectsToEnable);
            SnapshotInitialStates(objectsToDisable);
        }

        protected override void Activate()
        {
            foreach (var obj in objectsToEnable)
                if (obj != null) obj.SetActive(true);

            foreach (var obj in objectsToDisable)
                if (obj != null) obj.SetActive(false);
        }

        protected override void Deactivate()
        {
            // Restore về ĐÚNG trạng thái ban đầu 
            foreach (var kvp in _initialStates)
                if (kvp.Key != null) kvp.Key.SetActive(kvp.Value);
        }

        private void SnapshotInitialStates(List<GameObject> objects)
        {
            foreach (var obj in objects)
            {
                // Nếu object chưa có trong dict thì mới snapshot
                // (tránh override nếu cùng object nằm trong cả 2 list)
                if (obj != null && !_initialStates.ContainsKey(obj))
                    _initialStates[obj] = obj.activeSelf;
            }
        }
    }
}

