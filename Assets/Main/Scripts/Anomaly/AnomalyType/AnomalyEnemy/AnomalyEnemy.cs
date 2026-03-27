using UnityEngine;
using UnityEngine.AI;

namespace TheProject
{
    // Context cho AnomalyEnemyStateMachine — giữ refs và config
    // GO này phải inactive mặc định, AnomalyVariantGroup sẽ bật/tắt theo ngày
    [RequireComponent(typeof(NavMeshAgent), typeof(AnomalyEnemyStateMachine))]
    public class AnomalyEnemy : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Khớp với eventID trong AnomalyEventSO tương ứng.")]
        [SerializeField] private string _linkedEventID;

        [Header("Chase")]
        [SerializeField] private float _chaseSpeed = 3.5f;
        [SerializeField] private string _playerTag = "Player";

        [Header("Animator")]
        [SerializeField] private string _animSpeed = "Speed";
        [SerializeField] private string _animCaught = "Caught";

        [Header("Audio")]
        [SerializeField] private AudioClip _idleAudioClip;
        [SerializeField] private AudioClip _chaseAudioClip;
        [SerializeField] private AudioSource _enemyAudioSource;

        public NavMeshAgent Agent { get; private set; }
        public Animator Anim { get; private set; }
        public Transform PlayerTransform { get; private set; }
        public Vector3 OriginPosition { get; private set; }
        public Quaternion OriginRotation { get; private set; }

        public string AnimSpeed => _animSpeed;
        public string AnimCaught => _animCaught;
        public string LinkedEventID => _linkedEventID;
        public string PlayerTag => _playerTag;
        public float ChaseSpeed => _chaseSpeed;
        public AudioClip IdleAudioClip => _idleAudioClip;
        public AudioClip ChaseAudioClip => _chaseAudioClip;
        public AudioSource EnemyAudioSource => _enemyAudioSource;

        private bool _isFirstEnable = true;

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            Anim = GetComponentInChildren<Animator>();
            Agent.speed = _chaseSpeed;
            Agent.isStopped = true;
            OriginPosition = transform.position;
            OriginRotation = transform.rotation;

            if (Anim == null)
                Debug.LogWarning($"[AnomalyEnemy] {name}: thiếu Animator trong children");
        }

        private void OnEnable()
        {
            if (_isFirstEnable)
            {
                _isFirstEnable = false;
            }
            else
            {
                // Warp về gốc — tránh đứng chỗ bắt player ngày trước
                if (Agent != null && Agent.isOnNavMesh)
                {
                    Agent.isStopped = true;
                    Agent.ResetPath();
                    Agent.Warp(OriginPosition);
                }
                transform.rotation = OriginRotation;
            }

            PlayerTransform = GameObject.FindWithTag(_playerTag)?.transform;
        }

        public void RefreshPlayerRef()
        {
            PlayerTransform = GameObject.FindWithTag(_playerTag)?.transform;
        }

        public void ResetToOrigin()
        {
            if (Agent != null && Agent.isOnNavMesh)
            {
                Agent.isStopped = true;
                Agent.ResetPath();
                Agent.Warp(OriginPosition);
            }
            transform.rotation = OriginRotation;
        }
    }
}
