using EndlessPuzzle.Data;
using EndlessPuzzle.Managers;
using EndlessPuzzle.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

#if ENDLESS_PUZZLE_USE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace EndlessPuzzle.Bootstrap
{
    public static class PrototypeSceneBuilder
    {
        public static void Build(Transform runtimeRoot, GameConfigDatabase config)
        {
            QualitySettings.SetQualityLevel(0, true);

            Camera worldCamera = EnsureCamera();
            EnsureDirectionalLight();
            DisableSceneVolumes();
            EnsureEventSystem();
            ConfigureRenderEnvironment();

            PrototypePresentationSettings presentationSettings = ResolvePresentationSettings();
            TMP_FontAsset fontAsset = ResolveFontAsset(presentationSettings);

            Transform gameplayRoot = CreateChild(runtimeRoot, "GameplayRoot");
            Transform activeTargetsRoot = CreateChild(gameplayRoot, "ActiveTargets");
            Transform spawnArea = CreateChild(gameplayRoot, "SpawnArea");
            spawnArea.position = new Vector3(0f, config.SpawnSettings.SpawnY, 0f);

            Transform managersRoot = CreateChild(runtimeRoot, "Managers");
            Transform uiRoot = CreateChild(runtimeRoot, "UIRoot");

            ScoreManager scoreManager = CreateManager<ScoreManager>(managersRoot, "ScoreManager");
            DifficultyManager difficultyManager = CreateManager<DifficultyManager>(managersRoot, "DifficultyManager");
            QuestionManager questionManager = CreateManager<QuestionManager>(managersRoot, "QuestionManager");
            PoolManager poolManager = CreateManager<PoolManager>(managersRoot, "PoolManager");
            SpawnManager spawnManager = CreateManager<SpawnManager>(managersRoot, "SpawnManager");
            InputHandler inputHandler = CreateManager<InputHandler>(managersRoot, "InputHandler");
            BoundaryDetector boundaryDetector = CreateManager<BoundaryDetector>(managersRoot, "BoundaryDetector");
            UIManager uiManager = CreateManager<UIManager>(managersRoot, "UIManager");
            GameManager gameManager = CreateManager<GameManager>(managersRoot, "GameManager");

            poolManager.Initialize(config, presentationSettings);
            difficultyManager.Initialize(config);
            questionManager.Initialize(config);
            spawnManager.Initialize(config, poolManager, activeTargetsRoot, worldCamera);
            inputHandler.Initialize(worldCamera);
            boundaryDetector.Initialize(config.SpawnSettings.FailY);

            UiBuildResult ui = BuildUi(uiRoot, fontAsset);
            uiManager.Initialize(
                ui.QuestionText,
                ui.ScoreText,
                ui.BestText,
                ui.TimerText,
                ui.GameOverPanel,
                ui.GameOverReasonText,
                ui.GameOverScoreText,
                ui.RestartButton);

            gameManager.Initialize(config, scoreManager, difficultyManager, questionManager, spawnManager, uiManager, inputHandler, boundaryDetector);
        }

        private static Camera EnsureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = Object.FindAnyObjectByType<Camera>();
            }

            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            camera.transform.position = new Vector3(0f, 0.35f, -14.5f);
            camera.transform.rotation = Quaternion.identity;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.09f, 0.14f, 1f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 50f;
            camera.fieldOfView = 65f;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.useOcclusionCulling = false;

            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = false;
            cameraData.antialiasing = AntialiasingMode.None;
            cameraData.requiresDepthOption = CameraOverrideOption.Off;
            cameraData.requiresColorOption = CameraOverrideOption.Off;
            cameraData.renderShadows = false;

            return camera;
        }

        private static void EnsureDirectionalLight()
        {
            Light light = Object.FindAnyObjectByType<Light>();
            if (light == null)
            {
                GameObject lightObject = new GameObject("Directional Light");
                light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
            }

            light.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.98f, 0.95f, 1f);
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.Auto;
        }

        private static void DisableSceneVolumes()
        {
            Volume[] volumes = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
            for (int i = 0; i < volumes.Length; i++)
            {
                volumes[i].enabled = false;
            }
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem");
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }

