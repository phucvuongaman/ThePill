using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace TheProject
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance;

        [Header("Transition Settings")]
        [SerializeField] private GameObject _playerPrefab;
        [SerializeField] private CanvasGroup _fadeCanvas;

        [Tooltip("Các scene không có Player (MainMenu...). Khi đến scene này, player sẽ bị destroy.")]
        [SerializeField] private List<string> _menuSceneNames = new();

        private GameObject _playerInstance;

        private bool _isTransitioning = false;
        private float _nextFadeOutSpeed = 2f; // tốc độ fade mặc định

        /// <summary>Set tốc độ fade cho SameSceneTeleport tiếp theo (rồi tự reset về default).</summary>
        public void SetNextFadeOutSpeed(float speed) => _nextFadeOutSpeed = speed;
        public bool IsTransitioning => _isTransitioning;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        private void OnEnable()
        {
            EventManager.AddObserver<SceneChangeData>(GameEvents.SceneTransition.OnSceneChangeRequested, HandleSceneChangeRequest);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<SceneChangeData>(GameEvents.SceneTransition.OnSceneChangeRequested, HandleSceneChangeRequest);
        }

        private void HandleSceneChangeRequest(SceneChangeData data)
        {
            TransitionTo(data.SceneName, data.TargetSpawnID);
        }

        // Hàm TransitionTo cũ của bạn không cần thay đổi
        public void TransitionTo(string nextSceneName, string spawnID)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("Đang chuyển cảnh, từ từ thôi bạn ơi!");
                return;
            }
            // Kiểm tra sớm để báo lỗi rõ ràng thay vì crash giữa chừng
            if (_fadeCanvas == null)
                Debug.LogWarning("[SceneTransitionManager] _fadeCanvas chưa được gán trong Inspector! Transition vẫn chạy nhưng không có hiệu ứng fade.");
            if (_playerPrefab == null)
                Debug.LogError("[SceneTransitionManager] _playerPrefab chưa được gán trong Inspector!");

            StartCoroutine(TransitionRoutine(nextSceneName, spawnID));
        }

        #region TransitionRoutine
        // Khó nhớ comment nhiều một chút
        // Quy trình (Coroutine) chuyển cảnh
        /// <summary>
        /// Các viêc mà tôi cần làm khi chuyển scene
        /// Fade In Out mỗi khi chuyển cảnh: 🔹 1 và 10
        /// LoadScene mới rồi set active cho nó, mode sẽ là Additive để giữa cho SceneTransitionManager chạy không bị cắt tiết giữa chừng rồi dùng
        /// Lấy dữ liệu player cũ hoặc tạo mới player mới: 
        ///  BỎ + 🔹 2 và 3 (NHƯNG TÔI ĐÃ BỎ ĐI VÌ TÔI CẦN LOADSCENE, LÁY SPAWNPOINT,.. TRƯỚC KHI SET MỘT PLAYER)
        ///     + 🔹 6 và 7 sẽ gánh vác vấn đề đó
        ///  BỎ + 🔹 8 sẽ lắp cam vào player (TÔI SẼ ĐÃ GÃ CAMERACONTROLLER LÀM)
        /// Hủy đi scene cũ (trừ Persitent scene)
        /// </summary>
        /// <param name="nextSceneName"> Gã này ghi đúng string Scene name là được </param>
        /// <param name="spawnID"> phải tạo 1 GO chứa SpawnPoint, SpawnPoint.SpawnID sẽ giống với string spawnID là được</param>
        private IEnumerator TransitionRoutine(string nextSceneName, string spawnID)
        {
            // Debug.Log($"Bắt đầu chuyển cảnh: {nextSceneName} | Spawn: {spawnID}");
            _isTransitioning = true;
            //  Khóa input ngay lập tức — cursor ẩn, không xoay cam, không di chuyển
            InputManager.Instance?.TogglePlayerInput(false);
            bool success = false;
            try
            {

                // HECK: Nếu cùng scene thì chỉ Fade + Teleport, KHÔNG reload!
                string currentSceneName = SceneManager.GetActiveScene().name;
                bool isSameScene = (nextSceneName == currentSceneName);

                if (isSameScene)
                {
                    Debug.Log($"SAME SCENE detected! Fade + Teleport only (NO reload)");
                    yield return SameSceneTeleport(spawnID);
                    _isTransitioning = false;
                    success = true; // ← đặt trước yield break, nếu không finally sẽ log lỗi giả
                    Debug.Log("DONE! (Same-scene teleport)");
                    yield break;
                }




                // ---------------------------------------------------------------------------
                // 🔹 GIAI ĐOẠN 2: LOAD SCENE & GIẢI CỨU PLAYER
                // ---------------------------------------------------------------------------
                yield return Fade(1f); // Fade Out

                var loadOp = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
                while (!loadOp.isDone) yield return null;

                Scene newScene = SceneManager.GetSceneByName(nextSceneName);
                SceneManager.SetActiveScene(newScene);

                // Nếu đến menu scene -> destroy player, không move
                bool isMenuScene = _menuSceneNames.Contains(nextSceneName);
                GameObject playerToRescue = GameObject.FindWithTag("Player");
                if (playerToRescue != null)
                {
                    if (isMenuScene)
                    {
                        Destroy(playerToRescue.transform.root.gameObject);
                        _playerInstance = null;
                    }
                    else
                    {
                        GameObject playerRoot = playerToRescue.transform.root.gameObject;
                        if (playerRoot.scene != newScene)
                        {
                            playerRoot.transform.SetParent(null);
                            SceneManager.MoveGameObjectToScene(playerRoot, newScene);
                        }
                    }
                }

                // Unload Scene cũ
                for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
                {
                    Scene s = SceneManager.GetSceneAt(i);
                    if (s.name != newScene.name && s.name != this.gameObject.scene.name)
                    {
                        var unload = SceneManager.UnloadSceneAsync(s);
                        if (unload != null) while (!unload.isDone) yield return null;
                    }
                }

                // ---------------------------------------------------------------------------
                // 🔹 GIAI ĐOẠN 3: TÌM SPAWN POINT
                // ---------------------------------------------------------------------------
                Transform spawnTransform = null;
                SpawnPoint[] spawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
                foreach (var sp in spawns) { if (sp.SpawnID == spawnID) { spawnTransform = sp.transform; break; } }
                if (spawnTransform == null) spawnTransform = new GameObject("FallbackSpawn").transform;

                // ---------------------------------------------------------------------------
                // 🔹 GIAI ĐOẠN 4: CHờI 1 FRAME cho AnomalyManager và các script kịp Start()
                // ---------------------------------------------------------------------------
                yield return null;
                yield return new WaitForEndOfFrame();

                // ---------------------------------------------------------------------------
                // 🔹 GIAI ĐOẠN 5: XỬ LÝ PLAYER (CHỐT HẠ VỊ TRÍ CUỐI CÙNG)
                // ---------------------------------------------------------------------------

                if (!isMenuScene)
                {
                    var existingPlayer = GameObject.FindWithTag("Player");

                    if (existingPlayer == null)
                    {
                        // New Game: spawn player mới
                        _playerInstance = Instantiate(_playerPrefab, spawnTransform.position, spawnTransform.rotation);
                        EventManager.Notify(GameEvents.SceneTransition.OnPlayerSpawned, _playerInstance.transform);
                    }
                    else
                    {
                        Debug.Log($"[Transition] Teleport Player -> {spawnID}");
                        _playerInstance = existingPlayer;

                        var rb = _playerInstance.GetComponent<Rigidbody>();
                        if (rb)
                        {
                            rb.isKinematic = true;
                            rb.linearVelocity = Vector3.zero;
                            rb.angularVelocity = Vector3.zero;
                        }

                        _playerInstance.transform.position = spawnTransform.position;
                        _playerInstance.transform.rotation = spawnTransform.rotation;
                        Physics.SyncTransforms();

                        if (rb) rb.isKinematic = false;

                        EventManager.Notify(GameEvents.SceneTransition.OnPlayerSpawned, _playerInstance.transform);
                    }
                }

                // ---------------------------------------------------------------------------
                // 🔹 GIAI ĐOẠN 6: KẾT THÚC
                // ---------------------------------------------------------------------------

                // Cho AnomalyManager biết scene đã đen - trigger StartDay()
                EventManager.Notify(GameEvents.SceneTransition.OnBlackScreen);
                yield return null; // 1 frame để StartDay() chạy

                // Restore timeScale (có thể bị DecisionInteractable set = 0 trước đó)
                Time.timeScale = 1f;

                yield return Fade(0f); // Fade In
                InputManager.Instance?.TogglePlayerInput(true);
                success = true;
            }
            finally
            {
                // Dù có lỗi hay không, _isTransitioning phải về false
                _isTransitioning = false;
                if (!success)
                    Debug.LogError("TransitionRoutine gặp lỗi! Kiểm tra Inspector: _fadeCanvas và _playerPrefab đã được gán chưa?");
            }
            Debug.Log("DONE!");
        }

        #endregion


        /// <summary>
        /// Same-scene teleport (for Bed sleep, etc.) - Fade + Move player + Fade in
        /// </summary>
        private IEnumerator SameSceneTeleport(string spawnID)
        {
            // Debug.Log($"Same-scene teleport to: {spawnID}");

            // Chốt tốc độ fade rồi reset về default ngay
            float fadeSpeed = _nextFadeOutSpeed;
            _nextFadeOutSpeed = 2f;

            // Đóng băng mọi thứ: timeScale=0 dừng enemy, camera rotation, object
            // Chỉ cần khóa thêm bàn phím/chuột qua TogglePlayerInput
            Time.timeScale = 0f;
            InputManager.Instance?.TogglePlayerInput(false);

            // Fade out (unscaledDeltaTime vì timeScale = 0)
            yield return Fade(1f, fadeSpeed);

            // Find spawn point
            Transform spawnTransform = null;
            SpawnPoint[] spawns = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            foreach (var sp in spawns)
            {
                if (sp.SpawnID == spawnID)
                {
                    spawnTransform = sp.transform;
                    break;
                }
            }

            if (spawnTransform == null)
            {
                // Debug.LogWarning($"SpawnPoint '{spawnID}' not found!");
                Time.timeScale = 1f;
                InputManager.Instance?.TogglePlayerInput(true);
                yield return Fade(0f);
                yield break;
            }

            // Teleport player (màn hình đen)
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                var rb = player.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.isKinematic = true;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                player.transform.position = spawnTransform.position;
                player.transform.rotation = spawnTransform.rotation;
                Physics.SyncTransforms();

                if (rb) rb.isKinematic = false;
                // Debug.Log($"Player teleported to {spawnID}");
            }

            // Kích hoạt logic ngày mới trong lúc màn đen
            EventManager.Notify(GameEvents.SceneTransition.OnBlackScreen);

            // Fade in
            yield return Fade(0f);

            // Mở băng mọi thứ
            Time.timeScale = 1f;
            InputManager.Instance?.TogglePlayerInput(true);
        }

        private IEnumerator Fade(float targetAlpha, float speed = 2f)
        {
            if (_fadeCanvas == null) yield break;

            while (!Mathf.Approximately(_fadeCanvas.alpha, targetAlpha))
            {
                // Dùng unscaledDeltaTime — cần chạy kể cả khi Time.timeScale = 0
                _fadeCanvas.alpha = Mathf.MoveTowards(_fadeCanvas.alpha, targetAlpha, Time.unscaledDeltaTime * speed);
                yield return null;
            }
        }

    }
}
