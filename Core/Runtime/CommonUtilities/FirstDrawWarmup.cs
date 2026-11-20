using System.Collections;
using UnityEngine;

namespace AbstractPixel.Core
{
    /// <summary>
    /// Removes first-draw hitches for things that stay hidden until gameplay shows them (a renderer switched off at
    /// start, a particle effect that only plays on an impact). Run on a level's first frames, under the scene transition:
    /// the mesh is drawn once with its own materials, tiny, just in front of the main camera, and the particles emit one
    /// particle each there, so the GPU state for both exists before gameplay needs it. Then everything goes back.
    /// </summary>
    public static class FirstDrawWarmup
    {
        private const int MaxFramesToWaitForCamera = 120;
        private const int FramesToKeepDrawn = 2;
        private const float DistanceInFrontOfCamera = 4f;
        private const float CopyScale = 0.001f;

        /// <summary>Draws a tiny copy of the renderer's mesh and materials in front of the camera for two frames.</summary>
        public static IEnumerator DrawRendererCopy(Renderer _renderer)
        {
            Camera viewCamera = null;
            for (int frame = 0; frame < MaxFramesToWaitForCamera && viewCamera == null; frame++)
            {
                viewCamera = Camera.main;
                yield return null;
            }

            if (viewCamera == null || _renderer == null)
            {
                yield break;
            }

            GameObject copy = BuildRendererCopy(_renderer, PointInFrontOf(viewCamera));
            if (copy == null)
            {
                yield break;
            }

            for (int frame = 0; frame < FramesToKeepDrawn; frame++)
            {
                yield return null;
            }

            Object.Destroy(copy);
        }

        /// <summary>Emits one particle of each system (children included) in front of the camera, then clears them.</summary>
        public static IEnumerator EmitParticlesOnce(ParticleSystem _particles)
        {
            Camera viewCamera = null;
            for (int frame = 0; frame < MaxFramesToWaitForCamera && viewCamera == null; frame++)
            {
                viewCamera = Camera.main;
                yield return null;
            }

            if (viewCamera == null || _particles == null)
            {
                yield break;
            }

            Transform particlesTransform = _particles.transform;
            Vector3 restPosition = particlesTransform.position;
            Quaternion restRotation = particlesTransform.rotation;
            particlesTransform.position = PointInFrontOf(viewCamera);
            ParticleSystem[] systems = _particles.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem system in systems)
            {
                system.Emit(1);
            }

            for (int frame = 0; frame < FramesToKeepDrawn; frame++)
            {
                yield return null;
            }

            if (_particles == null)
            {
                yield break;
            }

            _particles.Clear(true);
            particlesTransform.SetPositionAndRotation(restPosition, restRotation);
        }

        private static Vector3 PointInFrontOf(Camera _camera)
        {
            Transform cameraTransform = _camera.transform;
            return cameraTransform.position + (cameraTransform.forward * DistanceInFrontOfCamera);
        }

        private static GameObject BuildRendererCopy(Renderer _renderer, Vector3 _point)
        {
            MeshFilter sourceFilter = _renderer.GetComponent<MeshFilter>();
            if (sourceFilter == null || sourceFilter.sharedMesh == null)
            {
                return null;
            }

            GameObject copy = new GameObject(_renderer.name + " (first-draw warmup)");
            copy.layer = _renderer.gameObject.layer;
            copy.transform.SetPositionAndRotation(_point, _renderer.transform.rotation);
            copy.transform.localScale = Vector3.one * CopyScale;

            MeshFilter copyFilter = copy.AddComponent<MeshFilter>();
            copyFilter.sharedMesh = sourceFilter.sharedMesh;

            MeshRenderer copyRenderer = copy.AddComponent<MeshRenderer>();
            copyRenderer.sharedMaterials = _renderer.sharedMaterials;
            copyRenderer.shadowCastingMode = _renderer.shadowCastingMode;
            copyRenderer.receiveShadows = _renderer.receiveShadows;
            copyRenderer.renderingLayerMask = _renderer.renderingLayerMask;
            return copy;
        }
    }
}
