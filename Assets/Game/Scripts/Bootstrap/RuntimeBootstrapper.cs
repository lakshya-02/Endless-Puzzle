using EndlessPuzzle.Data;
using EndlessPuzzle.Managers;
using EndlessPuzzle.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EndlessPuzzle.Bootstrap
{
    [DefaultExecutionOrder(-1000)]
    public sealed class RuntimeBootstrapper : MonoBehaviour
    {
        public const string GameplaySceneName = "SampleScene";

        private static RuntimeBootstrapper _instance;
        private static bool _startRequested;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateBootstrapper()
        {
            if (_instance != null)
            {
                return;
            }

            GameObject root = new GameObject("EndlessPuzzleRuntime");
            _instance = root.AddComponent<RuntimeBootstrapper>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.Portrait;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            TryBuildForCurrentScene();
        }

        public static void RequestGameplayStart()
        {
            _startRequested = true;

            if (_instance != null)
            {
                _instance.TryBuildForCurrentScene();
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            TryBuildForCurrentScene();
        }

        private void TryBuildForCurrentScene()
        {
            if (SceneManager.GetActiveScene().name != GameplaySceneName)
            {
                return;
            }

            if (ShouldWaitForMenuStart())
            {
                return;
            }

            if (FindAnyObjectByType<GameManager>() != null && transform.childCount > 0)
            {
                return;
            }

            GameConfigDatabase config = JsonDataLoader.Load();
            PrototypeSceneBuilder.Build(transform, config);
            _startRequested = false;
        }

        private static bool ShouldWaitForMenuStart()
        {
            MenuSceneController menuController = FindAnyObjectByType<MenuSceneController>();
            return menuController != null && !_startRequested;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;

            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
