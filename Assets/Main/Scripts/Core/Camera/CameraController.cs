using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace TheProject
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("Cinemachine Components")]
        [SerializeField] private CinemachineInputAxisController _camInputAxisController;
        [SerializeField] private CinemachineCamera _playerCam;
        [SerializeField] private Camera _unityCamera;
        [SerializeField] private AudioListener _audioListener;
        [SerializeField] private RawImage _renderTextureDisplay;



        private PlayerContext _playerContext;
        private Vector3 _originalCamPos;


        public CinemachineCamera PlayerCam { get => _playerCam; }
        public Camera UnityCamera => _unityCamera;
        public RawImage RenderTextureDisplay => _renderTextureDisplay;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

        }

        void Start()
        {
            if (GameManager.Instance != null)
            {
                HandleCameraOnGameStateChanged(GameManager.Instance.CurrentState);
            }
        }

        private void OnEnable()
        {
            EventManager.AddObserver<bool>(GameEvents.Camera.EnableCamRotate, EnableCamRotate);
            EventManager.AddObserver<Transform>(GameEvents.SceneTransition.OnPlayerSpawned, HandlePlayerSpawned);
            GameManager.OnGameStateChanged += HandleCameraOnGameStateChanged;
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<bool>(GameEvents.Camera.EnableCamRotate, EnableCamRotate);
            EventManager.RemoveListener<Transform>(GameEvents.SceneTransition.OnPlayerSpawned, HandlePlayerSpawned);
            GameManager.OnGameStateChanged -= HandleCameraOnGameStateChanged;
        }



        // Hàm này giờ sẽ được gọi MỘT LẦN khi Player được spawn
        private void HandlePlayerSpawned(Transform playerTransform)
        {
            if (playerTransform == null) return;

            _playerContext = playerTransform.GetComponent<PlayerContext>();
            if (_playerContext == null)
            {
                Debug.LogError("Spawned Player does not have a PlayerContext component!", playerTransform);
                return;
            }

            CameraAnchorMarker anchorMarker = playerTransform.GetComponentInChildren<CameraAnchorMarker>();
            if (anchorMarker != null)
            {
                Transform cameraAnchor = anchorMarker.transform;
                _playerCam.Follow = cameraAnchor;
                _originalCamPos = _playerCam.transform.localPosition;
                Debug.Log("Camera Target and Context SET!");
            }
            else
            {
                Debug.LogError("Could not find 'Camera Anchor' as a child of the new Player.", playerTransform);
            }
        }





        private void EnableCamRotate(bool active)
        {
            if (_camInputAxisController != null)
            {
                _camInputAxisController.enabled = active;
            }
        }

        private void HandleCameraOnGameStateChanged(GameState newState)
        {
            if (newState == GameState.MainMenu)
            {
                if (_playerCam != null) _playerCam.enabled = false;
                if (_unityCamera != null) _unityCamera.enabled = false;
                if (_audioListener != null) _audioListener.enabled = false;
                if (_camInputAxisController != null) _camInputAxisController.enabled = false;
            }
            else if (newState == GameState.Gameplay)
            {
                if (_playerCam != null) _playerCam.enabled = true;
                if (_unityCamera != null) _unityCamera.enabled = true;
                if (_audioListener != null) _audioListener.enabled = true;
                // Chỉ bật cam input khi Gameplay — Paused/Cutscene sẽ tự tắt
                if (_camInputAxisController != null) _camInputAxisController.enabled = true;
            }
            else // Paused (cutscene sau này cũng sẽ dùng đoạn này)
            {
                if (_camInputAxisController != null) _camInputAxisController.enabled = false;
            }
        }
    }
}