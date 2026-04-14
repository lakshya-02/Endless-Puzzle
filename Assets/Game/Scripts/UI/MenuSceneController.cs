using EndlessPuzzle.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EndlessPuzzle.UI
{
    public sealed class MenuSceneController : MonoBehaviour
    {
        [SerializeField] private string _gameplaySceneName = RuntimeBootstrapper.GameplaySceneName;
        [SerializeField] private GameObject _menuRoot;
        [SerializeField] private CanvasGroup _menuCanvasGroup;
        [SerializeField] private Canvas _menuCanvas;
        [SerializeField] private GraphicRaycaster _menuRaycaster;
        [SerializeField] private float _startDelaySeconds = 1.25f;
        [SerializeField] private string _menuRootName = "MenuRoot";

        private bool _isStarting;

        public void StartGame()
        {
            if (_isStarting)
            {
                return;
            }

            _isStarting = true;
            StartCoroutine(BeginGameAfterDelay());
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private System.Collections.IEnumerator BeginGameAfterDelay()
        {
            HideMenu();

            if (_startDelaySeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(_startDelaySeconds);
            }

            RuntimeBootstrapper.RequestGameplayStart();

            if (SceneManager.GetActiveScene().name != _gameplaySceneName)
            {
                SceneManager.LoadScene(_gameplaySceneName, LoadSceneMode.Single);
                yield break;
            }
        }

        private void HideMenu()
        {
            Canvas canvas = ResolveMenuCanvas();
            if (canvas != null)
            {
                canvas.enabled = false;
            }

            GraphicRaycaster raycaster = ResolveMenuRaycaster();
            if (raycaster != null)
            {
                raycaster.enabled = false;
            }

            if (_menuCanvasGroup != null)
            {
                _menuCanvasGroup.alpha = 0f;
                _menuCanvasGroup.interactable = false;
                _menuCanvasGroup.blocksRaycasts = false;
            }
        }

        private GameObject ResolveMenuRoot()
        {
            if (_menuRoot != null)
            {
                return _menuRoot;
            }

            Transform namedRoot = FindNamedMenuRoot();
            if (namedRoot != null)
            {
                return namedRoot.gameObject;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                return canvas.gameObject;
            }

            Graphic graphic = GetComponentInParent<Graphic>();
            if (graphic != null && graphic.canvas != null)
            {
                return graphic.canvas.gameObject;
            }

            return gameObject;
        }

        private Transform FindNamedMenuRoot()
        {
            if (string.IsNullOrWhiteSpace(_menuRootName))
            {
                return null;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            GameObject[] roots = activeScene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform found = FindInChildrenRecursive(roots[i].transform, _menuRootName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindInChildrenRecursive(Transform current, string targetName)
        {
            if (current.name == targetName)
            {
                return current;
            }

            for (int i = 0; i < current.childCount; i++)
            {
                Transform found = FindInChildrenRecursive(current.GetChild(i), targetName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private Canvas ResolveMenuCanvas()
        {
            if (_menuCanvas != null)
            {
                return _menuCanvas;
            }

            GameObject root = ResolveMenuRoot();
            if (root != null && root.TryGetComponent(out Canvas rootCanvas))
            {
                return rootCanvas;
            }

            return GetComponentInParent<Canvas>();
        }

        private GraphicRaycaster ResolveMenuRaycaster()
        {
            if (_menuRaycaster != null)
            {
                return _menuRaycaster;
            }

            Canvas canvas = ResolveMenuCanvas();
            if (canvas != null)
            {
                return canvas.GetComponent<GraphicRaycaster>();
            }

            return null;
        }
    }
}
