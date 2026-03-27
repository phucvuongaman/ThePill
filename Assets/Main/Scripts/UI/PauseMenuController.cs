using UnityEngine;

namespace TheProject
{
    // Gắn vào PausePanel (hoặc GO con bên trong).
    // Gán 3 hàm vào Button.OnClick() tương ứng.
    public class PauseMenuController : MonoBehaviour
    {
        private const string MAIN_MENU_SCENE = "MainMenu";

        // Cần ref đến chính PausePanel để ẩn khi mở Settings
        [SerializeField] private GameObject _pausePanel;

        public void OnResumeClicked()
        {
            GameManager.Instance.UpdateGameState(GameState.Gameplay);
        }

        public void OnSettingsClicked()
        {
            _pausePanel?.SetActive(false);
            SettingsManager.Instance.TogglePanel();
        }

        public void OnQuitToMenuClicked()
        {
            Time.timeScale = 1f;
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.TransitionTo(MAIN_MENU_SCENE, "");
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(MAIN_MENU_SCENE);
        }
    }
}
