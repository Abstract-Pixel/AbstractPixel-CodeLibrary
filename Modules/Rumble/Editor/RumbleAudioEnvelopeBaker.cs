using UnityEditor;
using UnityEngine;

namespace AbstractPixel.Rumble.Editor
{
    /// <summary>
    /// Editor tool: turns a sound's loudness over time into a RumblePattern, so the shake follows the sound's rhythm.
    /// Heavy (left) motor = the deep part of the sound; light (right) motor = the sharp part (clicks, strums).
    /// Uses the same start, trim, delay and fades the game plays the sound with, so shake and sound line up.
    /// Quiet moments become 0, so gaps in the sound are gaps in the rumble.
    /// </summary>
    public static class RumbleAudioEnvelopeBaker
    {
        private const float KeysPerSecond = 30f;
        private const float LowPassHz = 150f;
        private const float SilenceGate = 0.06f;

        /// <summary>Bakes the sound into _pattern's motor curves and duration, then saves the asset.</summary>
        public static bool Bake(RumbleAudioBakeSettings _settings, RumblePattern _pattern)
        {
            if (_settings == null || _settings.Clip == null || _pattern == null)
            {
                Debug.LogWarning("[Rumble] Bake needs a clip and a pattern.");
                return false;
            }

            AudioClip clip = _settings.Clip;
            float[] samples = ReadWithDecompressedImport(clip);
            if (samples == null)
            {
                return false;
            }

            int windowSize = Mathf.Max(1, Mathf.RoundToInt(clip.frequency / KeysPerSecond));
            int firstWindow = Mathf.FloorToInt(_settings.StartPosition * KeysPerSecond);
            float playedEnd = clip.length - _settings.EndTrim;
            int lastWindow = Mathf.Min(samples.Length / windowSize, Mathf.FloorToInt(playedEnd * KeysPerSecond));
            int playedWindows = lastWindow - firstWindow;
            if (playedWindows <= 0)
            {
                Debug.LogWarning("[Rumble] Nothing left of " + clip.name + " after start and trim.");
                return false;
            }

            RangeInt playedRange = new RangeInt(firstWindow, playedWindows);
            RumbleAudioEnvelope envelope = MeasureWindows(samples, clip.frequency, playedRange);
            float[] heavy = envelope.Heavy;
            float[] light = envelope.Light;
            ApplyFades(heavy, _settings);
            ApplyFades(light, _settings);

            int delayWindows = Mathf.RoundToInt(_settings.Delay * KeysPerSecond);
            int totalWindows = delayWindows + playedWindows;
            AnimationCurve heavyCurve = BuildCurve(heavy, delayWindows, _settings.Response);
            AnimationCurve lightCurve = BuildCurve(light, delayWindows, _settings.Response);
            ScaleCurve(lightCurve, _settings.LightMotorShare);
            float duration = totalWindows / KeysPerSecond;

            Undo.RecordObject(_pattern, "Bake rumble from audio");
            _pattern.SetBakedCurves(heavyCurve, lightCurve, duration);
            EditorUtility.SetDirty(_pattern);
            AssetDatabase.SaveAssetIfDirty(_pattern);
            return true;
        }

        /// <summary>
        /// Compressed clips can't be read. Re-import as Decompress On Load just for the read, then put the
        /// clip's own import settings back, so the game's audio settings never change.
        /// </summary>
        private static float[] ReadWithDecompressedImport(AudioClip _clip)
        {
            string clipPath = AssetDatabase.GetAssetPath(_clip);
            AudioImporter importer = AssetImporter.GetAtPath(clipPath) as AudioImporter;
            if (importer == null)
            {
                return ReadMonoSamples(_clip);
            }

            AudioImporterSampleSettings originalSettings = importer.defaultSampleSettings;
            if (originalSettings.loadType == AudioClipLoadType.DecompressOnLoad)
            {
                return ReadMonoSamples(_clip);
            }

            AudioImporterSampleSettings readableSettings = originalSettings;
            readableSettings.loadType = AudioClipLoadType.DecompressOnLoad;
            importer.defaultSampleSettings = readableSettings;
            importer.SaveAndReimport();

            AudioClip readableClip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
            float[] samples = ReadMonoSamples(readableClip);

            importer.defaultSampleSettings = originalSettings;
            importer.SaveAndReimport();
            return samples;
        }

