using System.Collections.Generic;
using UnityEngine;

namespace AbstractPixel.SceneManagement.Documentation
{
    /// <summary>
    /// A layered layout: each box goes in a layer one past the deepest box that points to it (or its hinted layer).
    /// Wide: layers are columns, left to right. Narrow: layers are rows, top to bottom, and a row wraps to the width.
    /// Boxes in a layer keep their order, so a hand-made graph keeps the order it was written in.
    /// </summary>
    public static class DocsGraphLayout
    {
        public const float BoxWidth = 136f;
        public const float BoxHeight = 50f;
        public const float ColumnGap = 32f;
        public const float RowGap = 14f;
        public const float NarrowRowGap = 30f;

        /// <summary>Box rectangles by id, and the size of the whole drawing.</summary>
        public static Dictionary<string, Rect> Place(DocsGraph _graph, float _width, out Vector2 _size)
        {
            List<List<DocsGraphNode>> layers = Layers(_graph);
            float wideWidth = layers.Count * BoxWidth + (layers.Count - 1) * ColumnGap;
            bool fitsWide = wideWidth <= _width;
            return fitsWide ? PlaceWide(layers, out _size) : PlaceNarrow(layers, _width, out _size);
        }

        public static List<List<DocsGraphNode>> Layers(DocsGraph _graph)
        {
            Dictionary<string, int> layerOf = new Dictionary<string, int>();
            foreach (DocsGraphNode node in _graph.Nodes)
            {
                layerOf[node.Id] = Mathf.Max(0, node.Layer);
            }
            // Longest path, a few passes (graphs here are small); a hint is a minimum.
            for (int pass = 0; pass < _graph.Nodes.Count; pass++)
            {
                bool changed = false;
                foreach (KeyValuePair<string, string> edge in _graph.Edges)
                {
                    bool known = layerOf.ContainsKey(edge.Key) && layerOf.ContainsKey(edge.Value);
                    bool isForward = known && IsForward(_graph, edge);
                    if (isForward && layerOf[edge.Value] < layerOf[edge.Key] + 1)
                    {
                        layerOf[edge.Value] = layerOf[edge.Key] + 1;
                        changed = true;
                    }
                }
                if (!changed)
                {
                    break;
                }
            }
            List<List<DocsGraphNode>> layers = new List<List<DocsGraphNode>>();
            foreach (DocsGraphNode node in _graph.Nodes)
            {
                int layer = layerOf[node.Id];
                while (layers.Count <= layer)
                {
                    layers.Add(new List<DocsGraphNode>());
                }
                layers[layer].Add(node);
            }
            layers.RemoveAll(_layer => _layer.Count == 0);
            return layers;
        }

        // An edge into a node with a fixed layer hint lower than its source would loop; such edges do not push layers.
        private static bool IsForward(DocsGraph _graph, KeyValuePair<string, string> _edge)
        {
            DocsGraphNode from = _graph.Find(_edge.Key);
            DocsGraphNode to = _graph.Find(_edge.Value);
            bool hinted = from.Layer >= 0 && to.Layer >= 0;
            return !hinted || to.Layer > from.Layer;
        }

        private static Dictionary<string, Rect> PlaceWide(List<List<DocsGraphNode>> _layers, out Vector2 _size)
        {
            Dictionary<string, Rect> rects = new Dictionary<string, Rect>();
            int tallest = 0;
            foreach (List<DocsGraphNode> layer in _layers)
            {
                tallest = Mathf.Max(tallest, layer.Count);
            }
            float height = tallest * BoxHeight + (tallest - 1) * RowGap;
            for (int column = 0; column < _layers.Count; column++)
            {
                List<DocsGraphNode> layer = _layers[column];
                float columnHeight = layer.Count * BoxHeight + (layer.Count - 1) * RowGap;
                float top = (height - columnHeight) * 0.5f;
                float x = column * (BoxWidth + ColumnGap);
                for (int row = 0; row < layer.Count; row++)
                {
                    rects[layer[row].Id] = new Rect(x, top + row * (BoxHeight + RowGap), BoxWidth, BoxHeight);
                }
            }
            _size = new Vector2(_layers.Count * BoxWidth + (_layers.Count - 1) * ColumnGap, height);
            return rects;
        }

        private static Dictionary<string, Rect> PlaceNarrow(List<List<DocsGraphNode>> _layers, float _width, out Vector2 _size)
        {
            Dictionary<string, Rect> rects = new Dictionary<string, Rect>();
            float boxWidth = Mathf.Min(BoxWidth, Mathf.Max(120f, _width));
            int perRow = Mathf.Max(1, Mathf.FloorToInt((_width + RowGap) / (boxWidth + RowGap)));
            float y = 0f;
            foreach (List<DocsGraphNode> layer in _layers)
            {
                for (int index = 0; index < layer.Count; index++)
                {
                    int column = index % perRow;
                    int row = index / perRow;
                    int inThisRow = Mathf.Min(perRow, layer.Count - row * perRow);
                    float rowWidth = inThisRow * boxWidth + (inThisRow - 1) * RowGap;
                    float left = Mathf.Max(0f, (_width - rowWidth) * 0.5f);
                    rects[layer[index].Id] = new Rect(left + column * (boxWidth + RowGap), y + row * (BoxHeight + RowGap), boxWidth, BoxHeight);
                }
                int rows = (layer.Count + perRow - 1) / perRow;
                y += rows * BoxHeight + (rows - 1) * RowGap + NarrowRowGap;
            }
            _size = new Vector2(_width, Mathf.Max(0f, y - NarrowRowGap));
            return rects;
        }
    }
}
