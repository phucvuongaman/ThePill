using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace TheProject
{
    /// <summary>
    /// [MAIN MENU CONTROLLER]
    /// Gắn vào GameObject trong scene MainMenu.
    /// Gán 4 hàm vào Button.OnClick() tương ứng.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        private const string GAMEPLAY_SCENE = "SampleScene";

        [Header("Buttons")]
        [SerializeField] private Button _btnNewGame;
        [SerializeField] private Button _btnSettings;
        [SerializeField] private Button _btnExit;

        // =====================================================================

        private void Start()
        {
            // Reset state về MainMenu: UIManager sẽ unlock cursor, ẩn pause panel
            GameManager.Instance?.UpdateGameState(GameState.MainMenu);


        }


        // =====================================================================
        // BUTTON CALLBACKS
        // =====================================================================

        /// <summary>Continue — load save cũ</summary>
        public void OnContinueClicked()
        {
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.TransitionTo(GAMEPLAY_SCENE, "CONTINUE");
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(GAMEPLAY_SCENE);
        }

        public void OnNewGameClicked()
        {
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.TransitionTo(GAMEPLAY_SCENE, "START");
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(GAMEPLAY_SCENE);
        }

        /// <summary>Settings — mở Settings Panel</summary>
        public void OnSettingsClicked()
        {
            SettingsManager.Instance.TogglePanel();
        }

        /// <summary>Exit — thoát game</summary>
        public void OnExitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
