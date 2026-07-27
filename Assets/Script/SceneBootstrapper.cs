using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZoneUA.SceneManagement;

[DefaultExecutionOrder(-2000)]
[DisallowMultipleComponent]
public sealed class SceneBootstrapper : MonoBehaviour
{
    private static SceneBootstrapper instance;

    [Header("Scene Configuration")]
    [SerializeField] private SceneCatalog catalog;
    [SerializeField] private bool loadInitialSceneOnStart = true;
    [SerializeField] private bool unloadPreviousGameplayScene = true;

    private readonly SceneTransitionState transitionState = new SceneTransitionState();
    private Coroutine transitionRoutine;
    private string activeGameplayScene = string.Empty;

    public static SceneBootstrapper Instance => instance;
    public SceneCatalog Catalog => catalog;
    public SceneTransitionState TransitionState => transitionState;
    public string ActiveGameplayScene => activeGameplayScene;
    public bool IsTransitioning => transitionRoutine != null;

    public event Action<SceneTransitionState> TransitionChanged;
    public event Action<string> SceneActivated;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (loadInitialSceneOnStart && catalog != null && !string.IsNullOrWhiteSpace(catalog.InitialProductionScene))
        {
            LoadScene(catalog.InitialProductionScene);
        }
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public bool LoadInitialScene() => catalog != null && LoadScene(catalog.InitialProductionScene);

    public bool LoadScene(string sceneName)
    {
        if (transitionRoutine != null || string.IsNullOrWhiteSpace(sceneName)) return false;
        string target = sceneName.Trim();
        string current = activeGameplayScene;
        if (!transitionState.TryBegin(current, target)) return false;

        transitionRoutine = StartCoroutine(TransitionRoutine(current, target));
        RaiseTransitionChanged();
        return true;
    }

    public void CancelTransition()
    {
        if (transitionRoutine == null) return;
        StopCoroutine(transitionRoutine);
        transitionRoutine = null;
        transitionState.Fail("Scene transition was cancelled.");
        RaiseTransitionChanged();
    }

    private IEnumerator TransitionRoutine(string previousScene, string targetScene)
    {
        if (!Application.CanStreamedLevelBeLoaded(targetScene))
        {
            FailTransition($"Scene '{targetScene}' is not present in Build Settings.");
            yield break;
        }

        Scene alreadyLoaded = SceneManager.GetSceneByName(targetScene);
        if (!alreadyLoaded.isLoaded)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
            if (load == null)
            {
                FailTransition($"Unity could not start loading scene '{targetScene}'.");
                yield break;
            }

            load.allowSceneActivation = false;
            while (load.progress < 0.9f)
            {
                transitionState.SetProgress(load.progress / 0.9f);
                RaiseTransitionChanged();
                yield return null;
            }

            transitionState.BeginActivation();
            transitionState.SetProgress(1f);
            RaiseTransitionChanged();
            load.allowSceneActivation = true;
            while (!load.isDone) yield return null;
        }

        Scene target = SceneManager.GetSceneByName(targetScene);
        if (!target.IsValid() || !target.isLoaded)
        {
            FailTransition($"Scene '{targetScene}' did not become loaded.");
            yield break;
        }

        SceneManager.SetActiveScene(target);
        activeGameplayScene = targetScene;
        SceneActivated?.Invoke(targetScene);

        if (unloadPreviousGameplayScene &&
            !string.IsNullOrWhiteSpace(previousScene) &&
            !string.Equals(previousScene, targetScene, StringComparison.OrdinalIgnoreCase))
        {
            Scene previous = SceneManager.GetSceneByName(previousScene);
            if (previous.IsValid() && previous.isLoaded)
            {
                transitionState.BeginUnload();
                RaiseTransitionChanged();
                AsyncOperation unload = SceneManager.UnloadSceneAsync(previous);
                if (unload != null)
                {
                    while (!unload.isDone) yield return null;
                }
            }
        }

        transitionState.Complete();
        transitionRoutine = null;
        RaiseTransitionChanged();
    }

    private void FailTransition(string message)
    {
        transitionState.Fail(message);
        transitionRoutine = null;
        Debug.LogError(message, this);
        RaiseTransitionChanged();
    }

    private void RaiseTransitionChanged() => TransitionChanged?.Invoke(transitionState);
}
