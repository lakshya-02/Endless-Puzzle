using EndlessPuzzle.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace EndlessPuzzle.Gameplay
{
    public sealed class TargetObject : MonoBehaviour
    {
        private const string VisualChildName = "Visual";
        private const string LabelChildName = "Label";

        private Transform _cachedTransform;
        private Transform _visualTransform;
        private Transform _labelTransform;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private TextMesh _label;
        private BoxCollider _boxCollider;
        private SphereCollider _sphereCollider;
        private CapsuleCollider _capsuleCollider;
        private float _labelColorPhaseOffset;

        public int ActualColorIndex { get; private set; } = -1;
        public int ShapeIndex { get; private set; } = -1;
        public int LabelColorIndex { get; private set; } = -1;

        public Transform CachedTransform => _cachedTransform;

        public void CacheReferences(
            Transform visualTransform,
            Transform labelTransform,
            MeshFilter meshFilter,
            MeshRenderer meshRenderer,
            TextMesh label,
            BoxCollider boxCollider,
            SphereCollider sphereCollider,
            CapsuleCollider capsuleCollider)
        {
            _cachedTransform = transform;
            _visualTransform = visualTransform;
            _labelTransform = labelTransform;
            _meshFilter = meshFilter;
            _meshRenderer = meshRenderer;
            _label = label;
            _boxCollider = boxCollider;
            _sphereCollider = sphereCollider;
            _capsuleCollider = capsuleCollider;

            _meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
        }

        private void Awake()
        {
            TryPopulateReferences();
        }

        public void Configure(
            GameConfigDatabase config,
            PrimitiveMeshLibrary meshLibrary,
            MaterialLibrary materialLibrary,
            int actualColorIndex,
            int shapeIndex,
            int labelColorIndex,
            Vector3 worldPosition,
            float baseScale,
            Vector3 visualEulerAngles,
            Quaternion labelWorldRotation)
        {
            EnsureReferences();

            ActualColorIndex = actualColorIndex;
            ShapeIndex = shapeIndex;
            LabelColorIndex = labelColorIndex;

            ShapeDefinition shape = config.GetShapeDefinition(shapeIndex);
            float targetScale = baseScale * shape.UniformScale;

            _cachedTransform.position = worldPosition;
            _cachedTransform.localScale = Vector3.one * targetScale;
            _visualTransform.localRotation = Quaternion.Euler(visualEulerAngles);
            _meshFilter.sharedMesh = meshLibrary.GetMesh(shape.PrimitiveType);
            _meshRenderer.sharedMaterial = materialLibrary.GetMaterial(actualColorIndex);

            _label.text = config.GetColorName(labelColorIndex);
            _label.color = Color.white;
            _labelTransform.localPosition = Vector3.up * GetLabelOffsetY(shape.PrimitiveType, targetScale);
            _labelTransform.localRotation = labelWorldRotation;
            _labelTransform.localScale = Vector3.one * Mathf.Max(0.85f, 1f / Mathf.Max(0.01f, targetScale));
            _labelColorPhaseOffset = Random.Range(0f, config.SpawnSettings.LabelColorCycleSeconds * config.ColorCount);

            EnableCollider(shape.PrimitiveType);
        }

        public bool Matches(int colorIndex, int shapeIndex, QuestionAttributeMask mask)
        {
            if ((mask & QuestionAttributeMask.Color) != 0 && ActualColorIndex != colorIndex)
            {
                return false;
            }

            if ((mask & QuestionAttributeMask.Shape) != 0 && ShapeIndex != shapeIndex)
            {
                return false;
            }

            return true;
        }

        public void MoveDown(float fallDistance)
        {
            EnsureReferences();

            Vector3 position = _cachedTransform.position;
            position.y -= fallDistance;
            _cachedTransform.position = position;
        }

        public void AnimateLabelColor(GameConfigDatabase config, float animationTime, float cycleDuration)
        {
            EnsureReferences();

            if (cycleDuration <= 0f || config.ColorCount <= 0)
            {
                _label.color = Color.white;
                return;
            }

            int colorIndex = Mathf.FloorToInt((animationTime + _labelColorPhaseOffset) / cycleDuration) % config.ColorCount;
            if (colorIndex < 0)
            {
                colorIndex += config.ColorCount;
            }

            _label.color = config.GetUnityColor(colorIndex);
        }

        public void ResetPooledState()
        {
            EnsureReferences();

            ActualColorIndex = -1;
            ShapeIndex = -1;
            LabelColorIndex = -1;
            _boxCollider.enabled = false;
            _sphereCollider.enabled = false;
            _capsuleCollider.enabled = false;
            _label.text = string.Empty;
            _label.color = Color.white;
            _labelColorPhaseOffset = 0f;
        }

        private void EnableCollider(ShapePrimitiveType primitiveType)
        {
            _boxCollider.enabled = false;
            _sphereCollider.enabled = false;
            _capsuleCollider.enabled = false;

            switch (primitiveType)
            {
                case ShapePrimitiveType.Cube:
                    _boxCollider.enabled = true;
                    break;
                case ShapePrimitiveType.Sphere:
                    _sphereCollider.enabled = true;
                    break;
                case ShapePrimitiveType.Capsule:
                case ShapePrimitiveType.Cylinder:
                    _capsuleCollider.enabled = true;
                    break;
            }
        }

        private static float GetLabelOffsetY(ShapePrimitiveType primitiveType, float targetScale)
        {
            float baseHeight;
            switch (primitiveType)
            {
                case ShapePrimitiveType.Capsule:
                case ShapePrimitiveType.Cylinder:
                    baseHeight = 1.02f;
                    break;
                case ShapePrimitiveType.Sphere:
                    baseHeight = 0.64f;
                    break;
                case ShapePrimitiveType.Cube:
                default:
                    baseHeight = 0.7f;
                    break;
            }

            return (baseHeight * targetScale) + 0.18f;
        }

        private void EnsureReferences()
        {
            TryPopulateReferences();

            if (_visualTransform == null ||
                _labelTransform == null ||
                _meshFilter == null ||
                _meshRenderer == null ||
                _label == null ||
                _boxCollider == null ||
                _sphereCollider == null ||
                _capsuleCollider == null)
            {
                throw new MissingReferenceException($"TargetObject '{name}' is missing one or more required child/component references.");
            }
        }

        private void TryPopulateReferences()
        {
            if (_cachedTransform == null)
            {
                _cachedTransform = transform;
            }

            if (_visualTransform == null)
            {
                _visualTransform = _cachedTransform.Find(VisualChildName);
            }

            if (_labelTransform == null)
            {
                _labelTransform = _cachedTransform.Find(LabelChildName);
            }

            if (_meshFilter == null && _visualTransform != null)
            {
                _meshFilter = _visualTransform.GetComponent<MeshFilter>();
            }

            if (_meshRenderer == null && _visualTransform != null)
            {
                _meshRenderer = _visualTransform.GetComponent<MeshRenderer>();
            }

            if (_label == null && _labelTransform != null)
            {
                _label = _labelTransform.GetComponent<TextMesh>();
            }

            if (_boxCollider == null)
            {
                _boxCollider = GetComponent<BoxCollider>();
            }

            if (_sphereCollider == null)
            {
                _sphereCollider = GetComponent<SphereCollider>();
            }

            if (_capsuleCollider == null)
            {
                _capsuleCollider = GetComponent<CapsuleCollider>();
            }

            if (_meshRenderer != null)
            {
                _meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                _meshRenderer.receiveShadows = false;
            }
        }
    }
}
