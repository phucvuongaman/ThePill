using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace TheProject
{
    // Manages the game loop: Phase -> Day -> Decision -> Wrong/Correct.
    //
    // Setup:
    // 1. Place this on a GameObject in the Persistent scene.
    // 2. Fill in Phase Config and Phase Event Pools in Inspector.
    // 3. Each day is decided randomly so players can't predict patterns.
    //
    // Flow: OnPlayerSpawned -> StartGame() -> StartDay() -> player decides -> OnDecision(bool)
    //       Correct: NextDay() | Wrong: RegisterWrong()
    public class AnomalyManager : MonoBehaviour
    {
        public static AnomalyManager Instance { get; private set; }

        [Header("Phase Config")]
        [Tooltip("Total number of phases in one playthrough.")]
        [SerializeField] private int _totalPhases = 3;

        [Tooltip("Number of days per phase before advancing to the next.")]
        [SerializeField] private int _daysPerPhase = 10;

        [Tooltip("Chance that any given day has an anomaly. 0 = always normal, 1 = always anomaly.")]
        [Range(0f, 1f)]
        [SerializeField] private float _anomalyChance = 0.6f;

        [Tooltip("Max number of wrong answers before resetting to Phase 1.")]
        [SerializeField] private int _maxWrong = 3;

        [Header("Phase Event Pools")]
        [Tooltip("One PhasePoolSO per phase. Index 0 = Phase 1.")]
        [SerializeField] private List<PhasePoolSO> _phasePools = new();

        [Header("Scene Transition")]
        [Tooltip("Scene name for each phase. Index 0 = Phase 1.")]
        [SerializeField] private List<string> _phaseSceneNames = new();

        [Tooltip("SpawnID to use when teleporting the player at the start of a day.")]
        [SerializeField] private string _startSpawnID = "START";

        [Tooltip("Scene names that are menus. When loaded, all game state is reset.")]
        [SerializeField] private List<string> _menuSceneNames = new() { "MainMenu" };

        [Header("Game Over")]
        [Tooltip("How long to wait for the game over cutscene before timing out.")]
        [SerializeField] private float _gameOverTimeout = 10f;

        // Runtime state
        public int CurrentPhase { get; private set; } = 0;
        public int CurrentDay { get; private set; } = 0;
        public int WrongCount { get; private set; } = 0;

        // Each day rolls independently so players can't figure out patterns by counting.
        public bool IsCurrentDayAnomaly { get; private set; } = false;

        private List<string> _usedEventIDs = new();
        private AnomalyEventData _currentEvent = null;
        private bool _gameStarted = false;
        private bool _gameOverDone = false;
        private bool _caughtEffectDone = false;

        public string CurrentEventID => _currentEvent?.eventID ?? "NORMAL";
        private const string NORMAL_EVENT_ID = "NORMAL";

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            EventManager.AddObserver<Transform>(GameEvents.SceneTransition.OnPlayerSpawned, HandlePlayerSpawned);
            EventManager.AddObserver(GameEvents.SceneTransition.OnBlackScreen, StartDay);
            EventManager.AddObserver(GameEvents.Anomaly.OnGameOverDone, OnGameOverDone);
            EventManager.AddObserver(GameEvents.Anomaly.OnPlayerCaughtDone, OnPlayerCaughtEffectDone);
            EventManager.AddObserver(GameEvents.Anomaly.OnPlayerCaught, OnCaughtByThreat);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            EventManager.RemoveListener<Transform>(GameEvents.SceneTransition.OnPlayerSpawned, HandlePlayerSpawned);
            EventManager.RemoveListener(GameEvents.SceneTransition.OnBlackScreen, StartDay);
            EventManager.RemoveListener(GameEvents.Anomaly.OnGameOverDone, OnGameOverDone);
            EventManager.RemoveListener(GameEvents.Anomaly.OnPlayerCaughtDone, OnPlayerCaughtEffectDone);
            EventManager.RemoveListener(GameEvents.Anomaly.OnPlayerCaught, OnCaughtByThreat);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // When returning to a menu scene, reset all state so New Game starts fresh.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_menuSceneNames.Contains(scene.name)) return;

            _gameStarted = false;
            CurrentPhase = 0;
            CurrentDay = 0;
            WrongCount = 0;
            _usedEventIDs.Clear();
            _currentEvent = null;
        }

        #endregion

        #region Public API

        // Called once when the player first spawns. Guards against firing twice.
        private void HandlePlayerSpawned(Transform _)
        {
            if (_gameStarted) return;
            StartGame();
        }

        public void StartGame()
        {
            _gameStarted = true;
            CurrentPhase = 0;
            CurrentDay = 0;
            WrongCount = 0;
            _usedEventIDs.Clear();
            StartDay();
        }

        // Called by DecisionPoint when the player submits their answer.
        // playerSaysAnomalyExists = true means the player chose "something is wrong".
        public void OnDecision(bool playerSaysAnomalyExists)
        {
            bool correct = (playerSaysAnomalyExists == IsCurrentDayAnomaly);

            if (correct)
            {
                Debug.Log($"[AnomalyManager] Day {CurrentDay + 1}: Correct.");
                EventManager.Notify(GameEvents.Anomaly.OnDayCorrect, new AnomalyResultData
                {
                    Phase = CurrentPhase + 1,
                    Day = CurrentDay + 2,
                    WasAnomaly = IsCurrentDayAnomaly
                });
                NextDay();
            }
            else if (!playerSaysAnomalyExists && IsCurrentDayAnomaly)
            {
                // Player said normal, but there was an anomaly.
                // Trigger the "caught" effect (red flash + sound) before registering the wrong.
                Debug.Log($"[AnomalyManager] Day {CurrentDay + 1}: Wrong — said normal but anomaly was present.");
                EventManager.Notify(GameEvents.Anomaly.OnPlayerCaught);
            }
            else
            {
                // Player said anomaly, but the day was normal.
                Debug.Log($"[AnomalyManager] Day {CurrentDay + 1}: Wrong — said anomaly but day was normal.");
                RegisterWrong();
            }
        }

        // Fires when an enemy physically catches the player during an anomaly day.
        private void OnCaughtByThreat()
        {
            Debug.Log("[AnomalyManager] Player caught by threat.");
            StartCoroutine(CaughtSequence());
        }

        // Waits for the caught visual effect to finish, then registers the wrong.
        private IEnumerator CaughtSequence()
        {
            _caughtEffectDone = false;

            float elapsed = 0f;
            const float timeout = 5f;
            while (!_caughtEffectDone && elapsed < timeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!_caughtEffectDone)
                Debug.LogWarning("[AnomalyManager] CaughtEffect timed out. Is EnemyCaughtEffect in the scene?");

            RegisterWrong();
        }

        private void OnPlayerCaughtEffectDone() => _caughtEffectDone = true;

        #endregion

        #region Game Flow

        private void StartDay()
        {
            // Day 1 of each phase is always normal to let the player get familiar with the environment.
            if (CurrentDay == 0)
            {
                IsCurrentDayAnomaly = false;
                _currentEvent = null;
            }
            else
            {
                // Each day rolls independently so players can't predict when anomalies happen.
                IsCurrentDayAnomaly = Random.value < _anomalyChance;

                _currentEvent = null;
                if (IsCurrentDayAnomaly)
                {
                    _currentEvent = PickRandomEvent();
                    if (_currentEvent != null)
                        _usedEventIDs.Add(_currentEvent.eventID);
                    else
                        IsCurrentDayAnomaly = false;
                }
            }

            string broadcastID = _currentEvent?.eventID ?? NORMAL_EVENT_ID;
            Debug.Log($"[AnomalyManager] Phase {CurrentPhase + 1} | Day {CurrentDay + 1} | {(IsCurrentDayAnomaly ? "ANOMALY" : "NORMAL")} | Event: {broadcastID}");

            EventManager.Notify(GameEvents.Anomaly.OnDayStart, broadcastID);
        }

        private void NextDay()
        {
            int nextDay = CurrentDay + 1;

            if (nextDay >= _daysPerPhase)
            {
                NextPhase();
            }
            else
            {
                CurrentDay = nextDay;
                // SameSceneTeleport fires OnBlackScreen when dark, which calls StartDay().
                SceneTransitionManager.Instance?.TransitionTo(
                    SceneManager.GetActiveScene().name, _startSpawnID);
            }
        }

        private void NextPhase()
        {
            int nextPhase = CurrentPhase + 1;

            if (nextPhase >= _totalPhases)
            {
                Debug.Log("[AnomalyManager] Game complete.");
                EventManager.Notify(GameEvents.Anomaly.OnGameComplete);
                return;
            }

            CurrentPhase = nextPhase;
            CurrentDay = 0;
            _usedEventIDs.Clear();

            EventManager.Notify(GameEvents.Anomaly.OnPhaseChanged, CurrentPhase);

            string sceneName = GetSceneForPhase(CurrentPhase);
            if (!string.IsNullOrEmpty(sceneName))
                SceneTransitionManager.Instance?.TransitionTo(sceneName, _startSpawnID);
            else
                StartDay();
        }

        private void RegisterWrong()
        {
            WrongCount++;
            EventManager.Notify(GameEvents.Anomaly.OnWrong, WrongCount);

            if (WrongCount >= _maxWrong)
                StartCoroutine(GameOverSequence());
            else
                ResetToCurrentPhaseStart();
        }

        private void ResetToCurrentPhaseStart()
        {
            Debug.Log($"[AnomalyManager] Wrong {WrongCount}/{_maxWrong}. Resetting to start of Phase {CurrentPhase + 1}.");

            // Deactivate any active anomalies immediately before the teleport fade.
            EventManager.Notify(GameEvents.Anomaly.OnGameReset);

            CurrentDay = 0;
            _usedEventIDs.Clear();

            string sceneName = GetSceneForPhase(CurrentPhase);
            if (!string.IsNullOrEmpty(sceneName))
                SceneTransitionManager.Instance?.TransitionTo(sceneName, _startSpawnID);
            else
                StartDay();
        }

        #endregion

        #region Game Over

        // Waits for the game over cutscene to finish, then resets to Phase 1.
        // Times out after _gameOverTimeout seconds in case the handler never fires OnGameOverDone.
        private IEnumerator GameOverSequence()
        {
            Debug.Log("[AnomalyManager] Game over sequence starting.");

            InputManager.Instance?.TogglePlayerInput(false);

            _gameOverDone = false;
            EventManager.Notify(GameEvents.Anomaly.OnGameOver);

            float elapsed = 0f;
            while (!_gameOverDone && elapsed < _gameOverTimeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!_gameOverDone)
                Debug.LogWarning($"[AnomalyManager] Game over timed out after {_gameOverTimeout}s. Did GameOverCutsceneHandler call OnGameOverDone?");

            DoActualReset();
        }

        private void OnGameOverDone() => _gameOverDone = true;

        // Full reset back to Phase 1. Only runs after the game over cutscene or timeout.
        private void DoActualReset()
        {
            Debug.Log("[AnomalyManager] Full reset to Phase 1.");

            CurrentPhase = 0;
            CurrentDay = 0;
            WrongCount = 0;
            _usedEventIDs.Clear();

            EventManager.Notify(GameEvents.Anomaly.OnGameReset);

            string sceneName = GetSceneForPhase(0);
            if (!string.IsNullOrEmpty(sceneName))
                SceneTransitionManager.Instance?.TransitionTo(sceneName, _startSpawnID);
            else
                StartDay();
        }

        #endregion

        #region Event Pool

        private AnomalyEventData PickRandomEvent()
        {
            if (CurrentPhase >= _phasePools.Count || _phasePools[CurrentPhase] == null)
            {
                Debug.LogWarning($"[AnomalyManager] No pool for Phase {CurrentPhase + 1}.");
                return null;
            }

            var pool = _phasePools[CurrentPhase].events;
            var available = pool.FindAll(e => !string.IsNullOrEmpty(e.eventID)
                                              && !_usedEventIDs.Contains(e.eventID));

            if (available.Count == 0)
            {
                Debug.LogError($"[AnomalyManager] Phase {CurrentPhase + 1} pool is empty. Add more events.");
                return null;
            }

            return available[Random.Range(0, available.Count)];
        }

        #endregion

        #region Helpers

        private string GetSceneForPhase(int phaseIndex)
        {
            if (_phaseSceneNames.Count > phaseIndex)
                return _phaseSceneNames[phaseIndex];
            return string.Empty;
        }

        #endregion
    }


    [System.Serializable]
    public class AnomalyEventData
    {
        [Tooltip("Unique event ID. Must match _linkedEventID in AnomalyVariantGroup.")]
        public string eventID;

        [TextArea(1, 3)]
        public string description;

        [Tooltip("True if this anomaly has an enemy that chases the player.")]
        public bool isThreat;
    }

}