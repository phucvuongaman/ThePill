using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Unity.Cinemachine;

namespace TheProject
{
    public class CutsceneDirector : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private PlayableDirector _director;
        [SerializeField] private bool _playOnStart = false;

        private void Start()
        {
            if (_director == null) _director = GetComponent<PlayableDirector>();
            BindTimelineToMainCamera();

            if (_playOnStart) PlayCutscene();
        }

        private void BindTimelineToMainCamera()
        {
            if (_director == null || _director.playableAsset == null) return;
            var timelineAsset = (TimelineAsset)_director.playableAsset;

            var mainBrain = Camera.main.GetComponent<CinemachineBrain>();
            if (mainBrain != null)
            {
                foreach (var track in timelineAsset.GetOutputTracks())
                {
                    if (track is CinemachineTrack)
                    {
                        _director.SetGenericBinding(track, mainBrain);
                        break;
                    }
                }
            }
        }

        public void PlayCutscene()
        {
            if (_director == null) return;
            _director.stopped += OnCutsceneStopped;

            if (InputManager.Instance != null) InputManager.Instance.TogglePlayerInput(false);
            // if (AudioManager.Instance != null) AudioManager.Instance.PlayMusic(null, 1.0f);

            _director.Play();
        }

        private void OnCutsceneStopped(PlayableDirector obj)
        {
            _director.stopped -= OnCutsceneStopped;

            Debug.Log("OnCutsceneStopped");

            if (InputManager.Instance != null) InputManager.Instance.TogglePlayerInput(true);

            // --- 🆕 LOGIC MỚI: GỌI VÀO EVENT TRIGGER MANAGER ---
            // ---------------------------------------------------

            Debug.Log("🏁 Cutscene Finished.");
        }
    }
}