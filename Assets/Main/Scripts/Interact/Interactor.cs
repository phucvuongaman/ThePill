using UnityEngine;
using UnityEngine.InputSystem;

namespace TheProject
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private float maxDistance = 5f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private LayerMask detectionLayer;

        // Kéo RawImage (cái hiển thị Render Texture) vào đây trong Inspector
        // private RawImage _renderTextureDisplay;


        private BaseInteractable _currentTarget;

        public BaseInteractable CurrentTarget => _currentTarget;


        private Camera CurrentCamera
        {
            get
            {
                // // Ưu tiên 1: Lấy từ CameraController 
                if (CameraController.Instance != null && CameraController.Instance.UnityCamera != null)
                {

                    return CameraController.Instance.UnityCamera;
                }

                // Ưu tiên 2: Chữa cháy nếu chưa có Controller
                return Camera.main;
            }
        }

        // Gọi trong các player state cần sử dụng
        public void OnFocus()
        {
            if (CurrentCamera == null) return;
            //  Ray ray = CurrentCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            // Chuyển đổi tọa độ chuột: Screen → RawImage → Render Texture
            Vector2 screenPos = Mouse.current.position.ReadValue();
            if (!TryGetRenderTexturePoint(screenPos, out Vector2 rtPoint)) return;

            Ray ray = CurrentCamera.ScreenPointToRay(rtPoint);
            RaycastHit hit;

            // 1. Bắn tia Ray
            if (Physics.Raycast(ray, out hit, maxDistance, detectionLayer))
            {
                var newTarget = hit.transform.GetComponent<BaseInteractable>();

                // Kiểm tra kỹ điều kiện
                bool isValidTarget = newTarget != null
                                     && (interactableLayer.value & (1 << hit.transform.gameObject.layer)) != 0;

                if (isValidTarget)
                {
                    if (newTarget != _currentTarget) FocusTarget(newTarget);
                }
                else
                {
                    // 👇 CÁI ELSE BẠN NÓI ĐÂY: Nhìn trúng tường -> Xóa ngay
                    ClearTarget();
                }
            }
            else
            {
                // 2. Nhìn vào hư không -> Xóa ngay
                ClearTarget();
            }
        }

        // Đăng ký với nút E InputManager ở Context
        public void InteractPress()
        {
            if (_currentTarget != null)
            {
                // 👇 FIX THÊM: Kiểm tra xem vật đó có bị hủy (Destroy) bất ngờ không
                if (_currentTarget == null || _currentTarget.gameObject == null)
                {
                    ClearTarget();
                    return;
                }

                _currentTarget.OnInteractPress(this);
            }
            // else
            // {
            //     Debug.Log("No interactable target in range.");
            // }
        }
        // Đăng ký với nút E InputManager ở Context
        public void InteractHold()
        {
            if (_currentTarget != null)
            {
                if (_currentTarget == null || _currentTarget.gameObject == null)
                {
                    ClearTarget();
                    return;
                }
                _currentTarget.OnInteractHold(this);
            }
            // else
            // {
            //     Debug.Log("No interactable target to hold interaction with.");
            // }
        }

        // Chuyển tọa độ chuột trên màn hình thật -> tọa độ pixel trên Render Texture
        private bool TryGetRenderTexturePoint(Vector2 screenMousePos, out Vector2 rtPoint)
        {
            rtPoint = Vector2.zero;

            // Nếu không có CameraController hoặc RawImage thì dùng tọa độ gốc (fallback)
            if (CameraController.Instance == null || CameraController.Instance.RenderTextureDisplay == null)
            {
                rtPoint = screenMousePos;
                return true;
            }

            RectTransform rect = CameraController.Instance.RenderTextureDisplay.rectTransform;

            // Kiểm tra chuột có đang nằm trong vùng RawImage không
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rect, screenMousePos, null, out Vector2 localPoint))
            {
                return false;
            }

            // localPoint nằm trong khoảng [-width/2, width/2] và [-height/2, height/2]
            // Normalize về [0, 1]
            Vector2 size = rect.rect.size;
            float normalizedX = (localPoint.x / size.x) + 0.5f;
            float normalizedY = (localPoint.y / size.y) + 0.5f;

            // Nhân với kích thước Render Texture để ra pixel thực
            RenderTexture rt = CurrentCamera.targetTexture;
            if (rt == null)
            {
                rtPoint = screenMousePos;
                return true;
            }

            rtPoint = new Vector2(normalizedX * rt.width, normalizedY * rt.height);
            return true;
        }

        private void FocusTarget(BaseInteractable newTarget)
        {
            if (_currentTarget != null) _currentTarget.OnLoseFocus();

            _currentTarget = newTarget;
            _currentTarget.OnFocus();
            // UIManager.Instance.ShowInteractText(_currentTarget.InteractableName);
        }

        private void ClearTarget()
        {
            if (_currentTarget != null)
            {
                _currentTarget.OnLoseFocus();
                _currentTarget = null;
                // UIManager.Instance.HideInteractText();
            }
        }


        private void OnDisable()
        {
            ClearTarget();
        }
    }
}

