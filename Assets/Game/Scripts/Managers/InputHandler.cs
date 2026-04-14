using System;
using EndlessPuzzle.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;

#if ENDLESS_PUZZLE_USE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Utilities;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
#endif

namespace EndlessPuzzle.Managers
{
    public sealed class InputHandler : MonoBehaviour
    {
        private readonly RaycastHit[] _raycastHits = new RaycastHit[4];

        private Camera _worldCamera;
        private bool _inputEnabled;

        public event Action<TargetObject> TargetTapped;

#if ENDLESS_PUZZLE_USE_INPUT_SYSTEM
        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            TouchSimulation.Enable();
        }

        private void OnDisable()
        {
            TouchSimulation.Disable();
            EnhancedTouchSupport.Disable();
        }
#endif

        public void Initialize(Camera worldCamera)
        {
            _worldCamera = worldCamera;
        }

        public void SetInputEnabled(bool isEnabled)
        {
            _inputEnabled = isEnabled;
        }

        private void Update()
        {
            if (!_inputEnabled || _worldCamera == null)
            {
                return;
            }

#if ENDLESS_PUZZLE_USE_INPUT_SYSTEM
            if (TryReadNewInputSystemPress(out Vector2 screenPosition))
            {
                TryHitTarget(screenPosition);
                return;
            }
#else
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase != TouchPhase.Began)
                {
                    return;
                }

                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return;
                }

                TryHitTarget(touch.position);
                return;
            }

            if (!Application.isMobilePlatform && Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                TryHitTarget(Input.mousePosition);
            }
#endif
        }

        private void TryHitTarget(Vector2 screenPosition)
        {
            if (!IsValidScreenPosition(screenPosition))
            {
                return;
            }

            Ray ray = _worldCamera.ScreenPointToRay(screenPosition);
            int hitCount = Physics.RaycastNonAlloc(ray, _raycastHits, 100f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            if (hitCount <= 0)
            {
                return;
            }

            float nearestDistance = float.MaxValue;
            TargetObject nearestTarget = null;

            for (int i = 0; i < hitCount; i++)
            {
                if (_raycastHits[i].distance >= nearestDistance)
                {
                    continue;
                }

                if (_raycastHits[i].collider != null && _raycastHits[i].collider.TryGetComponent(out TargetObject targetObject))
                {
                    nearestDistance = _raycastHits[i].distance;
                    nearestTarget = targetObject;
                }
            }

            if (nearestTarget != null)
            {
                TargetTapped?.Invoke(nearestTarget);
            }
        }

        private static bool IsValidScreenPosition(Vector2 screenPosition)
        {
            return !float.IsNaN(screenPosition.x)
                && !float.IsNaN(screenPosition.y)
                && !float.IsInfinity(screenPosition.x)
                && !float.IsInfinity(screenPosition.y);
        }

#if ENDLESS_PUZZLE_USE_INPUT_SYSTEM
        private static bool TryReadNewInputSystemPress(out Vector2 screenPosition)
        {
            ReadOnlyArray<Touch> activeTouches = Touch.activeTouches;
            for (int i = 0; i < activeTouches.Count; i++)
            {
                Touch touch = activeTouches[i];
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    screenPosition = touch.screenPosition;
                    return true;
                }
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPosition = Mouse.current.position.ReadValue();
                return true;
            }

            screenPosition = default;
            return false;
        }
#endif
    }
}
