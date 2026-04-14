using EndlessPuzzle.Data;
using EndlessPuzzle.Gameplay;
using UnityEngine;

namespace EndlessPuzzle.Managers
{
    public sealed class GameManager : MonoBehaviour
    {
        private enum GameState
        {
            Booting = 0,
            Playing = 1,
            GameOver = 2
        }

        private GameConfigDatabase _config;
        private ScoreManager _scoreManager;
        private DifficultyManager _difficultyManager;
        private QuestionManager _questionManager;
        private SpawnManager _spawnManager;
        private UIManager _uiManager;
        private InputHandler _inputHandler;
        private BoundaryDetector _boundaryDetector;
        private float _remainingTimeSeconds;

        private GameState _state;

        public void Initialize(
            GameConfigDatabase config,
            ScoreManager scoreManager,
            DifficultyManager difficultyManager,
            QuestionManager questionManager,
            SpawnManager spawnManager,
            UIManager uiManager,
            InputHandler inputHandler,
            BoundaryDetector boundaryDetector)
        {
            _config = config;
            _scoreManager = scoreManager;
            _difficultyManager = difficultyManager;
            _questionManager = questionManager;
            _spawnManager = spawnManager;
            _uiManager = uiManager;
            _inputHandler = inputHandler;
            _boundaryDetector = boundaryDetector;

            _inputHandler.TargetTapped += HandleTargetTapped;
            _uiManager.RestartRequested += RestartGame;
            _uiManager.Bind(_scoreManager, _questionManager);
            StartGame();
        }

        private void Update()
        {
            if (_state != GameState.Playing)
            {
                return;
            }

            RuntimeDifficulty difficulty = _difficultyManager.CurrentDifficulty;
            TickTimer(Time.deltaTime);
            if (_state != GameState.Playing)
            {
                return;
            }

            int missedObjects = _spawnManager.TickMovement(Time.deltaTime, difficulty.FallSpeed, _boundaryDetector, _scoreManager.CurrentScore);
            if (missedObjects > 0)
            {
                HandleMissedObjects(missedObjects);
                if (_state != GameState.Playing)
                {
                    return;
                }
            }

            bool spawnedAny = _spawnManager.TickSpawning(Time.deltaTime, difficulty);
            if (missedObjects > 0)
            {
                _spawnManager.FillToTarget(_difficultyManager.CurrentDifficulty);
            }

            if (missedObjects > 0 || spawnedAny)
            {
                if (!RefreshQuestion(missedObjects > 0))
                {
                    HandleGameOver("The question system could not recover.");
                }
            }
        }

        private void StartGame()
        {
            _state = GameState.Booting;
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Time.timeScale = 1f;

            _scoreManager.Initialize();
            _scoreManager.ResetSession();
            _difficultyManager.ResetDifficulty();
            _spawnManager.ResetState();
            _spawnManager.BeginRun();
            _spawnManager.FillToTarget(_difficultyManager.CurrentDifficulty);
            _remainingTimeSeconds = _config.SpawnSettings.StartTimeSeconds;
            _uiManager.ShowGameOver(false, 0, 0, string.Empty);
            _uiManager.SetTimer(_remainingTimeSeconds, true);
            _inputHandler.SetInputEnabled(true);

            bool questionReady = RefreshQuestion(true);
            if (!questionReady)
            {
                HandleGameOver("No valid question could be generated.");
                return;
            }

            _state = GameState.Playing;
        }

        private void RestartGame()
        {
            _questionManager.ClearQuestion();
            StartGame();
        }

        private void HandleTargetTapped(TargetObject targetObject)
        {
            if (_state != GameState.Playing)
            {
                return;
            }

            if (_questionManager.IsCorrectAnswer(targetObject))
            {
                int tappedShapeIndex = targetObject.ShapeIndex;
                _spawnManager.ReleaseActiveObject(targetObject);
                _scoreManager.AddPoints(1);
                AddTime(_config.SpawnSettings.CorrectTimeRewardSeconds + _config.GetShapeDefinition(tappedShapeIndex).TimeBonusSeconds);
                _difficultyManager.NotifyScoreChanged(_scoreManager.CurrentScore);
                _spawnManager.FillToTarget(_difficultyManager.CurrentDifficulty);

                if (!RefreshQuestion(true))
                {
                    HandleGameOver("The question system could not recover.");
                }

                return;
            }

            _spawnManager.ReleaseActiveObject(targetObject);
            _scoreManager.ApplyPenalty(_config.SpawnSettings.WrongTapScorePenalty);
            _difficultyManager.NotifyScoreChanged(_scoreManager.CurrentScore);
            ApplyTimePenalty(_config.SpawnSettings.WrongTapTimePenaltySeconds);
            if (_state != GameState.Playing)
            {
                return;
            }

            _spawnManager.FillToTarget(_difficultyManager.CurrentDifficulty);
            if (!RefreshQuestion(true))
            {
                HandleGameOver("The question system could not recover.");
            }
        }

        private bool RefreshQuestion(bool forceRegenerate)
        {
            if (_spawnManager.ActiveCount == 0)
            {
                _spawnManager.FillToTarget(_difficultyManager.CurrentDifficulty);
            }

            if (_spawnManager.ActiveCount == 0)
            {
                return false;
            }

            return _questionManager.EnsureValidQuestion(_spawnManager.ActiveObjects, _scoreManager.CurrentScore, forceRegenerate);
        }

        private void HandleGameOver(string reason)
        {
            _state = GameState.GameOver;
            _spawnManager.StopRun();
            _inputHandler.SetInputEnabled(false);
            _uiManager.ShowGameOver(true, _scoreManager.CurrentScore, _scoreManager.BestScore, reason);
        }

        private void HandleMissedObjects(int missedObjects)
        {
            for (int i = 0; i < missedObjects; i++)
            {
                _scoreManager.ApplyPenalty(_config.SpawnSettings.MissScorePenalty);
                _difficultyManager.NotifyScoreChanged(_scoreManager.CurrentScore);
                ApplyTimePenalty(_config.SpawnSettings.MissTimePenaltySeconds);

                if (_state != GameState.Playing)
                {
                    return;
                }
            }
        }

        private void TickTimer(float deltaTime)
        {
            _remainingTimeSeconds -= deltaTime;
            if (_remainingTimeSeconds <= 0f)
            {
                _remainingTimeSeconds = 0f;
                _uiManager.SetTimer(_remainingTimeSeconds);
                HandleGameOver("Time ran out.");
                return;
            }

            _uiManager.SetTimer(_remainingTimeSeconds);
        }

        private void AddTime(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            _remainingTimeSeconds = Mathf.Min(_config.SpawnSettings.MaxTimeSeconds, _remainingTimeSeconds + amount);
            _uiManager.SetTimer(_remainingTimeSeconds, true);
        }

        private void ApplyTimePenalty(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            _remainingTimeSeconds = Mathf.Max(0f, _remainingTimeSeconds - amount);
            _uiManager.SetTimer(_remainingTimeSeconds, true);
            if (_remainingTimeSeconds <= 0f)
            {
                HandleGameOver("Time ran out.");
            }
        }

        private void OnDestroy()
        {
            if (_inputHandler != null)
            {
                _inputHandler.TargetTapped -= HandleTargetTapped;
            }

            if (_uiManager != null)
            {
                _uiManager.RestartRequested -= RestartGame;
            }
        }
    }
}
