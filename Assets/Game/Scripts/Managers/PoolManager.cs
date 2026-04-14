using EndlessPuzzle.Data;
using EndlessPuzzle.Gameplay;
using EndlessPuzzle.Pooling;
using UnityEngine;

namespace EndlessPuzzle.Managers
{
    public sealed class PoolManager : MonoBehaviour
    {
        private ComponentPool<TargetObject> _targetPool;
        private PrimitiveMeshLibrary _meshLibrary;
        private MaterialLibrary _materialLibrary;

        public PrimitiveMeshLibrary MeshLibrary => _meshLibrary;
        public MaterialLibrary MaterialLibrary => _materialLibrary;

        public void Initialize(GameConfigDatabase config, PrototypePresentationSettings presentationSettings)
        {
            Transform poolRoot = new GameObject("TargetPool").transform;
            poolRoot.SetParent(transform, false);

            _meshLibrary = new PrimitiveMeshLibrary();
            _materialLibrary = new MaterialLibrary(config, presentationSettings != null ? presentationSettings.ColorMaterialPalette : null);

            Font worldLabelFont = presentationSettings != null ? presentationSettings.WorldLabelFont : null;
            TargetObject prototype = TargetObjectFactory.CreatePrototype(poolRoot, worldLabelFont);
            prototype.name = "TargetPrototype";
            _targetPool = new ComponentPool<TargetObject>(prototype, config.SpawnSettings.PoolPrewarmCount, poolRoot);
        }

        public TargetObject GetTarget()
        {
            return _targetPool.Get();
        }

        public void Release(TargetObject targetObject)
        {
            targetObject.ResetPooledState();
            _targetPool.Release(targetObject);
        }
    }
}
