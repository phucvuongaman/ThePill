using System.Collections;
using UnityEngine;

namespace TheProject
{
    // Flash đỏ + scream khi player bị bắt. Sau khi xong -< báo AnomalyManager qua OnPlayerCaughtDone.
    public class EnemyCaughtEffect : MonoBehaviour
    {
        [Header("Red Flash")]
        [SerializeField] private CanvasGroup _redPanel;
        [SerializeField] private float _holdDuration = 0.5f;
        [SerializeField] private float _fadeSpeed = 6f;

        [Header("Audio")]
        [SerializeField] private AudioClip _screamClip;
        [SerializeField] private AudioSource _audioSource;

        private void Awake()
        {
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();
            if (_redPanel != null)
                _redPanel.alpha = 0f;
        }

        private void OnEnable() =>
            EventManager.AddObserver(GameEvents.Anomaly.OnPlayerCaught, HandlePlayerCaught);

        private void OnDisable() =>
            EventManager.RemoveListener(GameEvents.Anomaly.OnPlayerCaught, HandlePlayerCaught);

        private void HandlePlayerCaught() => StartCoroutine(PlayCaughtEffect());

        private IEnumerator PlayCaughtEffect()
        {
            InputManager.Instance?.TogglePlayerInput(false);

            if (_audioSource != null && _screamClip != null)
                _audioSource.PlayOneShot(_screamClip);

            if (_redPanel != null)
            {
                yield return FadePanel(0f, 1f);
                yield return new WaitForSecondsRealtime(_holdDuration);
                yield return FadePanel(1f, 0f);
            }
            else
            {
                Debug.LogWarning("[EnemyCaughtEffect] _redPanel chưa gán — bỏ qua flash");
                yield return new WaitForSecondsRealtime(_holdDuration + 0.5f);
            }

            EventManager.Notify(GameEvents.Anomaly.OnPlayerCaughtDone);
        }

        private IEnumerator FadePanel(float from, float to)
        {
            _redPanel.alpha = from;
            while (!Mathf.Approximately(_redPanel.alpha, to))
            {
                _redPanel.alpha = Mathf.MoveTowards(_redPanel.alpha, to, Time.unscaledDeltaTime * _fadeSpeed);
                yield return null;
            }
            _redPanel.alpha = to;
        }
    }
}
