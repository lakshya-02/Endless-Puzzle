using System;
using EndlessPuzzle.Data;
using UnityEngine;

namespace EndlessPuzzle.Managers
{
    public sealed class DifficultyManager : MonoBehaviour
    {
        public event Action<RuntimeDifficulty> DifficultyChanged;

        private GameConfigDatabase _config;
        private RuntimeDifficulty _currentDifficulty;

        public RuntimeDifficulty CurrentDifficulty => _currentDifficulty;

        public void Initialize(GameConfigDatabase config)
        {
            _config = config;
            ResetDifficulty();
        }

        public void ResetDifficulty()
        {
            UpdateDifficulty(0);
        }

        public void NotifyScoreChanged(int score)
        {
            UpdateDifficulty(score);
        }

        private void UpdateDifficulty(int score)
        {
            DifficultyTierDefinition[] tiers = _config.DifficultyTiers;
            int tierIndex = 0;

            for (int i = 1; i < tiers.Length; i++)
            {
                if (score >= tiers[i].ScoreThreshold)
                {
                    tierIndex = i;
                }
                else
                {
                    break;
                }
            }

            DifficultyTierDefinition tier = tiers[tierIndex];
            int maxTargets = Mathf.Min(tier.MaxActiveObjects, _config.SpawnSettings.AbsoluteMaxActiveObjects, _config.TotalUniqueTargetCombos);

            _currentDifficulty = new RuntimeDifficulty(
                tierIndex,
                maxTargets,
                tier.FallSpeed,
                tier.MisleadingChance,
                tier.SpawnInterval);

            DifficultyChanged?.Invoke(_currentDifficulty);
        }
    }
}
