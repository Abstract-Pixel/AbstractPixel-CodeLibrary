using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AbstractPixel.Core.UI
{
    /// <summary>
    /// A rounded-rectangle outline drawn as a mesh (no sprite), with a 360° fill: FillAmount 0..1 draws the border
    /// clockwise from the top centre back round to itself. Any size keeps a crisp, even line thickness, which a
    /// stretched outline sprite with Image.Type.Filled can't do.
    /// The mesh only rebuilds when the fill, size or shape changes. Pulse the alpha through canvasRenderer.SetAlpha
    /// and the size through localScale: neither rebuilds the mesh, so a looping pulse costs nothing per frame.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class RadialOutlineGraphic : MaskableGraphic
    {
        [Tooltip("Line thickness in canvas units.")]
        [SerializeField, Min(0.5f)] private float thickness = 4f;

        [Tooltip("Corner radius in canvas units. Never less than the thickness, never more than half the shorter side.")]
        [SerializeField, Min(0f)] private float cornerRadius = 8f;

        [Tooltip("Points per rounded corner. More = smoother corners.")]
        [SerializeField, Range(1, 16)] private int cornerSegments = 6;

        [Tooltip("How much of the border is drawn, clockwise from the top centre. 1 = the full outline.")]
        [SerializeField, Range(0f, 1f)] private float fillAmount = 1f;

        // Reused every rebuild, so redrawing the outline allocates nothing.
        private readonly List<Vector2> outerPoints = new List<Vector2>(80);
        private readonly List<Vector2> innerPoints = new List<Vector2>(80);
        private readonly List<float> distances = new List<float>(80);

        public float FillAmount
        {
            get { return fillAmount; }
            set { SetFillAmount(value); }
        }

        public void SetFillAmount(float _fillAmount)
        {
            // Exact compare: a tween ending on 1 must land on exactly 1, not stop at 0.9999999.
            float clampedFill = Mathf.Clamp01(_fillAmount);
            if (clampedFill == fillAmount)
            {
                return;
            }

            fillAmount = clampedFill;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper _vertexHelper)
        {
            _vertexHelper.Clear();
            if (fillAmount <= 0f)
            {
                return;
            }

            BuildPath(GetPixelAdjustedRect());
            float totalLength = distances[distances.Count - 1];
            if (totalLength <= 0f)
            {
                return;
            }

            float drawnLength = totalLength * fillAmount;
            AddVertexPair(_vertexHelper, outerPoints[0], innerPoints[0]);

            for (int pointIndex = 1; pointIndex < outerPoints.Count; pointIndex++)
            {
                bool isLastSegment = distances[pointIndex] >= drawnLength;
                if (isLastSegment)
                {
                    AddPartialSegment(_vertexHelper, pointIndex, drawnLength);
                    break;
                }

                AddVertexPair(_vertexHelper, outerPoints[pointIndex], innerPoints[pointIndex]);
                AddQuadToPreviousPair(_vertexHelper);
            }
        }

        /// <summary>The segment the fill stops in: drawn up to the exact point the fill reaches.</summary>
        private void AddPartialSegment(VertexHelper _vertexHelper, int _pointIndex, float _drawnLength)
        {
            float segmentStart = distances[_pointIndex - 1];
            float segmentLength = distances[_pointIndex] - segmentStart;
            float segmentProgress = segmentLength > 0f ? (_drawnLength - segmentStart) / segmentLength : 1f;

            Vector2 outerEnd = Vector2.Lerp(outerPoints[_pointIndex - 1], outerPoints[_pointIndex], segmentProgress);
            Vector2 innerEnd = Vector2.Lerp(innerPoints[_pointIndex - 1], innerPoints[_pointIndex], segmentProgress);
            AddVertexPair(_vertexHelper, outerEnd, innerEnd);
            AddQuadToPreviousPair(_vertexHelper);
        }

        private void AddVertexPair(VertexHelper _vertexHelper, Vector2 _outer, Vector2 _inner)
        {
            Color32 vertexColor = color;
            _vertexHelper.AddVert(_outer, vertexColor, Vector4.zero);
            _vertexHelper.AddVert(_inner, vertexColor, Vector4.zero);
        }

        /// <summary>Joins the last two vertex pairs into a quad (two triangles).</summary>
        private static void AddQuadToPreviousPair(VertexHelper _vertexHelper)
        {
            int newOuter = _vertexHelper.currentVertCount - 2;
            int newInner = newOuter + 1;
            int previousOuter = newOuter - 2;
            int previousInner = newOuter - 1;
            _vertexHelper.AddTriangle(previousOuter, newOuter, newInner);
            _vertexHelper.AddTriangle(previousOuter, newInner, previousInner);
        }

        /// <summary>
        /// Fills the outer/inner point lists clockwise from the top centre, round every corner and back to the top
        /// centre, plus the running length along the outer edge at each point.
        /// </summary>
        private void BuildPath(Rect _rect)
        {
            outerPoints.Clear();
            innerPoints.Clear();
            distances.Clear();

            float halfShortSide = Mathf.Min(_rect.width, _rect.height) * 0.5f;
            float lineThickness = Mathf.Min(thickness, halfShortSide);
            // At least the thickness, so the inner edge always has a corner centre to turn around.
            float outerRadius = Mathf.Min(Mathf.Max(cornerRadius, lineThickness), halfShortSide);
            float innerRadius = outerRadius - lineThickness;

            // Corner centres, inset by the radius.
            float left = _rect.xMin + outerRadius;
            float right = _rect.xMax - outerRadius;
            float bottom = _rect.yMin + outerRadius;
            float top = _rect.yMax - outerRadius;

            AddPoint(new Vector2(_rect.center.x, _rect.yMax), new Vector2(_rect.center.x, _rect.yMax - lineThickness));
            AddCorner(new Vector2(right, top), 90f, outerRadius, innerRadius);
            AddCorner(new Vector2(right, bottom), 0f, outerRadius, innerRadius);
            AddCorner(new Vector2(left, bottom), -90f, outerRadius, innerRadius);
            AddCorner(new Vector2(left, top), -180f, outerRadius, innerRadius);
            AddPoint(new Vector2(_rect.center.x, _rect.yMax), new Vector2(_rect.center.x, _rect.yMax - lineThickness));
        }

        /// <summary>A quarter turn clockwise around one corner centre, starting at _startDegrees.</summary>
        private void AddCorner(Vector2 _centre, float _startDegrees, float _outerRadius, float _innerRadius)
        {
            for (int segment = 0; segment <= cornerSegments; segment++)
            {
                float degrees = _startDegrees - 90f * segment / cornerSegments;
                float radians = degrees * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

                Vector2 outer = _centre + direction * _outerRadius;
                Vector2 inner = _centre + direction * _innerRadius;
                AddPoint(outer, inner);
            }
        }

        private void AddPoint(Vector2 _outer, Vector2 _inner)
        {
            float distance = 0f;
            if (outerPoints.Count > 0)
            {
                Vector2 previousOuter = outerPoints[outerPoints.Count - 1];
                distance = distances[distances.Count - 1] + Vector2.Distance(previousOuter, _outer);
            }

            outerPoints.Add(_outer);
            innerPoints.Add(_inner);
            distances.Add(distance);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetVerticesDirty();
        }
#endif
    }
}
