using AbstractPixel.Core;
using UnityEditor;
using UnityEngine;

namespace AbstractPixel.Rumble.Editor
{
    /// <summary>
    /// RumblePattern inspector for designers: the normal fields, plus
    /// - Preview: plays the pattern on the connected controller (Play mode), so tuning needs no game event.
    /// - Bake from sound: turns a clip's loudness into the motor curves, with the same start, trim, delay and
    ///   fades the game plays it with, so the shake follows the sound.
    /// </summary>
    [CustomEditor(typeof(RumblePattern))]
    public class RumblePatternEditor : UnityEditor.Editor
    {
        private readonly RumbleAudioBakeSettings bakeSettings = new RumbleAudioBakeSettings();
        private bool isBakeOpen;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            DrawPreview();
            EditorGUILayout.Space();
            DrawBake();
        }

        private void DrawPreview()
        {
            EditorGUILayout.LabelField("Try it", EditorStyles.boldLabel);
            if (Application.isPlaying == false)
            {
                EditorGUILayout.HelpBox("Enter Play mode to feel this pattern on the controller.", MessageType.Info);
                return;
            }

            bool hasService = ServiceLocator.TryGet(out RumbleService rumble);
            if (hasService == false)
            {
                EditorGUILayout.HelpBox("No RumbleService in the scene yet.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Preview on controller"))
            {
                rumble.StopAllPatterns();
                rumble.Play((RumblePattern)target);
            }

            if (GUILayout.Button("Stop"))
            {
                rumble.StopAllPatterns();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Keep the Game view focused: rumble pauses while Unity is not the active window.", MessageType.None);
        }

        private void DrawBake()
        {
            isBakeOpen = EditorGUILayout.Foldout(isBakeOpen, "Bake from sound", true);
            if (isBakeOpen == false)
            {
                return;
            }

            EditorGUI.indentLevel++;
            bakeSettings.Clip = (AudioClip)EditorGUILayout.ObjectField("Sound", bakeSettings.Clip, typeof(AudioClip), false);
            EditorGUILayout.LabelField("Copy these from how the sound is played (e.g. its BroAudio entity):", EditorStyles.miniLabel);
            bakeSettings.StartPosition = EditorGUILayout.FloatField("Start position (s)", bakeSettings.StartPosition);
            bakeSettings.EndTrim = EditorGUILayout.FloatField("End trim (s)", bakeSettings.EndTrim);
            bakeSettings.Delay = EditorGUILayout.FloatField("Delay (s)", bakeSettings.Delay);
            bakeSettings.FadeIn = EditorGUILayout.FloatField("Fade in (s)", bakeSettings.FadeIn);
            bakeSettings.FadeOut = EditorGUILayout.FloatField("Fade out (s)", bakeSettings.FadeOut);
            bakeSettings.LightMotorShare = EditorGUILayout.Slider("Light motor share", bakeSettings.LightMotorShare, 0f, 1f);
            bakeSettings.Response = EditorGUILayout.Slider("Response (lower = quiet parts stronger)", bakeSettings.Response, 0.3f, 1f);

            bool hasClip = bakeSettings.Clip != null;
            EditorGUI.BeginDisabledGroup(hasClip == false);
            if (GUILayout.Button("Bake (replaces both motor curves and the duration)"))
            {
                RumbleAudioEnvelopeBaker.Bake(bakeSettings, (RumblePattern)target);
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
        }
    }
}
