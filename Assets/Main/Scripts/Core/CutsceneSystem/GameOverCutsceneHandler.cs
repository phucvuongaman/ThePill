using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace TheProject
{
    // Listens for OnGameOver, plays a Timeline cutscene, then fires OnGameOverDone.
    // Setup: assign a PlayableDirector to _director. If null, the cutscene is skipped
    // and AnomalyManager will reset after its timeout.
    public class GameOverCutsceneHandler : MonoBehaviour
    {
        [Header("Cutscene")]
        [Tooltip("PlayableDirector chứa Timeline cutscene death. Kéo vào đây.")]
        [SerializeField] private PlayableDirector _director;

        [Tooltip("Delay (giây, unscaled) trước khi play cutscene — cho fade đen kịp xảy ra.")]
        [SerializeField] private float _preDelay = 0.5f;

        private void OnEnable()
        {
            EventManager.AddObserver(GameEvents.Anomaly.OnGameOver, HandleGameOver);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener(GameEvents.Anomaly.OnGameOver, HandleGameOver);
        }

        private void Awake()
        {
            if (_director == null)
                _director = GetComponent<PlayableDirector>();

            if (_director == null)
                Debug.LogWarning("[GameOverCutsceneHandler] PlayableDirector not assigned. Cutscene will be skipped.");
        }

        private void HandleGameOver()
        {
            StartCoroutine(PlayGameOverCutscene());
        }

        private IEnumerator PlayGameOverCutscene()
        {
            Debug.Log("[GameOverCutsceneHandler] Starting game over cutscene.");

            if (_preDelay > 0f)
                yield return new WaitForSecondsRealtime(_preDelay);

            if (_director != null)
            {
                _director.stopped += OnDirectorStopped;
                _director.time = 0;
                _director.Play();

                while (_director.state == PlayState.Playing)
                    yield return null;
            }
            else
            {
                // No director assigned, skip straight to done so AnomalyManager isn't blocked.
                Debug.LogWarning("[GameOverCutsceneHandler] No PlayableDirector, skipping cutscene.");
                NotifyDone();
            }
        }

        private void OnDirectorStopped(PlayableDirector director)
        {
            director.stopped -= OnDirectorStopped;
            NotifyDone();
        }

        private void NotifyDone()
        {
            Debug.Log("[GameOverCutsceneHandler] Cutscene done, notifying AnomalyManager.");
            EventManager.Notify(GameEvents.Anomaly.OnGameOverDone);
        }
    }
}
