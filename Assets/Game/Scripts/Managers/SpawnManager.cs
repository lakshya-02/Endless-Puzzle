using System.Collections.Generic;
using EndlessPuzzle.Data;
using EndlessPuzzle.Gameplay;
using UnityEngine;

namespace EndlessPuzzle.Managers
{
    public sealed class SpawnManager : MonoBehaviour
    {
        private const int LabelColorAnimationUnlockScore = 50;

        private readonly List<TargetObject> _activeObjects = new List<TargetObject>(16);

        private GameConfigDatabase _config;
        private PoolManager _poolManager;
        private Transform _gameplayRoot;
        private Quaternion _labelRotation;
        private bool[] _usedTargetCombos;
        private float _spawnTimer;
        private float _labelAnimationTime;
        private bool _isRunning;

        public IReadOnlyList<TargetObject> ActiveObjects => _activeObjects;
        public int ActiveCount => _activeObjects.Count;

        public void Initialize(GameConfigDatabase config, PoolManager poolManager, Transform gameplayRoot, Camera worldCamera)
        {
            _config = config;
            _poolManager = poolManager;
            _gameplayRoot = gameplayRoot;
            _labelRotation = worldCamera.transform.rotation;
            _usedTargetCombos = new bool[_config.TotalUniqueTargetCombos];
        }

        public void BeginRun()
        {
            _isRunning = true;
            _spawnTimer = 0f;
            _labelAnimationTime = 0f;
        }

        public void StopRun()
        {
            _isRunning = false;
        }

        public void ResetState()
        {
            for (int i = _activeObjects.Count - 1; i >= 0; i--)
            {
                _poolManager.Release(_activeObjects[i]);
            }

            _activeObjects.Clear();

            for (int i = 0; i < _usedTargetCombos.Length; i++)
            {
                _usedTargetCombos[i] = false;
            }

            _spawnTimer = 0f;
            _labelAnimationTime = 0f;
        }

        public void FillToTarget(RuntimeDifficulty difficulty)
        {
            int desiredCount = Mathf.Min(difficulty.MaxActiveObjects, _config.TotalUniqueTargetCombos);
            while (_activeObjects.Count < desiredCount)
            {
                SpawnOne(difficulty, useSpawnStacking: true);
            }
        }

        public int TickMovement(float deltaTime, float fallSpeed, BoundaryDetector boundaryDetector, int currentScore)
        {
            if (!_isRunning)
            {
                return 0;
            }

            _labelAnimationTime += deltaTime;
            float fallDistance = fallSpeed * deltaTime;
            int missedCount = 0;
            float cycleDuration = currentScore >= LabelColorAnimationUnlockScore ? _config.SpawnSettings.LabelColorCycleSeconds : 0f;

            for (int i = _activeObjects.Count - 1; i >= 0; i--)
            {
                TargetObject targetObject = _activeObjects[i];
                targetObject.MoveDown(fallDistance);
                targetObject.AnimateLabelColor(_config, _labelAnimationTime, cycleDuration);

                if (boundaryDetector.HasCrossedFailBoundary(targetObject.CachedTransform.position.y))
                {
                    _activeObjects.RemoveAt(i);
                    _usedTargetCombos[GetComboKey(targetObject.ActualColorIndex, targetObject.ShapeIndex)] = false;
                    _poolManager.Release(targetObject);
                    missedCount++;
                }
            }

            return missedCount;
        }

        public bool TickSpawning(float deltaTime, RuntimeDifficulty difficulty)
        {
            if (!_isRunning)
            {
                return false;
            }

            _spawnTimer -= deltaTime;
            int desiredCount = Mathf.Min(difficulty.MaxActiveObjects, _config.TotalUniqueTargetCombos);
            bool spawnedAny = false;

            while (_activeObjects.Count < desiredCount && _spawnTimer <= 0f)
            {
                SpawnOne(difficulty, useSpawnStacking: false);
                spawnedAny = true;
                _spawnTimer += difficulty.SpawnInterval;
            }

            return spawnedAny;
        }

        public void ReleaseActiveObject(TargetObject targetObject)
        {
            if (targetObject == null)
            {
                return;
            }

            for (int i = _activeObjects.Count - 1; i >= 0; i--)
            {
                if (_activeObjects[i] != targetObject)
                {
                    continue;
                }

                _activeObjects.RemoveAt(i);
                _usedTargetCombos[GetComboKey(targetObject.ActualColorIndex, targetObject.ShapeIndex)] = false;
                _poolManager.Release(targetObject);
                return;
            }
        }

        private void SpawnOne(RuntimeDifficulty difficulty, bool useSpawnStacking)
        {
            if (!TryGetUniqueTargetIdentity(out int actualColorIndex, out int shapeIndex))
            {
                return;
            }

            int labelColorIndex = ChooseLabelColor(actualColorIndex, difficulty.MisleadingChance);
            float spawnY = GetSpawnY(useSpawnStacking);
            Vector3 spawnPosition = FindSpawnPosition(spawnY);
            Vector3 visualEulerAngles = new Vector3(Random.Range(-6f, 6f), Random.Range(0f, 360f), Random.Range(-6f, 6f));

            TargetObject targetObject = _poolManager.GetTarget();
            targetObject.transform.SetParent(_gameplayRoot, true);
            targetObject.Configure(
                _config,
                _poolManager.MeshLibrary,
                _poolManager.MaterialLibrary,
                actualColorIndex,
                shapeIndex,
                labelColorIndex,
                spawnPosition,
                _config.SpawnSettings.TargetScale,
                visualEulerAngles,
                _labelRotation);

            _activeObjects.Add(targetObject);
            _usedTargetCombos[GetComboKey(actualColorIndex, shapeIndex)] = true;
        }

