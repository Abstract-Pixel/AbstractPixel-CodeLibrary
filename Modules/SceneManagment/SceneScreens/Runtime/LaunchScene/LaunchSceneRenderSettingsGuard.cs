using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace AbstractPixel.SceneManagement.SceneScreens
{
    /// <summary>
    /// Keeps a launch scene dark while the start scene group opens behind it. Start-up makes the start scene group's
    /// main scene active, and the active scene's RenderSettings (ambient, fog, reflections) apply to every scene, so they
    /// would light the launch scene. When the active scene changes it remembers the new scene's values, swaps in black
    /// ones, and puts them back at the hand-off (RestoreMenuSettings), so the start scene group looks as it would without
    /// the launch scene.
    /// </summary>
    public sealed class LaunchSceneRenderSettingsGuard
    {
        private struct Snapshot
        {
            public AmbientMode AmbientMode;
            public Color AmbientLight;
            public float AmbientIntensity;
            public SphericalHarmonicsL2 AmbientProbe;
            public float ReflectionIntensity;
            public bool Fog;
        }

        private Snapshot menuSettings;
        private bool hasMenuSettings;
        private bool isGuarding;

        public void BeginGuarding()
        {
            if (isGuarding)
            {
                return;
            }

            isGuarding = true;
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        }

        /// <summary>Puts the active scene's own settings back. Safe to call more than once.</summary>
        public void RestoreMenuSettings()
        {
            if (isGuarding == false)
            {
                return;
            }

            isGuarding = false;
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
            if (hasMenuSettings == false)
            {
                return;
            }

            Apply(menuSettings);
            hasMenuSettings = false;
        }

        private void HandleActiveSceneChanged(Scene _previous, Scene _next)
        {
            menuSettings = Capture();
            hasMenuSettings = true;
            Apply(BlackStageSettings());
        }

        private static Snapshot Capture()
        {
            Snapshot snapshot = new Snapshot();
            snapshot.AmbientMode = RenderSettings.ambientMode;
            snapshot.AmbientLight = RenderSettings.ambientLight;
            snapshot.AmbientIntensity = RenderSettings.ambientIntensity;
            snapshot.AmbientProbe = RenderSettings.ambientProbe;
            snapshot.ReflectionIntensity = RenderSettings.reflectionIntensity;
            snapshot.Fog = RenderSettings.fog;
            return snapshot;
        }

        private static Snapshot BlackStageSettings()
        {
            Snapshot black = new Snapshot();
            black.AmbientMode = AmbientMode.Flat;
            black.AmbientLight = Color.black;
            black.AmbientIntensity = 0f;
            black.AmbientProbe = new SphericalHarmonicsL2();
            black.ReflectionIntensity = 0f;
            black.Fog = false;
            return black;
        }

        private static void Apply(Snapshot _settings)
        {
            RenderSettings.ambientMode = _settings.AmbientMode;
            RenderSettings.ambientLight = _settings.AmbientLight;
            RenderSettings.ambientIntensity = _settings.AmbientIntensity;
            RenderSettings.ambientProbe = _settings.AmbientProbe;
            RenderSettings.reflectionIntensity = _settings.ReflectionIntensity;
            RenderSettings.fog = _settings.Fog;
        }
    }
}
