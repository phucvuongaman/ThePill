using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TheProject
{
    // Lắng nghe OnJumpscare -> hiện ảnh + phát sound -> tắt sau _duration giây
    // Đặt trong Persistent scene
    // Chỉ cần 1 cái
    public class JumpscareEffect : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private CanvasGroup _panel;
        [SerializeField] private Image _image;
        [SerializeField] private float _duration = 1f;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;

        [Tooltip("Sound phát nếu jumpscare không có sound riêng.")]
        [SerializeField] private AudioClip _defaultSound;

        private void Awake()
        {
            if (_panel != null) _panel.alpha = 0f;
        }

        private void OnEnable() =>
            EventManager.AddObserver<JumpscareData>(GameEvents.Anomaly.OnJumpscare, HandleJumpscare);

        private void OnDisable() =>
            EventManager.RemoveListener<JumpscareData>(GameEvents.Anomaly.OnJumpscare, HandleJumpscare);

        private void HandleJumpscare(JumpscareData data) => StartCoroutine(PlayRoutine(data));

        private IEnumerator PlayRoutine(JumpscareData data)
        {
            if (_image != null && data.Sprite != null)
                _image.sprite = data.Sprite;

            // Dùng sound riêng nếu có, fallback về default
            AudioClip clip = data.Sound != null ? data.Sound : _defaultSound;
            if (_audioSource != null && clip != null)
                _audioSource.PlayOneShot(clip);

            if (_panel != null) _panel.alpha = 1f;
            yield return new WaitForSeconds(_duration);
            if (_panel != null) _panel.alpha = 0f;
        }
    }
}
