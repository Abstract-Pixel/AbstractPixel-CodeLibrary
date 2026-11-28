using AbstractPixel.Core.Editor.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AbstractPixel.SceneManagement.Editor
{
    /// <summary>
    /// Draws spawn point previews in the Scene view: see-through meshes in the label's colour (bright for a set
    /// instance, dimmer for a predicted match, faded for a position no relocatable reaches), tags such as
    /// "Crate · Instance 5", the link from a selected relocatable to its position, and the Shift+click ghost.
    /// </summary>
    internal static class SpawnPointPreviewDrawing
    {
        public const float SetAlpha = 0.65f;
        public const float PredictedAlpha = 0.42f;
        public const float UnreachedAlpha = 0.16f;
        private const float TagDistance = 40f;
        private static readonly Vector2 TagSpacing = new Vector2(130f, 22f);
        private static readonly System.Collections.Generic.List<Vector2> tagsDrawn = new System.Collections.Generic.List<Vector2>();

        private static Material material;
        private static GUIStyle tagStyle;

        public static readonly Color Allowed = EditorUI.Green;
        public static readonly Color TooSteep = EditorUI.Red;
        public static readonly Color Warning = EditorUI.Yellow;

        public static void DrawShape(PreviewShape _shape, Pose _pose, Color _colour)
        {
            Material previewMaterial = Material();
            Matrix4x4 root = Matrix4x4.TRS(_pose.position, _pose.rotation, Vector3.one);
            previewMaterial.SetColor("_Color", _colour);
            previewMaterial.SetPass(0);
            for (int index = 0; index < _shape.Meshes.Count; index++)
            {
                Mesh mesh = _shape.Meshes[index];
                Matrix4x4 matrix = root * _shape.Matrices[index];
                for (int subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
                {
                    Graphics.DrawMeshNow(mesh, matrix, subMesh);
                }
            }
        }

        /// <summary>A thin outline around the preview, for the selected and the hovered position.</summary>
        public static void DrawOutline(PreviewShape _shape, Pose _pose, Color _colour)
        {
            Matrix4x4 previous = Handles.matrix;
            Handles.matrix = Matrix4x4.TRS(_pose.position, _pose.rotation, Vector3.one);
            Handles.color = _colour;
            Handles.DrawWireCube(_shape.LocalBounds.center, _shape.LocalBounds.size);
            Handles.matrix = previous;
        }

        /// <summary>Forgets the tags drawn so far; called once per repaint before any tag.</summary>
        public static void BeginTags()
        {
            tagsDrawn.Clear();
        }

        /// <summary>
        /// Draws a tag unless one is already drawn close by on screen, so a field of 100 positions stays readable.
        /// A forced tag (selected, hovered, the add ghost) is always drawn.
        /// </summary>
        /// <summary>A thin line from the preview's centre out of its front, so the facing reads without changing the shape.</summary>
        public static void DrawFacing(PreviewShape _shape, Pose _pose, Color _colour)
        {
            Vector3 centre = _pose.position + _pose.rotation * _shape.LocalBounds.center;
            float length = Mathf.Max(0.5f, _shape.LocalBounds.extents.z + 0.5f);
            Handles.color = _colour;
            Handles.DrawLine(centre, centre + _pose.rotation * Vector3.forward * length, 2f);
        }

        public static void DrawTag(Vector3 _position, string _text, Color _colour, bool _force)
        {
            Vector2 onScreen = HandleUtility.WorldToGUIPoint(_position);
            if (!_force && IsCrowded(onScreen))
            {
                return;
            }
            tagsDrawn.Add(onScreen);
            GUIStyle style = TagStyle();
            style.normal.textColor = _colour;
            Handles.Label(_position, _text, style);
        }

        private static bool IsCrowded(Vector2 _onScreen)
        {
            foreach (Vector2 drawn in tagsDrawn)
            {
                bool closeAcross = Mathf.Abs(drawn.x - _onScreen.x) < TagSpacing.x;
                bool closeDown = Mathf.Abs(drawn.y - _onScreen.y) < TagSpacing.y;
                if (closeAcross && closeDown)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>True when a tag at this point is near enough to the camera to read.</summary>
        public static bool IsNearCamera(SceneView _view, Vector3 _position)
        {
            Camera camera = _view.camera;
            return camera != null && Vector3.Distance(camera.transform.position, _position) < TagDistance;
        }

        public static Color ColourFor(RelocationEditorModel.Position _position)
        {
            Color colour = RelocationLabels.ColourOf(_position.Address.Label);
            if (_position.IsTooSteep)
            {
                colour = TooSteep;
            }
            else if (_position.Overlaps || _position.HasDuplicateInstance)
            {
                colour = Warning;
            }
            colour.a = AlphaFor(_position);
            return colour;
        }

        public static float AlphaFor(RelocationEditorModel.Position _position)
        {
            if (_position.Match == null || !_position.IsInMainScene)
            {
                return UnreachedAlpha;
            }
            if (_position.MatchHasSetInstance)
            {
                return SetAlpha;
            }
            return PredictedAlpha;
        }

        public static string TagFor(RelocationEditorModel.Position _position)
        {
            string text = RelocationLabels.NameOf(_position.Address.Label) + " · Instance " + _position.Address.Instance;
            if (_position.IsTooSteep)
            {
                text += " · " + Mathf.RoundToInt(_position.Slope) + "°";
            }
            return text;
        }

        private static Material Material()
        {
            if (material != null)
            {
                return material;
            }
            material = new Material(Shader.Find("Hidden/Internal-Colored"));
            material.hideFlags = HideFlags.HideAndDontSave;
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_Cull", (int)CullMode.Back);
            material.SetInt("_ZWrite", 0);
            material.SetInt("_ZTest", (int)CompareFunction.LessEqual);
            return material;
        }

        private static GUIStyle TagStyle()
        {
            if (tagStyle != null)
            {
                return tagStyle;
            }
            tagStyle = new GUIStyle(EditorStyles.miniBoldLabel);
            tagStyle.alignment = TextAnchor.LowerCenter;
            tagStyle.padding = new RectOffset(4, 4, 1, 1);
            Texture2D background = new Texture2D(1, 1);
            background.hideFlags = HideFlags.HideAndDontSave;
            background.SetPixel(0, 0, new Color(0.08f, 0.08f, 0.08f, 0.72f));
            background.Apply();
            tagStyle.normal.background = background;
            return tagStyle;
        }
    }
}
