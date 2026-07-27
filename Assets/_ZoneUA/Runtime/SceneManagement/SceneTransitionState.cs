using System;

namespace ZoneUA.SceneManagement
{
    public enum SceneTransitionPhase
    {
        Idle,
        Loading,
        Activating,
        Unloading,
        Completed,
        Failed
    }

    public sealed class SceneTransitionState
    {
        public SceneTransitionPhase Phase { get; private set; } = SceneTransitionPhase.Idle;
        public string CurrentScene { get; private set; } = string.Empty;
        public string TargetScene { get; private set; } = string.Empty;
        public float Progress { get; private set; }
        public string Error { get; private set; } = string.Empty;
        public bool IsBusy => Phase is SceneTransitionPhase.Loading or SceneTransitionPhase.Activating or SceneTransitionPhase.Unloading;

        public bool TryBegin(string currentScene, string targetScene)
        {
            if (IsBusy || string.IsNullOrWhiteSpace(targetScene)) return false;
            CurrentScene = currentScene ?? string.Empty;
            TargetScene = targetScene.Trim();
            Progress = 0f;
            Error = string.Empty;
            Phase = SceneTransitionPhase.Loading;
            return true;
        }

        public void SetProgress(float progress)
        {
            Progress = Math.Clamp(progress, 0f, 1f);
        }

        public void BeginActivation() => Phase = SceneTransitionPhase.Activating;
        public void BeginUnload() => Phase = SceneTransitionPhase.Unloading;

        public void Complete()
        {
            CurrentScene = TargetScene;
            TargetScene = string.Empty;
            Progress = 1f;
            Error = string.Empty;
            Phase = SceneTransitionPhase.Completed;
        }

        public void Fail(string error)
        {
            Error = error ?? "Unknown scene transition failure.";
            TargetScene = string.Empty;
            Phase = SceneTransitionPhase.Failed;
        }

        public void Reset()
        {
            TargetScene = string.Empty;
            Progress = 0f;
            Error = string.Empty;
            Phase = SceneTransitionPhase.Idle;
        }
    }
}
