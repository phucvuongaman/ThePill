using UnityEngine;

namespace TheProject
{
    // Gắn lên cùng GO với Animator của player (hoặc model child).
    // Thêm Animation Event tại frame chân chạm đất -> gọi PlayFootstep()
    // Tuyệt vời
    public class FootstepSound : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _clips;

        [Range(0f, 1f)]
        [SerializeField] private float _volume = 0.7f;

        // Gọi từ Animation Event
        public void PlayFootstep()
        {
            if (_audioSource == null || _clips == null || _clips.Length == 0) return;

            AudioClip clip = _clips[Random.Range(0, _clips.Length)];
            _audioSource.PlayOneShot(clip, _volume);
        }
    }
}
