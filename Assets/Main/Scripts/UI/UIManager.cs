using UnityEngine;

namespace TheProject
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private GameObject _pausePanel;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                HandleGameStateChanged(GameManager.Instance.CurrentState);
        }

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += HandleGameStateChanged;
            InputManager.onEscapePressed += TogglePausePanel;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
            InputManager.onEscapePressed -= TogglePausePanel;
        }

        public void TogglePausePanel()
        {
            if (GameManager.Instance.CurrentState == GameState.MainMenu) return;

            bool isOpening = _pausePanel != null && !_pausePanel.activeSelf;
            _pausePanel?.SetActive(isOpening);

            GameManager.Instance.UpdateGameState(
                isOpening ? GameState.Paused : GameState.Gameplay);
        }

        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.MainMenu:
                    _pausePanel?.SetActive(false);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    Time.timeScale = 1f;
                    break;

                case GameState.Gameplay:
                    _pausePanel?.SetActive(false);
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Time.timeScale = 1f;
                    break;

                case GameState.Paused:
                    _pausePanel?.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    Time.timeScale = 0f;
                    break;
            }
        }
    }
}