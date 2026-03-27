using System.Collections;
using TMPro;
using UnityEngine;

namespace TheProject
{
    /// <summary>
    /// [ANOMALY TRANSITION UI]
    /// Hiển thị message giữa các lần chuyển ngày:
    ///   - Đúng  -> "DAY X"
    ///   - Sai   -> câu ngẫu nhiên bí ẩn
    ///   - Thua hẳn -> "THẤT BẠI"
    ///   - Thắng hết -> "HOÀN THÀNH" -> load Main Menu
    ///
    /// QUAN TRỌNG: Tất cả timing dùng Unscaled Time vì
    /// SameSceneTeleport đặt Time.timeScale = 0 trong lúc transition.
    /// </summary>
    public class AnomalyTransitionUI : MonoBehaviour
    {
        [Header("Result Panel (Đúng / Sai)")]
        [SerializeField] private CanvasGroup _resultPanel;
        [SerializeField] private TextMeshProUGUI _messageText;

        [Header("Game Over / Complete Panel")]
        [SerializeField] private CanvasGroup _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _gameOverText;

        [Header("Timing")]
        [Tooltip("Thời gian hiển thị message (giây, unscaled).")]
        [SerializeField] private float _displayDuration = 2f;
        [SerializeField] private float _fadeSpeed = 4f;

        private const string CORRECT_FORMAT = "NIGHT {0}";

        private static readonly string[] WRONG_MESSAGES =
        {
            "The house looks the same as always.",
            "I just need more sleep.",
            "Nothing unusual at all.",
            "That thing wasn't real.",
            "I'll be more careful next time.",
            "Stay calm. Stay calm.",
            "Why did I see that?",
            "Don't think about it. Don't think about it.",
        };

        private Coroutine _displayRoutine;

        private void OnEnable()
        {
            EventManager.AddObserver<AnomalyResultData>(GameEvents.Anomaly.OnDayCorrect, HandleCorrect);
            EventManager.AddObserver<int>(GameEvents.Anomaly.OnWrong, HandleWrong);
            EventManager.AddObserver(GameEvents.Anomaly.OnGameOver, HandleGameOver);
            EventManager.AddObserver(GameEvents.Anomaly.OnGameComplete, HandleGameComplete);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<AnomalyResultData>(GameEvents.Anomaly.OnDayCorrect, HandleCorrect);
            EventManager.RemoveListener<int>(GameEvents.Anomaly.OnWrong, HandleWrong);
            EventManager.RemoveListener(GameEvents.Anomaly.OnGameOver, HandleGameOver);
            EventManager.RemoveListener(GameEvents.Anomaly.OnGameComplete, HandleGameComplete);
        }

        private void Start()
        {
            SetAlpha(_resultPanel, 0f);
            SetAlpha(_gameOverPanel, 0f);
        }

        // ── HANDLERS ────────────────────────────────────────────────

        private void HandleCorrect(AnomalyResultData data)
        {
            // ĐÊM THỨ X + timestamp giả tạo cảm giác horror diary
            int hour = Random.Range(0, 2) == 0 ? Random.Range(22, 24) : Random.Range(0, 4);
            int minute = Random.Range(0, 60);
            string period = hour >= 22 ? "PM" : "AM";
            string time = $"{hour:D2}:{minute:D2} {period}";
            string msg = $"{string.Format(CORRECT_FORMAT, data.Day)}\n{time}";
            ShowResult(msg);
        }

        private void HandleWrong(int _)
        {
            ShowResult(WRONG_MESSAGES[Random.Range(0, WRONG_MESSAGES.Length)]);
        }

        private void HandleGameOver()
        {
            if (_displayRoutine != null) StopCoroutine(_displayRoutine);
            SetAlpha(_resultPanel, 0f);
            if (_gameOverText != null) _gameOverText.text = "FAILED";
            // Sau khi show xong -> bao AnomalyManager tiep tuc DoActualReset
            _displayRoutine = StartCoroutine(ShowGameOverThenNotify());
        }

        private IEnumerator ShowGameOverThenNotify()
        {
            yield return ShowPanel(_gameOverPanel, _displayDuration * 1.5f);
            EventManager.Notify(GameEvents.Anomaly.OnGameOverDone);
        }

        private void HandleGameComplete()
        {
            if (_displayRoutine != null) StopCoroutine(_displayRoutine);
            SetAlpha(_resultPanel, 0f);
            if (_gameOverText != null) _gameOverText.text = "COMPLETED";
            _displayRoutine = StartCoroutine(ShowCompleteAndExit());
        }

        // ── DISPLAY LOGIC ───────────────────────────────────────────

        private void ShowResult(string message)
        {
            if (_displayRoutine != null) StopCoroutine(_displayRoutine);
            if (_messageText != null) _messageText.text = message;
            _displayRoutine = StartCoroutine(ShowPanel(_resultPanel, _displayDuration));
        }

        /// <summary>
        /// Hiển thị panel trong thời gian thực (unscaled) vì timeScale có thể = 0.
        /// KHÔNG quản lý input ở đây — SameSceneTeleport đã dùng timeScale=0 lo hết.
        /// </summary>
        private IEnumerator ShowPanel(CanvasGroup panel, float duration)
        {
            if (panel == null) yield break;

            yield return FadePanel(panel, 0f, 1f);
            yield return new WaitForSecondsRealtime(duration);
            yield return FadePanel(panel, 1f, 0f);
        }

        /// <summary>
        /// Riêng cho game complete — không có SameSceneTeleport nên tự quản lý timeScale.
        /// </summary>
        private IEnumerator ShowCompleteAndExit()
        {
            Time.timeScale = 0f;
            InputManager.Instance?.TogglePlayerInput(false);

            yield return FadePanel(_gameOverPanel, 0f, 1f);
            yield return new WaitForSecondsRealtime(_displayDuration * 2f);
            yield return FadePanel(_gameOverPanel, 1f, 0f);

            Time.timeScale = 1f;
            SceneTransitionManager.Instance?.TransitionTo("MainMenu", "");
        }

        private IEnumerator FadePanel(CanvasGroup panel, float from, float to)
        {
            panel.alpha = from;
            while (!Mathf.Approximately(panel.alpha, to))
            {
                // unscaledDeltaTime — vẫn chạy khi timeScale = 0
                panel.alpha = Mathf.MoveTowards(panel.alpha, to, Time.unscaledDeltaTime * _fadeSpeed);
                yield return null;
            }
            panel.alpha = to;
        }

        private void SetAlpha(CanvasGroup panel, float alpha)
        {
            if (panel != null) panel.alpha = alpha;
        }
    }
}