        /// <summary>All channels averaged into one. Null (with a warning) when the clip's data can't be read.</summary>
        private static float[] ReadMonoSamples(AudioClip _clip)
        {
            int channels = _clip.channels;
            float[] interleaved = new float[_clip.samples * channels];
            bool isRead = _clip.GetData(interleaved, 0);
            if (isRead == false)
            {
                Debug.LogWarning("[Rumble] Can't read " + _clip.name + ". Set its Load Type to Decompress On Load.");
                return null;
            }

            float[] mono = new float[_clip.samples];
            for (int sampleIndex = 0; sampleIndex < mono.Length; sampleIndex++)
            {
                float sum = 0f;
                for (int channel = 0; channel < channels; channel++)
                {
                    sum += interleaved[sampleIndex * channels + channel];
                }

                mono[sampleIndex] = sum / channels;
            }

            return mono;
        }

        /// <summary>Per window: loudness (RMS) of the low part and of the rest, split by a one-pole low-pass.</summary>
        private static RumbleAudioEnvelope MeasureWindows(float[] _samples, int _sampleRate, RangeInt _windows)
        {
            int windowSize = Mathf.Max(1, Mathf.RoundToInt(_sampleRate / KeysPerSecond));
            float lowPassBlend = 1f - Mathf.Exp(-2f * Mathf.PI * LowPassHz / _sampleRate);
            float lowPassed = 0f;
            RumbleAudioEnvelope envelope = new RumbleAudioEnvelope(_windows.length);
            for (int window = 0; window < _windows.length; window++)
            {
                float lowSquares = 0f;
                float highSquares = 0f;
                int start = (_windows.start + window) * windowSize;
                for (int offset = 0; offset < windowSize; offset++)
                {
                    float sample = _samples[start + offset];
                    lowPassed += (sample - lowPassed) * lowPassBlend;
                    float highPart = sample - lowPassed;
                    lowSquares += lowPassed * lowPassed;
                    highSquares += highPart * highPart;
                }

                envelope.Heavy[window] = Mathf.Sqrt(lowSquares / windowSize);
                envelope.Light[window] = Mathf.Sqrt(highSquares / windowSize);
            }

            return envelope;
        }

        /// <summary>The sound's own fade in / fade out, applied to the loudness.</summary>
        private static void ApplyFades(float[] _levels, RumbleAudioBakeSettings _settings)
        {
            float playedLength = _levels.Length / KeysPerSecond;
            for (int index = 0; index < _levels.Length; index++)
            {
                float time = index / KeysPerSecond;
                float fadeIn = 1f;
                if (_settings.FadeIn > 0f)
                {
                    fadeIn = Mathf.Clamp01(time / _settings.FadeIn);
                }

                float fadeOut = 1f;
                if (_settings.FadeOut > 0f)
                {
                    float timeLeft = playedLength - time;
                    fadeOut = Mathf.Clamp01(timeLeft / _settings.FadeOut);
                }

                _levels[index] *= fadeIn * fadeOut;
            }
        }

        private static float FindLoudest(float[] _levels)
        {
            float loudest = 0f;
            for (int index = 0; index < _levels.Length; index++)
            {
                loudest = Mathf.Max(loudest, _levels[index]);
            }

            return loudest;
        }

        /// <summary>
        /// Each motor normalised to its own loudest moment, shaped by the response curve, quiet windows gated to 0,
        /// silent during the delay, linear between keys, ending at 0.
        /// </summary>
        private static AnimationCurve BuildCurve(float[] _levels, int _delayWindows, float _response)
        {
            int totalWindows = _delayWindows + _levels.Length;
            float loudest = FindLoudest(_levels);
            Keyframe[] keys = new Keyframe[totalWindows + 1];
            for (int index = 0; index < totalWindows; index++)
            {
                float level = 0f;
                int levelIndex = index - _delayWindows;
                if (levelIndex >= 0 && loudest > 0f)
                {
                    level = _levels[levelIndex] / loudest;
                }

                if (level < SilenceGate)
                {
                    level = 0f;
                }

                float shaped = Mathf.Pow(level, _response);
                float time = (float)index / totalWindows;
                keys[index] = new Keyframe(time, shaped);
            }

            keys[totalWindows] = new Keyframe(1f, 0f);
            AnimationCurve curve = new AnimationCurve(keys);
            for (int index = 0; index < keys.Length; index++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, index, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(curve, index, AnimationUtility.TangentMode.Linear);
            }

            return curve;
        }

        /// <summary>Scales every key, e.g. so the light motor sits a little under the heavy one.</summary>
        private static void ScaleCurve(AnimationCurve _curve, float _share)
        {
            Keyframe[] keys = _curve.keys;
            for (int index = 0; index < keys.Length; index++)
            {
                keys[index].value *= _share;
            }

            _curve.keys = keys;
        }
    }
}
