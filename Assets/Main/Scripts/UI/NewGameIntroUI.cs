
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace TheProject
{
    // Hiện bảng lore khi bắt đầu Day 1 (new game hoặc sau game over reset)
    // Đặt trong Persistent scene. Nhấn phím bất kỳ để qua slide
    public class NewGameIntroUI : MonoBehaviour
    {
        public static NewGameIntroUI Instance { get; private set; }

        [Header("Panel")]
        [SerializeField] private CanvasGroup _panel;
        [SerializeField] private TextMeshProUGUI _bodyText;

        [Header("Timing")]
        [SerializeField] private float _fadeDuration = 0.6f;
        [SerializeField] private float _holdDuration = 3f;

        [Tooltip("Delay trước khi intro bắt đầu. Set 0 để bắt đầu ngay lúc màn hình đang đen.")]
        [SerializeField] private float _startDelay = 0f;

        [Header("Slides — chỉnh nội dung trong Inspector")]
        [TextArea(2, 5)]
        [SerializeField]
        private string[] _slides = {
            "\"You need to take your pills on time.\nThey'll help you forget the bad things.\nDon't forget this time.\"\n— Dr. Khanh",
            "Three weeks since I moved\ninto this house.",
            "The neighbors say something bad\nhappened here before.",
            "Last night I heard\nfootsteps again.",
            "But I'm probably just\nvery tired.",
        };

        private bool _isPlaying = false;
        private bool _skipRequested = false;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            ResetPanel();
        }

        private void OnEnable()
        {
            EventManager.AddObserver<string>(GameEvents.Anomaly.OnDayStart, HandleDayStart);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<string>(GameEvents.Anomaly.OnDayStart, HandleDayStart);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            // Bắt input skip ở Update để đảm bảo không bị miss frame
            if (_isPlaying && (Keyboard.current?.anyKey.wasPressedThisFrame ?? false))
                _skipRequested = true;
        }

        private void OnSceneLoaded(Scene _, LoadSceneMode __)
        {
            if (_isPlaying)
                InputManager.Instance?.TogglePlayerInput(true);

            StopAllCoroutines();
            _isPlaying = false;
            _skipRequested = false;
            ResetPanel();
        }

        private void HandleDayStart(string _)
        {
            if (_isPlaying) return;
            if (AnomalyManager.Instance?.CurrentDay != 0) return;
            if (_slides == null || _slides.Length == 0) return;

            StartCoroutine(DelayThenPlay());
        }

        private IEnumerator DelayThenPlay()
        {
            if (_startDelay > 0f)
                yield return new WaitForSecondsRealtime(_startDelay);

            _isPlaying = true;
            InputManager.Instance?.TogglePlayerInput(false);
            yield return PlaySlides();
            InputManager.Instance?.TogglePlayerInput(true);
            _isPlaying = false;
        }

        private IEnumerator PlaySlides()
        {
            _panel.gameObject.SetActive(true);
            _panel.alpha = 0f;

            foreach (var slide in _slides)
            {
                _skipRequested = false;

                if (_bodyText != null) _bodyText.text = slide;

                yield return Fade(0f, 1f);

                float t = 0f;
                while (t < _holdDuration && !_skipRequested)
                {
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }

                yield return Fade(1f, 0f);
            }

            _panel.alpha = 0f;
            _panel.gameObject.SetActive(false);
        }

        // Time-based lerp, chính xác hơn MoveTowards
        private IEnumerator Fade(float from, float to)
        {
            float t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                _panel.alpha = Mathf.Lerp(from, to, t / _fadeDuration);
                yield return null;
            }
            _panel.alpha = to;
        }

        private void ResetPanel()
        {
            if (_panel == null) return;
            _panel.alpha = 0f;
            _panel.gameObject.SetActive(false);
        }
    }
}