#if ENDLESS_PUZZLE_USE_INPUT_SYSTEM
            StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (oldModule != null)
            {
                Object.Destroy(oldModule);
            }

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#else
            if (eventSystem.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }

        private static void ConfigureRenderEnvironment()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.62f, 0.67f, 0.74f, 1f);
        }

        private static PrototypePresentationSettings ResolvePresentationSettings()
        {
            return Resources.Load<PrototypePresentationSettings>("Presentation/PrototypePresentationSettings");
        }

        private static TMP_FontAsset ResolveFontAsset(PrototypePresentationSettings presentationSettings)
        {
            TMP_FontAsset fontAsset = presentationSettings != null ? presentationSettings.FontAsset : null;
            if (fontAsset == null)
            {
                fontAsset = TMP_Settings.defaultFontAsset;
            }

            if (fontAsset == null)
            {
                fontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }

            if (fontAsset == null)
            {
                Debug.LogError("TextMeshPro default font asset is missing. Import TMP Essential Resources from Window > TextMeshPro > Import TMP Essential Resources.");
            }
            else if (!IsFontAssetUsable(fontAsset))
            {
                Debug.LogWarning($"TMP font asset '{fontAsset.name}' is not fully usable yet. Falling back to component defaults where possible. Re-import TMP Essential Resources if text fails to render.");
                return null;
            }

            return fontAsset;
        }

        private static UiBuildResult BuildUi(Transform uiRoot, TMP_FontAsset fontAsset)
        {
            GameObject canvasObject = new GameObject("RuntimeCanvas");
            canvasObject.transform.SetParent(uiRoot, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvasObject.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.65f;

            RectTransform safeArea = CreateRect("SafeArea", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            safeArea.gameObject.AddComponent<SafeAreaFitter>();

            RectTransform hudRoot = CreateRect("HudRoot", safeArea, Vector2.zero, Vector2.one, new Vector2(24f, 22f), new Vector2(-24f, -22f), new Vector2(0.5f, 0.5f));
            RectTransform topPanel = CreatePanel("TopPanel", hudRoot, new Color(0.08f, 0.1f, 0.15f, 0.9f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -104f), Vector2.zero, new Vector2(0.5f, 1f));
            RectTransform questionCard = CreatePanel("QuestionCard", hudRoot, new Color(0.1f, 0.13f, 0.19f, 0.94f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -196f), new Vector2(0f, -116f), new Vector2(0.5f, 1f));

            TextMeshProUGUI scoreText = CreateText(
                "ScoreText",
                topPanel,
                fontAsset,
                30f,
                FontStyles.Bold,
                TextAlignmentOptions.Left,
                new Vector2(0f, 0f),
                new Vector2(0.33f, 1f),
                new Vector2(26f, 0f),
                new Vector2(-8f, 0f),
                new Color(0.96f, 0.97f, 1f, 1f));

            TextMeshProUGUI bestText = CreateText(
                "BestText",
                topPanel,
                fontAsset,
                30f,
                FontStyles.Bold,
                TextAlignmentOptions.Right,
                new Vector2(0.67f, 0f),
                new Vector2(1f, 1f),
                new Vector2(8f, 0f),
                new Vector2(-26f, 0f),
                new Color(0.96f, 0.97f, 1f, 1f));

            TextMeshProUGUI timerText = CreateText(
                "TimerText",
                topPanel,
                fontAsset,
                34f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0.33f, 0f),
                new Vector2(0.67f, 1f),
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Color(1f, 0.94f, 0.79f, 1f));

            TextMeshProUGUI questionText = CreateText(
                "QuestionText",
                questionCard,
                fontAsset,
                42f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(28f, 8f),
                new Vector2(-28f, -8f),
                new Color(0.98f, 0.98f, 1f, 1f));
            questionText.enableWordWrapping = true;

            RectTransform gameOverPanel = CreatePanel(
                "GameOverPanel",
                hudRoot,
                new Color(0.06f, 0.07f, 0.11f, 0.96f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-300f, -240f),
                new Vector2(300f, 240f),
                new Vector2(0.5f, 0.5f));

            TextMeshProUGUI titleText = CreateText(
                "TitleText",
                gameOverPanel,
                fontAsset,
                56f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(20f, -96f),
                new Vector2(-20f, -20f),
                Color.white);
            titleText.SetText("Game Over");

            TextMeshProUGUI reasonText = CreateText(
                "ReasonText",
                gameOverPanel,
                fontAsset,
                32f,
                FontStyles.Normal,
                TextAlignmentOptions.Center,
                new Vector2(0f, 0.50f),
                new Vector2(1f, 0.72f),
                new Vector2(28f, 0f),
                new Vector2(-28f, 0f),
                new Color(0.91f, 0.93f, 0.97f, 1f));

            TextMeshProUGUI scoreSummaryText = CreateText(
                "ScoreSummaryText",
                gameOverPanel,
                fontAsset,
                36f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0f, 0.28f),
                new Vector2(1f, 0.46f),
                new Vector2(28f, 0f),
                new Vector2(-28f, 0f),
                Color.white);

            Button restartButton = CreateButton(
                "RestartButton",
                gameOverPanel,
                fontAsset,
                "Restart",
                new Vector2(0.5f, 0.15f),
                new Vector2(0.5f, 0.15f),
                new Vector2(-180f, -54f),
                new Vector2(180f, 54f),
                new Color(0.2f, 0.54f, 0.34f, 1f));

            return new UiBuildResult(
                questionText,
                scoreText,
                bestText,
                timerText,
                gameOverPanel.gameObject,
                reasonText,
                scoreSummaryText,
                restartButton);
        }

        private static Transform CreateChild(Transform parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static T CreateManager<T>(Transform parent, string name) where T : Component
        {
            GameObject managerObject = new GameObject(name);
            managerObject.transform.SetParent(parent, false);
            return managerObject.AddComponent<T>();
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Vector2 pivot)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.pivot = pivot;
            return rect;
        }

        private static RectTransform CreatePanel(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Vector2 pivot)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax, pivot);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private static TextMeshProUGUI CreateText(
            string name,
            Transform parent,
            TMP_FontAsset fontAsset,
            float fontSize,
            FontStyles fontStyle,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax,
            Color color)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax, new Vector2(0.5f, 0.5f));
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            TryAssignFont(text, fontAsset);
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.alignment = alignment;
            text.color = color;
            text.enableWordWrapping = false;
            text.raycastTarget = false;
            text.text = string.Empty;
            return text;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            TMP_FontAsset fontAsset,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax,
            Color backgroundColor)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax, new Vector2(0.5f, 0.5f));
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = backgroundColor;

            Button button = rect.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = backgroundColor * 1.08f;
            colors.pressedColor = backgroundColor * 0.9f;
            colors.selectedColor = backgroundColor;
            button.colors = colors;

            TextMeshProUGUI labelText = CreateText(
                "Label",
                rect,
                fontAsset,
                46f,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                Vector2.zero,
                Vector2.one,
                new Vector2(12f, 12f),
                new Vector2(-12f, -12f),
                Color.white);
            labelText.SetText(label);

            return button;
        }

        private static bool IsFontAssetUsable(TMP_FontAsset fontAsset)
        {
            try
            {
                return fontAsset != null && fontAsset.material != null;
            }
            catch
            {
                return false;
            }
        }

        private static void TryAssignFont(TMP_Text textComponent, TMP_FontAsset fontAsset)
        {
            if (textComponent == null || fontAsset == null)
            {
                return;
            }

            try
            {
                textComponent.font = fontAsset;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Failed to assign TMP font '{fontAsset.name}' to '{textComponent.name}'. Falling back to TMP defaults. {exception.Message}");
            }
        }

        private readonly struct UiBuildResult
        {
            public UiBuildResult(
                TextMeshProUGUI questionText,
                TextMeshProUGUI scoreText,
                TextMeshProUGUI bestText,
                TextMeshProUGUI timerText,
                GameObject gameOverPanel,
                TextMeshProUGUI gameOverReasonText,
                TextMeshProUGUI gameOverScoreText,
                Button restartButton)
            {
                QuestionText = questionText;
                ScoreText = scoreText;
                BestText = bestText;
                TimerText = timerText;
                GameOverPanel = gameOverPanel;
                GameOverReasonText = gameOverReasonText;
                GameOverScoreText = gameOverScoreText;
                RestartButton = restartButton;
            }

            public TextMeshProUGUI QuestionText { get; }
            public TextMeshProUGUI ScoreText { get; }
            public TextMeshProUGUI BestText { get; }
            public TextMeshProUGUI TimerText { get; }
            public GameObject GameOverPanel { get; }
            public TextMeshProUGUI GameOverReasonText { get; }
            public TextMeshProUGUI GameOverScoreText { get; }
            public Button RestartButton { get; }
        }
    }
}
