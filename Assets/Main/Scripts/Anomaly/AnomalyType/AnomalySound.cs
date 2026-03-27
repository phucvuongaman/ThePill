using UnityEngine;

namespace TheProject
{
    /// <summary>
    /// [ANOMALY SOUND]
    /// Kế thừa AnomalyBase.
    /// Phát âm thanh bất thường khi anomaly này active (tiếng bước chân, tiếng thở, tiếng gõ...).
    /// Script luôn active → luôn nhận event, không bị mất listener.
    ///
    /// SETUP TRONG UNITY EDITOR:
    /// 1. Gắn script lên bất kỳ GO nào (nên là GameObject rỗng hoặc cùng GO với âm thanh liên quan)
    /// 2. Gán AudioSource vào _audioSource (hoặc để tự tìm trên same GO)
    /// 3. Gán AudioClip vào _anomalyClip
    /// 4. Set _linkedEventID = eventID của AnomalyEventSO
    /// 5. Tick _looping nếu muốn phát lặp liên tục
    /// </summary>
    public class AnomalySound : AnomalyBase
    {
        [Header("Sound Settings")]
        [Tooltip("AudioSource để phát âm thanh. Để trống sẽ tự tìm trên GO này.")]
        [SerializeField] private AudioSource _audioSource;

        [Tooltip("AudioClip phát khi anomaly active.")]
        [SerializeField] private AudioClip _anomalyClip;

        [Tooltip("Phát lặp liên tục không? (false = chỉ phát 1 lần)")]
        [SerializeField] private bool _looping = true;

        [Tooltip("Âm lượng anomaly (0-1).")]
        [Range(0f, 1f)]
        [SerializeField] private float _volume = 1f;

        private void Awake()
        {
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();

            if (_audioSource == null)
                Debug.LogWarning("[AnomalySound] Không tìm thấy AudioSource! Gán vào Inspector.");
        }

        protected override void Activate()
        {
            if (_audioSource == null || _anomalyClip == null) return;

            _audioSource.clip = _anomalyClip;
            _audioSource.loop = _looping;
            _audioSource.volume = _volume;
            _audioSource.Play();
        }

        protected override void Deactivate()
        {
            if (_audioSource == null) return;

            _audioSource.Stop();
            _audioSource.clip = null;
        }
    }
}
