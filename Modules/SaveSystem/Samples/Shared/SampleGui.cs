using System;
using UnityEngine;

namespace AbstractPixel.SaveSystem.Samples
{
    /// <summary>
    /// The samples' shared on-screen look: large, calm panels that scale with the Game view, so every sample screen
    /// reads the same way. Sample-only; a real game draws its own UI. Call it only from OnGUI.
    /// Use: BeginPanel, then Value / Hint / Button / Row, then Footer, then EndPanel.
    /// </summary>
    public static class SampleGui
    {
        // Everything is laid out for a 1280x720 screen and scaled to the real one, like a Canvas Scaler.
        const float ReferenceHeight = 720f;
        const float Margin = 20f;

        static GUIStyle panelStyle;
        static GUIStyle titleStyle;
        static GUIStyle hintStyle;
        static GUIStyle labelStyle;
        static GUIStyle valueStyle;
        static GUIStyle sectionStyle;
        static GUIStyle buttonStyle;
        static GUIStyle codeStyle;
        static GUIStyle nameStyle;
        static Texture2D panelBackground;
        static Matrix4x4 matrixBeforePanel;

        /// <summary>Starts a panel at (x, y) on the 1280x720 reference screen, with a title and an optional one-line hint.</summary>
        public static void BeginPanel(float x, float y, float width, string title, string hint = null)
        {
            EnsureStyles();
            matrixBeforePanel = GUI.matrix;
            float scale = Screen.height / ReferenceHeight;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            float height = ReferenceHeight - y - Margin;
            GUILayout.BeginArea(new Rect(x, y, width, height));
            GUILayout.BeginVertical(panelStyle);
            GUILayout.Label(title, titleStyle);
            if (!string.IsNullOrEmpty(hint))
            {
                GUILayout.Label(hint, hintStyle);
            }
            GUILayout.Space(6f);
        }

        public static void EndPanel()
        {
            GUILayout.EndVertical();
            GUILayout.EndArea();
            GUI.matrix = matrixBeforePanel;
        }

        /// <summary>A small heading that splits a panel into parts, such as one part per save file.</summary>
        public static void Section(string heading)
        {
            GUILayout.Space(8f);
            GUILayout.Label(heading, sectionStyle);
        }

        /// <summary>One "name: value" line, with the value in bold so the saved number stands out.</summary>
        public static void Value(string name, string value)
        {
            GUILayout.BeginHorizontal();
            // The name is one line at its natural width, so it never wraps word by word next to its value.
            GUILayout.Label(name, nameStyle, GUILayout.ExpandWidth(false));
            GUILayout.Label(value, valueStyle);
            GUILayout.EndHorizontal();
        }

        public static void Label(string text)
        {
            GUILayout.Label(text, labelStyle);
        }

        public static void Hint(string text)
        {
            GUILayout.Label(text, hintStyle);
        }

        /// <summary>Read-only text such as JSON, in a smaller font so a whole save file fits.</summary>
        public static void Code(string text)
        {
            GUILayout.Label(text, codeStyle);
        }

        public static bool Button(string text)
        {
            return GUILayout.Button(text, buttonStyle);
        }

        public static bool SmallButton(string text, float width)
        {
            return GUILayout.Button(text, buttonStyle, GUILayout.Width(width));
        }

        /// <summary>Lays out the next buttons side by side until EndRow.</summary>
        public static void BeginRow()
        {
            GUILayout.BeginHorizontal();
        }

        public static void EndRow()
        {
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// The same Save and Reload scene buttons on every sample. afterSave runs right after saving, for a screen
        /// that shows the file it just wrote.
        /// </summary>
        public static void Footer(Action afterSave = null)
        {
            GUILayout.Space(12f);
            BeginRow();
            if (Button("Save"))
            {
                SaveActions.SaveAll();
                afterSave?.Invoke();
            }
            if (Button("Reload scene"))
            {
                SampleScenes.Reload();   // in your game: SceneManager.LoadScene(SceneManager.GetActiveScene().name)
            }
            EndRow();
        }

        // GUI.skin only exists inside OnGUI, so the styles are built on the first draw and kept.
        static void EnsureStyles()
        {
            if (panelStyle != null && panelBackground != null)
            {
                return;
            }

            panelBackground = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            panelBackground.SetPixel(0, 0, new Color(0.09f, 0.11f, 0.15f, 0.88f));
            panelBackground.Apply();

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(18, 18, 14, 16)
            };
            panelStyle.normal.background = panelBackground;

            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, wordWrap = true };
            titleStyle.normal.textColor = Color.white;

            hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
            hintStyle.normal.textColor = new Color(0.72f, 0.76f, 0.82f);

            sectionStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
            sectionStyle.normal.textColor = new Color(0.55f, 0.75f, 1f);

            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true };
            labelStyle.normal.textColor = new Color(0.86f, 0.88f, 0.92f);

            nameStyle = new GUIStyle(labelStyle) { wordWrap = false };

            valueStyle = new GUIStyle(labelStyle) { fontStyle = FontStyle.Bold };
            valueStyle.normal.textColor = Color.white;

            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20, fixedHeight = 44f, margin = new RectOffset(4, 4, 4, 4) };

            codeStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true, richText = false };
            codeStyle.normal.textColor = new Color(0.8f, 0.86f, 0.8f);
        }
    }
}
