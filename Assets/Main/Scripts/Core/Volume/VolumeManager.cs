using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TheProject
{
    // Chỉ cần Global Volume để điều chỉnh Brightness.
    // Kéo Global Volume GO vào Inspector.
    public class VolumeManager : MonoBehaviour
    {
        public static VolumeManager Instance { get; private set; }

        [SerializeField] private Volume _globalVolume;

        private ColorAdjustments _colorAdjust;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            if (_globalVolume != null)
                _globalVolume.profile.TryGet(out _colorAdjust);
            else
                Debug.LogWarning("[VolumeManager] _globalVolume chưa gán.");
        }

        private void Start()
        {
            // Apply brightness đã lưu từ Settings
            float saved = PlayerPrefs.GetFloat("Brightness", 0f);
            SetBrightness(saved);
        }

        public void SetBrightness(float value)
        {
            if (_colorAdjust != null)
                _colorAdjust.postExposure.value = value;
        }
    }
}