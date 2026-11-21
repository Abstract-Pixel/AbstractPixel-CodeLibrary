using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace AbstractPixel.Core
{
    /// <summary>
    /// Graphics-free samples use Unity's built-in Default-Material, which only the Built-in render pipeline draws.
    /// When Play starts, this swaps it for the active render pipeline's own default material on every renderer in
    /// this object's scene, so the sample looks the same in Built-in, URP and HDRP without referencing a pipeline asset.
    /// It changes nothing in edit mode and nothing on disk.
    /// </summary>
    [AddComponentMenu("Abstract Pixel/Pipeline Default Material")]
    public class PipelineDefaultMaterial : MonoBehaviour
    {
        private const string BuiltInDefaultMaterialName = "Default-Material";

        private void Awake()
        {
            Material pipelineMaterial = FindPipelineDefaultMaterial();
            if (pipelineMaterial == null)
            {
                // The Built-in render pipeline draws Default-Material as it is.
                return;
            }
            SwapInScene(gameObject.scene, pipelineMaterial);
        }

        private static Material FindPipelineDefaultMaterial()
        {
            RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
            if (pipeline == null)
            {
                return null;
            }
            return pipeline.defaultMaterial;
        }

        private static void SwapInScene(Scene _scene, Material _pipelineMaterial)
        {
            List<Renderer> renderers = new List<Renderer>();
            foreach (GameObject root in _scene.GetRootGameObjects())
            {
                root.GetComponentsInChildren(true, renderers);
                foreach (Renderer renderer in renderers)
                {
                    SwapOnRenderer(renderer, _pipelineMaterial);
                }
            }
        }

        private static void SwapOnRenderer(Renderer _renderer, Material _pipelineMaterial)
        {
            Material[] materials = _renderer.sharedMaterials;
            bool changed = false;
            for (int index = 0; index < materials.Length; index++)
            {
                bool isBuiltInDefault = materials[index] != null && materials[index].name == BuiltInDefaultMaterialName;
                if (isBuiltInDefault)
                {
                    materials[index] = _pipelineMaterial;
                    changed = true;
                }
            }
            if (changed)
            {
                _renderer.sharedMaterials = materials;
            }
        }
    }
}
