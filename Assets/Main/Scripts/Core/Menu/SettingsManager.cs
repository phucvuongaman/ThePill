using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace TheProject
{
    // Singleton xuyên scene. Quản lý Settings Panel + lưu PlayerPrefs.
    // Setup: Kéo AudioMixer (đã Expose 'SoundVolume' & 'SFXVolume'), Panel, 3 Slider vào Inspector.
    // Mỗi Slider.OnValueChanged → kéo SettingsManager → chọn hàm Set... tương ứng.
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        private const string KEY_SOUND = "SoundVolume";
        private const string KEY_SFX = "SFXVolume";
        private const string KEY_BRIGHTNESS = "Brightness";

        private const string MIXER_SOUND = "SoundVolume";
        private const string MIXER_SFX = "SFXVolume";

        private const float DEFAULT_SOUND = 0.8f;
        private const float DEFAULT_SFX = 0.8f;
        private const float DEFAULT_BRIGHTNESS = 0f;

        public float CurrentSound => PlayerPrefs.GetFloat(KEY_SOUND, DEFAULT_SOUND);
        public float CurrentSFX => PlayerPrefs.GetFloat(KEY_SFX, DEFAULT_SFX);
        public float CurrentBrightness => PlayerPrefs.GetFloat(KEY_BRIGHTNESS, DEFAULT_BRIGHTNESS);

        [Header("Audio Mixer")]
        [Tooltip("Cần Expose 2 parameter: 'SoundVolume' và 'SFXVolume'")]
        [SerializeField] private AudioMixer _audioMixer;

        [Header("Settings Panel")]
        [SerializeField] private GameObject _settingsPanel;

        [Header("Sliders")]
        [SerializeField] private Slider _sliderSound;
        [SerializeField] private Slider _sliderSFX;
        [SerializeField] private Slider _sliderBrightness;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            if (_audioMixer == null)
                Debug.LogWarning("[SettingsManager] _audioMixer chưa gán -> Sound/SFX sẽ không thay đổi.");

            _settingsPanel?.SetActive(false);
            ApplyAllSettings();
        }

        private void OnEnable()
        {
            InputManager.onEscapePressed += ClosePanel;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            InputManager.onEscapePressed -= ClosePanel;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // ── PANEL ────────────────────────────────────────────────────

        public void TogglePanel()
        {
            if (_settingsPanel == null) return;
            bool isOpening = !_settingsPanel.activeSelf;
            _settingsPanel.SetActive(isOpening);
            if (isOpening) SyncSliders();
        }

        public void ClosePanel() => _settingsPanel?.SetActive(false);

        private void OnSceneLoaded(Scene _, LoadSceneMode __) => ClosePanel();

        // ── PUBLIC API (gán vào Slider.OnValueChanged) ───────────────

        // Slider: Min=0, Max=1
        public void SetSoundVolume(float value)
        {
            PlayerPrefs.SetFloat(KEY_SOUND, value);
            if (_audioMixer != null)
                _audioMixer.SetFloat(MIXER_SOUND, LinearToDecibel(value));
        }

        // Slider: Min=0, Max=1
        public void SetSFXVolume(float value)
        {
            PlayerPrefs.SetFloat(KEY_SFX, value);
            if (_audioMixer != null)
                _audioMixer.SetFloat(MIXER_SFX, LinearToDecibel(value));
        }

        // Slider: Min=-2, Max=2
        public void SetBrightness(float value)
        {
            PlayerPrefs.SetFloat(KEY_BRIGHTNESS, value);
            if (VolumeManager.Instance != null)
                VolumeManager.Instance.SetBrightness(value);
            else
                Debug.LogWarning("[SettingsManager] VolumeManager.Instance null -> Brightness không apply.");
        }

        // ── PRIVATE ──────────────────────────────────────────────────

        private void ApplyAllSettings()
        {
            SetSoundVolume(CurrentSound);
            SetSFXVolume(CurrentSFX);
            SetBrightness(CurrentBrightness);
        }

        private void SyncSliders()
        {
            _sliderSound?.SetValueWithoutNotify(CurrentSound);
            _sliderSFX?.SetValueWithoutNotify(CurrentSFX);
            _sliderBrightness?.SetValueWithoutNotify(CurrentBrightness);
        }

        // linear [0,1] -> decibel [-80, 0]
        private float LinearToDecibel(float linear)
        {
            return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
        }
    }
}
