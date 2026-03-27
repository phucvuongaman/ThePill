using System.Collections;
using UnityEngine;

namespace TheProject
{
    /// <summary>
    /// [ANOMALY MOVER]
    /// Kế thừa AnomalyBase.
    /// Di chuyển object đến vị trí bất thường khi anomaly active.
    /// Deactivate() đưa object về vị trí gốc.
    /// Script luôn active → luôn nhận event.
    ///
    /// SETUP TRONG UNITY EDITOR:
    /// 1. Gắn script lên object cần di chuyển
    /// 2. Gán _targetTransform = vị trí đích khi anomaly (có thể là GameObject rỗng làm marker)
    /// 3. Set _moveDuration = thời gian di chuyển (0 = tức thì)
    /// 4. Set _linkedEventID = eventID của AnomalyEventSO
    ///
    /// MẸO: Tạo 1 GameObject rỗng đặt ở vị trí bất thường làm "target marker", kéo vào _targetTransform
    /// </summary>
    public class AnomalyMover : AnomalyBase
    {
        [Header("Move Settings")]
        [Tooltip("Vị trí đích khi anomaly active. Dùng GameObject rỗng làm marker trong scene.")]
        [SerializeField] private Transform _targetTransform;

        [Tooltip("Thời gian di chuyển (giây). 0 = dịch chuyển tức thì.")]
        [SerializeField] private float _moveDuration = 0.5f;

        // Vị trí gốc (Normal Day) — snapshot khi Start()
        private Vector3 _originPosition;
        private Quaternion _originRotation;

        private Coroutine _moveRoutine;

        private void Start()
        {
            // Ghi lại vị trí ban đầu = Normal Day
            _originPosition = transform.position;
            _originRotation = transform.rotation;
        }

        protected override void Activate()
        {
            if (_targetTransform == null)
            {
                Debug.LogWarning($"[AnomalyMover] {gameObject.name}: _targetTransform chưa được gán!");
                return;
            }

            MoveTo(_targetTransform.position, _targetTransform.rotation);
        }

        protected override void Deactivate()
        {
            // Về vị trí gốc (Normal Day)
            MoveTo(_originPosition, _originRotation);
        }

        private void MoveTo(Vector3 targetPos, Quaternion targetRot)
        {
            if (_moveRoutine != null)
                StopCoroutine(_moveRoutine);

            if (_moveDuration <= 0f)
            {
                // Tức thì
                transform.position = targetPos;
                transform.rotation = targetRot;
            }
            else
            {
                _moveRoutine = StartCoroutine(MoveRoutine(targetPos, targetRot));
            }
        }

        private IEnumerator MoveRoutine(Vector3 targetPos, Quaternion targetRot)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            float elapsed = 0f;

            while (elapsed < _moveDuration)
            {
                float t = elapsed / _moveDuration;
                // SmoothStep: chậm ở đầu và cuối, nhanh ở giữa — tự nhiên hơn Lerp thẳng
                t = t * t * (3f - 2f * t);

                transform.position = Vector3.Lerp(startPos, targetPos, t);
                transform.rotation = Quaternion.Lerp(startRot, targetRot, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPos;
            transform.rotation = targetRot;
        }
    }
}