        private bool TryGetUniqueTargetIdentity(out int actualColorIndex, out int shapeIndex)
        {
            int freeCount = 0;
            bool prioritizeMissingShapes = _activeObjects.Count < _config.ShapeCount;
            bool[] activeShapeUsage = null;

            if (prioritizeMissingShapes)
            {
                activeShapeUsage = new bool[_config.ShapeCount];
                for (int i = 0; i < _activeObjects.Count; i++)
                {
                    activeShapeUsage[_activeObjects[i].ShapeIndex] = true;
                }
            }

            for (int colorIndex = 0; colorIndex < _config.ColorCount; colorIndex++)
            {
                for (int shapeIdx = 0; shapeIdx < _config.ShapeCount; shapeIdx++)
                {
                    if (!_usedTargetCombos[GetComboKey(colorIndex, shapeIdx)])
                    {
                        if (prioritizeMissingShapes && activeShapeUsage[shapeIdx])
                        {
                            continue;
                        }

                        freeCount++;
                    }
                }
            }

            if (freeCount == 0 && prioritizeMissingShapes)
            {
                prioritizeMissingShapes = false;
                for (int colorIndex = 0; colorIndex < _config.ColorCount; colorIndex++)
                {
                    for (int shapeIdx = 0; shapeIdx < _config.ShapeCount; shapeIdx++)
                    {
                        if (!_usedTargetCombos[GetComboKey(colorIndex, shapeIdx)])
                        {
                            freeCount++;
                        }
                    }
                }
            }

            if (freeCount == 0)
            {
                actualColorIndex = -1;
                shapeIndex = -1;
                return false;
            }

            int selection = Random.Range(0, freeCount);
            for (int colorIndex = 0; colorIndex < _config.ColorCount; colorIndex++)
            {
                for (int shapeIdx = 0; shapeIdx < _config.ShapeCount; shapeIdx++)
                {
                    if (_usedTargetCombos[GetComboKey(colorIndex, shapeIdx)])
                    {
                        continue;
                    }

                    if (prioritizeMissingShapes && activeShapeUsage[shapeIdx])
                    {
                        continue;
                    }

                    if (selection == 0)
                    {
                        actualColorIndex = colorIndex;
                        shapeIndex = shapeIdx;
                        return true;
                    }

                    selection--;
                }
            }

            actualColorIndex = -1;
            shapeIndex = -1;
            return false;
        }

        private int ChooseLabelColor(int actualColorIndex, float misleadingChance)
        {
            if (_config.ColorCount <= 1 || Random.value >= misleadingChance)
            {
                return actualColorIndex;
            }

            int labelColorIndex = actualColorIndex;
            while (labelColorIndex == actualColorIndex)
            {
                labelColorIndex = Random.Range(0, _config.ColorCount);
            }

            return labelColorIndex;
        }

        private float GetSpawnY(bool useSpawnStacking)
        {
            SpawnSettingsDefinition spawnSettings = _config.SpawnSettings;
            if (!useSpawnStacking)
            {
                return spawnSettings.SpawnY;
            }

            return spawnSettings.SpawnY + (_activeObjects.Count * spawnSettings.SpawnStackSpacingY);
        }

        private Vector3 FindSpawnPosition(float spawnY)
        {
            SpawnSettingsDefinition spawnSettings = _config.SpawnSettings;
            Vector3 bestPosition = new Vector3(0f, spawnY, 0f);

            for (int attempt = 0; attempt < spawnSettings.MaxSpawnAttempts; attempt++)
            {
                Vector3 candidate = new Vector3(
                    Random.Range(spawnSettings.HorizontalMin, spawnSettings.HorizontalMax),
                    spawnY,
                    Random.Range(spawnSettings.DepthMin, spawnSettings.DepthMax));

                bestPosition = candidate;

                if (IsPositionReadable(candidate, spawnSettings))
                {
                    return candidate;
                }
            }

            return bestPosition;
        }

        private bool IsPositionReadable(Vector3 candidate, SpawnSettingsDefinition spawnSettings)
        {
            for (int i = 0; i < _activeObjects.Count; i++)
            {
                Vector3 activePosition = _activeObjects[i].CachedTransform.position;
                bool tooCloseX = Mathf.Abs(activePosition.x - candidate.x) < spawnSettings.MinSpacingX;
                bool tooCloseZ = Mathf.Abs(activePosition.z - candidate.z) < spawnSettings.MinSpacingZ;
                bool tooCloseY = Mathf.Abs(activePosition.y - candidate.y) < (spawnSettings.SpawnStackSpacingY * 0.75f);

                if ((tooCloseX && tooCloseZ) || (tooCloseX && tooCloseY))
                {
                    return false;
                }
            }

            return true;
        }

        private int GetComboKey(int colorIndex, int shapeIndex)
        {
            return (colorIndex * _config.ShapeCount) + shapeIndex;
        }
    }
}
