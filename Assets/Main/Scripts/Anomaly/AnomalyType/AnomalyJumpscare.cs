using UnityEngine;

namespace TheProject
{
    // Chỉ lo detect trigger và fire event.
    // JumpscareEffect (handler riêng) lo phần UI + audio.
    [RequireComponent(typeof(Collider))]
    public class AnomalyJumpscare : AnomalyBase
    {
        [SerializeField] private Sprite _jumpscareSprite;

        [Tooltip("Để trống thì JumpscareEffect dùng default sound.")]
        [SerializeField] private AudioClip _jumpscareSound;

        private Collider _col;
        private bool _triggered = false;

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _col.isTrigger = true;
        }

        protected override void Activate()
        {
            _triggered = false;
            _col.enabled = true;
        }

        protected override void Deactivate()
        {
            _col.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered) return;
            if (!other.CompareTag("Player")) return;

            _triggered = true;
            _col.enabled = false;

            EventManager.Notify<JumpscareData>(GameEvents.Anomaly.OnJumpscare, new JumpscareData
            {
                Sprite = _jumpscareSprite,
                Sound = _jumpscareSound
            });
        }
    }
}
