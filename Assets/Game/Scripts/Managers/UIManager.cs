using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EndlessPuzzle.Managers
{
    public sealed class UIManager : MonoBehaviour
    {
        private TextMeshProUGUI _questionText;
        private TextMeshProUGUI _scoreText;
        private TextMeshProUGUI _bestText;
        private TextMeshProUGUI _timerText;
        private GameObject _gameOverPanel;
        private TextMeshProUGUI _gameOverReasonText;
        private TextMeshProUGUI _gameOverScoreText;
        private Button _restartButton;
        private int _lastTimerTenths = int.MinValue;

        public event Action RestartRequested;

        public void Initialize(
            TextMeshProUGUI questionText,
            TextMeshProUGUI scoreText,
            TextMeshProUGUI bestText,
            TextMeshProUGUI timerText,
            GameObject gameOverPanel,
            TextMeshProUGUI gameOverReasonText,
            TextMeshProUGUI gameOverScoreText,
            Button restartButton)
        {
            _questionText = questionText;
            _scoreText = scoreText;
            _bestText = bestText;
            _timerText = timerText;
            _gameOverPanel = gameOverPanel;
            _gameOverReasonText = gameOverReasonText;
            _gameOverScoreText = gameOverScoreText;
            _restartButton = restartButton;

            _restartButton.onClick.RemoveAllListeners();
            _restartButton.onClick.AddListener(() => RestartRequested?.Invoke());

            ShowGameOver(false, 0, 0, string.Empty);
            SetQuestionText(string.Empty);
            UpdateScores(0, 0);
            SetTimer(0f, true);
        }

        public void Bind(ScoreManager scoreManager, QuestionManager questionManager)
        {
            scoreManager.ScoresChanged += UpdateScores;
            questionManager.QuestionChanged += SetQuestionText;

            UpdateScores(scoreManager.CurrentScore, scoreManager.BestScore);
            SetQuestionText(questionManager.CurrentPrompt);
        }

        public void ShowGameOver(bool isVisible, int score, int bestScore, string reason)
        {
            _gameOverPanel.SetActive(isVisible);
            if (!isVisible)
            {
                return;
            }

            _gameOverReasonText.SetText(reason);
            _gameOverScoreText.SetText("Score: {0}\nBest: {1}", score, bestScore);
        }

        public void SetTimer(float remainingTimeSeconds, bool forceRefresh = false)
        {
            int tenths = Mathf.Max(0, Mathf.CeilToInt(remainingTimeSeconds * 10f));
            if (!forceRefresh && tenths == _lastTimerTenths)
            {
                return;
            }

            _lastTimerTenths = tenths;
            _timerText.SetText("Time: {0:0.0}", tenths * 0.1f);
        }

        private void UpdateScores(int currentScore, int bestScore)
        {
            _scoreText.SetText("Score: {0}", currentScore);
            _bestText.SetText("Best: {0}", bestScore);
        }

        private void SetQuestionText(string prompt)
        {
            _questionText.SetText(string.IsNullOrEmpty(prompt) ? " " : prompt);
        }
    }
}
