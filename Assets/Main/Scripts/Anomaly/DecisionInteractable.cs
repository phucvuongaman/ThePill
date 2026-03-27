using Unity.VisualScripting;
using UnityEngine;

namespace TheProject
{
    public class DecisionInteractable : BaseInteractable
    {
        [SerializeField] private bool _reportAnomaly = true;

        [SerializeField] private InteractableType _interactType = InteractableType.GenericObject;

        [Header("Effects")]
        [SerializeField] private float _fadeSpeed = 3f;
        [SerializeField] private AudioSource _decisionAudio;
        [SerializeField] private AudioClip _anomalyDecisionClip;

        protected OutLineController outLine;

        public override InteractableType InteractType
        {
            get => _interactType;
            set => _interactType = value;
        }

        private bool _hasDecided = false;

        private void Awake()
        {
            outLine = GetComponent<OutLineController>();

        }

        private void OnEnable() =>
            EventManager.AddObserver<string>(GameEvents.Anomaly.OnDayStart, HandleDayStart);

        private void OnDisable() =>
            EventManager.RemoveListener<string>(GameEvents.Anomaly.OnDayStart, HandleDayStart);

        public override void OnFocus()
        {
            outLine?.EnableOutline();
        }
        public override void OnLoseFocus()
        {
            outLine?.DisableOutline();
        }

        public override void OnInteractPress(Interactor interactor)
        {
            if (_hasDecided) return;
            if (AnomalyManager.Instance == null)
            {
                Debug.LogError("[DecisionInteractable] AnomalyManager.Instance null!");
                return;
            }

            _hasDecided = true;

            // Freeze ngay — tránh di chuyển trong lúc fade
            Time.timeScale = 0f;
            InputManager.Instance?.TogglePlayerInput(false);

            SceneTransitionManager.Instance?.SetNextFadeOutSpeed(_fadeSpeed);

            if (_decisionAudio != null && _anomalyDecisionClip != null)
                _decisionAudio.PlayOneShot(_anomalyDecisionClip);

            AnomalyManager.Instance.OnDecision(_reportAnomaly);
        }

        private void HandleDayStart(string _) => _hasDecided = false;
    }
}
