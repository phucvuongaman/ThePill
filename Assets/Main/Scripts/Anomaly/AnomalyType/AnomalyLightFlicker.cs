using System.Collections;
using UnityEngine;

namespace TheProject
{
    // Inherits AnomalyBase. Makes a light flicker when this anomaly is active.
    // Assign a Light to _light, set _linkedEventID to match your AnomalyEventSO.
    public class AnomalyLightFlicker : AnomalyBase
    {
        [Header("Light Settings")]
        [Tooltip("Light component của đèn cần làm nháy.")]
        [SerializeField] private Light _light;

        [Tooltip("Số lần nháy mỗi giây.")]
        [SerializeField] private float _flickerSpeed = 10f;

        [Tooltip("Thời gian nháy (giây). 0 = nháy vô hạn đến khi Deactivate.")]
        [SerializeField] private float _flickerDuration = 0f;

        private Coroutine _flickerRoutine;
        private float _originalIntensity;

        private void Awake()
        {
            if (_light == null)
                _light = GetComponent<Light>();

            if (_light == null)
                Debug.LogWarning($"[AnomalyLightFlicker] '{gameObject.name}' has no Light. Drag one into _light manually.");
            else
                _originalIntensity = _light.intensity;
        }

        protected override void Activate()
        {
            if (_light == null)
            {
                Debug.LogError($"[AnomalyLightFlicker] '{gameObject.name}' _light is null.");
                return;
            }

            if (_flickerRoutine != null) StopCoroutine(_flickerRoutine);
            _flickerRoutine = StartCoroutine(FlickerRoutine());
        }

        protected override void Deactivate()
        {
            if (_flickerRoutine != null)
            {
                StopCoroutine(_flickerRoutine);
                _flickerRoutine = null;
            }

            if (_light != null)
            {
                _light.enabled = true;
                _light.intensity = _originalIntensity;
            }
        }

        private IEnumerator FlickerRoutine()
        {
            float elapsed = 0f;
            float interval = 1f / _flickerSpeed;

            while (_flickerDuration <= 0f || elapsed < _flickerDuration)
            {
                _light.enabled = !_light.enabled;
                // WaitForSecondsRealtime so this keeps running even when timeScale = 0.
                yield return new WaitForSecondsRealtime(interval);
                elapsed += interval;
            }

            _light.enabled = true;
            _light.intensity = _originalIntensity;
        }
    }
}
