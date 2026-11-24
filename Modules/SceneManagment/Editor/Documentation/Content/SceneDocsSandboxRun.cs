using System.Collections.Generic;
using AbstractPixel.Core;
using AbstractPixel.SceneManagement.Editor;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>The kind of scene change the sandbox runs.</summary>
    public enum SandboxChange { FromAnotherGroup, Retry, PreloadedGroup }

    /// <summary>What the sandbox screen shows.</summary>
    public enum SandboxScreen { OldLevel, LoadingScene, NewLevel }

    /// <summary>The sandbox's own settings: a copy of the project's, changed only in the docs page.</summary>
    public class SceneDocsSandboxModel
    {
        public const string GroupName = "Your scene group";
        public List<string> LoadingScenes = new List<string>();
        public string DefaultLoadingScene = "";
        public float MinimumSeconds = 1f;
        public ShowLoadingScene Show = ShowLoadingScene.WhenArrivingFromAnotherSceneGroup;
        public string GroupPick = "";
        public SandboxChange Change = SandboxChange.FromAnotherGroup;
        public float LoadSeconds = 0.6f;
        public bool HasScreenEffect = true;

        /// <summary>A copy of the project's loading scene settings. Reads only; the project is never changed.</summary>
        public static SceneDocsSandboxModel FromProject()
        {
            SceneDocsSandboxModel model = new SceneDocsSandboxModel();
            SceneSystemSettings settings = SceneSystemSettingsAssets.Find();
            if (settings != null)
            {
                foreach (SceneReference scene in settings.LoadingScenes)
                {
                    if (scene != null && !string.IsNullOrEmpty(scene.SceneName) && !model.LoadingScenes.Contains(scene.SceneName))
                    {
                        model.LoadingScenes.Add(scene.SceneName);
                    }
                }
                model.DefaultLoadingScene = settings.DefaultLoadingScene == null ? "" : settings.DefaultLoadingScene.SceneName ?? "";
                model.MinimumSeconds = settings.LoadingSceneMinimumSeconds;
            }
            if (model.LoadingScenes.Count == 0)
            {
                model.LoadingScenes.Add("Loading Scene");
                model.DefaultLoadingScene = "Loading Scene";
            }
            return model;
        }
    }

    /// <summary>One moment of a sandbox run: what happens, why, and what the screen shows.</summary>
    public class SandboxEvent
    {
        public float Time;
        public string What;
        public string Why;
        public bool IsWarning;
        public bool Prints = true;
        public SandboxScreen? Screen;
        public bool? Covered;
        public float Progress = -1f;
    }

    /// <summary>
    /// Builds a sandbox run. Whether the change shows a loading scene, and which, comes from the scene system's own
    /// LoadingSceneRoute.ChooseFor, run on throwaway copies of a scene group and the settings. The timing follows the
    /// order of a change (design decision 3), with a fade of FadeSeconds when there is a screen effect.
    /// </summary>
    public static class SceneDocsSandboxRun
    {
        public const float FadeSeconds = 0.3f;
        private const int NewScenes = 3;

        public static List<SandboxEvent> Build(SceneDocsSandboxModel _model, out string _verdict)
        {
            List<SandboxEvent> events = new List<SandboxEvent>();
            string reason;
            bool wouldWarn;
            string scene = Choose(_model, out reason, out wouldWarn);
            _verdict = scene == null ? "No loading scene. " + reason : "Shows " + scene + ". " + reason;
            if (wouldWarn)
            {
                events.Add(new SandboxEvent { Time = 0f, IsWarning = true, What = "Warning in the Console", Why = "'" + SceneDocsSandboxModel.GroupName + "' shows a loading scene, but it picked none and there is no default. The change goes on without one." });
            }

            bool effect = _model.HasScreenEffect;
            float fade = effect ? FadeSeconds : 0f;
            float time = 0f;
            if (effect)
            {
                events.Add(new SandboxEvent { Time = time, What = "Hide screen starts", Why = "Covers the old level (a " + FadeSeconds + " s fade).", Covered = true });
            }
            else
            {
                events.Add(new SandboxEvent { Time = time, What = "No screen effect", Why = "The same steps make an instant cut." });
            }
            time += fade;

            float shownAt = 0f;
            if (scene != null)
            {
                events.Add(new SandboxEvent { Time = time, What = "The loading scene opens: " + scene, Why = "Before the old level closes, so a frame never has no camera.", Screen = SandboxScreen.LoadingScene, Progress = 0f });
                time += 0.1f;
                shownAt = time;
                events.Add(new SandboxEvent { Time = time, What = effect ? "Show screen starts" : "The loading scene shows", Why = "The minimum time counts from here.", Covered = false });
                time += fade;
            }

            string behind = scene != null ? "Behind the loading scene." : effect ? "Behind the cover." : "The old level is still on screen.";
            events.Add(new SandboxEvent { Time = time, What = "The old scenes close", Why = behind });
            time += 0.1f;
            events.Add(new SandboxEvent { Time = time, What = "Unused memory is freed", Why = behind });
            time += 0.1f;
            events.Add(new SandboxEvent { Time = time, What = "The new scenes open while the progress fills", Why = "Each of the " + NewScenes + " scenes has an equal share of the progress." });
            for (int opened = 1; opened <= NewScenes; opened++)
            {
                float at = time + _model.LoadSeconds * opened / NewScenes;
                float progress = (float)opened / NewScenes;
                string why = opened == NewScenes ? "1 only once every scene is open." : "Never goes back.";
                events.Add(new SandboxEvent { Time = at, What = "Progress " + progress.ToString("0.00") + " (" + opened + " of " + NewScenes + " open)", Why = why, Progress = progress });
            }
            time += _model.LoadSeconds;

            if (scene != null)
            {
                float minimumEnd = shownAt + _model.MinimumSeconds;
                if (minimumEnd > time)
                {
                    events.Add(new SandboxEvent { Time = time, What = "Waits " + (minimumEnd - time).ToString("0.00") + " s for the minimum time", Why = "Minimum time is " + _model.MinimumSeconds.ToString("0.##") + " s from Show screen; the load was faster." });
                    time = minimumEnd;
                }
                else
                {
                    events.Add(new SandboxEvent { Time = time, What = "No wait", Why = (time - shownAt).ToString("0.00") + " s have passed since Show screen, more than the minimum time (" + _model.MinimumSeconds.ToString("0.##") + " s)." });
                }
                events.Add(new SandboxEvent { Time = time, What = "The loading progress view plays its \"full\" animation", Why = "The scene system waits for it before Hide screen." });
                time += 0.3f;
                if (effect)
                {
                    events.Add(new SandboxEvent { Time = time, What = "Hide screen starts", Why = "Covers the loading scene.", Covered = true });
                    time += fade;
                }
                events.Add(new SandboxEvent { Time = time, What = "The loading scene closes", Why = effect ? "Behind the cover." : "Cut to the new level.", Screen = SandboxScreen.NewLevel });
                time += 0.05f;
            }
            else
            {
                events.Add(new SandboxEvent { Time = time, Prints = false, Screen = SandboxScreen.NewLevel });
            }

            if (effect)
            {
                events.Add(new SandboxEvent { Time = time, What = "Show screen on the new level", Why = "Uncovers the new level.", Covered = false });
                time += fade;
            }
            events.Add(new SandboxEvent { Time = time, What = "Done in " + time.ToString("0.00") + " s", Why = _verdict });
            return events;
        }

        // Asks the scene system's own code, on throwaway copies, whether and which loading scene this change shows.
        private static string Choose(SceneDocsSandboxModel _model, out string _reason, out bool _wouldWarn)
        {
            _wouldWarn = false;
            bool isRetry = _model.Change == SandboxChange.Retry;
            bool usesPreload = _model.Change == SandboxChange.PreloadedGroup;
            SceneGroup group = ScriptableObject.CreateInstance<SceneGroup>();
            SceneSystemSettings settings = ScriptableObject.CreateInstance<SceneSystemSettings>();
            group.hideFlags = HideFlags.HideAndDontSave;
            settings.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                group.name = SceneDocsSandboxModel.GroupName;
                group.ShowLoadingScene = _model.Show;
                foreach (string name in _model.LoadingScenes)
                {
                    settings.LoadingScenes.Add(new SceneReference(name));
                }
                settings.DefaultLoadingScene = new SceneReference(_model.DefaultLoadingScene ?? "");
                LoadingSceneRoute route = new LoadingSceneRoute();
                route.FindSettings = () => settings;

                // A placeholder pick asks only "does this change show one?", so the real call never logs to the Console.
                group.LoadingScene = new SceneReference("?");
                bool shows = route.ChooseFor(group, isRetry, usesPreload) != null;
                if (!shows)
                {
                    _reason = usesPreload ? "A switch to a preloaded group never shows one: its scenes are already open."
                        : _model.Show == ShowLoadingScene.Never ? "Show the loading scene is Never."
                        : "A Retry with \"When arriving from another scene group\" goes straight back in without it.";
                    return null;
                }
                bool hasPick = !string.IsNullOrEmpty(_model.GroupPick);
                if (!hasPick && string.IsNullOrEmpty(_model.DefaultLoadingScene))
                {
                    _wouldWarn = true;
                    _reason = "The group picked none and there is no default loading scene.";
                    return null;
                }
                group.LoadingScene = new SceneReference(hasPick ? _model.GroupPick : "");
                SceneReference chosen = route.ChooseFor(group, isRetry, usesPreload);
                _reason = hasPick ? "The group picked it." : "The group picked none, so it uses the default.";
                return chosen == null ? null : chosen.SceneName;
            }
            finally
            {
                Object.DestroyImmediate(group);
                Object.DestroyImmediate(settings);
            }
        }
    }
}
