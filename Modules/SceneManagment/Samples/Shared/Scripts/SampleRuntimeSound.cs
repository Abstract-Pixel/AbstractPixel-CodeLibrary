using UnityEngine;

namespace AbstractPixel.SceneManagement.Samples
{
    /// <summary>
    /// The one sound of the scene samples (sample 10's launch sting): a short sine tone with a fade-out, made at runtime
    /// with AudioClip.Create, so the samples ship no audio file.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SampleRuntimeSound : MonoBehaviour
    {
        private const int SampleRate = 44100;

        [SerializeField] private float frequency = 660f;
        [SerializeField] private float seconds = 0.6f;
        [SerializeField] private float volume = 0.4f;

        private AudioSource audioSource;
        private AudioClip clip;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            clip = MakeSting();
            audioSource.clip = clip;
        }

        /// <summary>Plays the sting once.</summary>
        public void Play()
        {
            if (audioSource == null || clip == null)
            {
                return;
            }
            audioSource.Play();
        }

        private void OnDestroy()
        {
            if (clip != null)
            {
                Destroy(clip);
            }
        }

        // A sine wave that fades out linearly, shorter than one second.
        private AudioClip MakeSting()
        {
            float clampedSeconds = Mathf.Clamp(seconds, 0.05f, 0.95f);
            int sampleCount = Mathf.RoundToInt(SampleRate * clampedSeconds);
            float[] samples = new float[sampleCount];
            for (int index = 0; index < sampleCount; index++)
            {
                float time = (float)index / SampleRate;
                float fade = 1f - (float)index / sampleCount;
                samples[index] = Mathf.Sin(2f * Mathf.PI * frequency * time) * fade * volume;
            }
            AudioClip sting = AudioClip.Create("Sample 10 Sting", sampleCount, 1, SampleRate, false);
            sting.SetData(samples, 0);
            return sting;
        }
    }
}
