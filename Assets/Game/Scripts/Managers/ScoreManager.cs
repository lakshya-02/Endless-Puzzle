using System;
using UnityEngine;

namespace EndlessPuzzle.Managers
{
    public sealed class ScoreManager : MonoBehaviour
    {
        private const string BestScoreKey = "EndlessPuzzle.BestScore";

        public event Action<int, int> ScoresChanged;

        public int CurrentScore { get; private set; }
        public int BestScore { get; private set; }

        public void Initialize()
        {
            BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
            CurrentScore = 0;
            ScoresChanged?.Invoke(CurrentScore, BestScore);
        }

        public void ResetSession()
        {
            CurrentScore = 0;
            ScoresChanged?.Invoke(CurrentScore, BestScore);
        }

        public void AddPoint()
        {
            AdjustScore(1);
        }

        public void AddPoints(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            AdjustScore(amount);
        }

        public void ApplyPenalty(int penalty)
        {
            if (penalty <= 0)
            {
                return;
            }

            AdjustScore(-penalty);
        }

        private void AdjustScore(int delta)
        {
            CurrentScore = Mathf.Max(0, CurrentScore + delta);

            if (CurrentScore > BestScore)
            {
                BestScore = CurrentScore;
                PlayerPrefs.SetInt(BestScoreKey, BestScore);
                PlayerPrefs.Save();
            }

            ScoresChanged?.Invoke(CurrentScore, BestScore);
        }
    }
}
