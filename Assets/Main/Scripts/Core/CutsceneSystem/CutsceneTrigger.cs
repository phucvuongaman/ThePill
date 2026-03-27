using UnityEngine;

namespace TheProject
{
    public class CutsceneTrigger : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Kéo GameObject chứa CutsceneDirector vào đây")]
        [SerializeField] private CutsceneDirector _director;

        [Tooltip("Nếu true: Kích hoạt xong sẽ tự xóa Trigger này để không lặp lại")]
        [SerializeField] private bool _triggerOnce = true;

        [Tooltip("Tag của Player (để tránh quái vật hay vật thể lạ kích hoạt)")]
        [SerializeField] private string _playerTag = "Player";

        private bool _hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            // 1. Check xem có đúng là Player không
            if (!other.CompareTag(_playerTag)) return;

            // 2. Check xem đã diễn chưa (Double check)
            if (_triggerOnce && _hasTriggered) return;

            // 3. Gọi Cutscene
            if (_director != null)
            {
                Debug.Log($"[CutsceneTrigger] Player entered {gameObject.name}. Playing cutscene.");
                _director.PlayCutscene(); // Gọi hàm trong script CutsceneDirector.cs của cậu

                _hasTriggered = true;

                // 4. Tự hủy Trigger nếu cần
                if (_triggerOnce)
                {
                    // Tắt gameObject trigger đi thay vì Destroy (để tránh lỗi Reference nếu lỡ có script khác gọi)
                    gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogError($"[CutsceneTrigger] Chưa gán CutsceneDirector cho {gameObject.name}!");
            }
        }
    }
}