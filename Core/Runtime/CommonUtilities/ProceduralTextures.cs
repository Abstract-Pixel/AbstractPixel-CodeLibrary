using UnityEngine;

namespace AbstractPixel.Core
{
    /// <summary>
    /// Textures built in code (no art asset, no image model), so they are reproducible and tweakable by number.
    /// The caller owns the returned texture and destroys it when done.
    /// </summary>
    public static class ProceduralTextures
    {
        /// <summary>
        /// A white anti-aliased ring touching the texture's edge, _thicknessPixels wide, transparent elsewhere.
        /// Good for selection rings and ripples (tint it with the Image colour).
        /// </summary>
        public static Texture2D CreateRing(int _size, float _thicknessPixels, string _name)
        {
            Texture2D texture = new Texture2D(_size, _size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.name = _name;

            float centre = (_size - 1) * 0.5f;
            Vector2 centrePoint = new Vector2(centre, centre);
            float outerRadius = _size * 0.5f - 1f;
            float innerRadius = outerRadius - _thicknessPixels;
            Color32[] pixels = new Color32[_size * _size];

            for (int y = 0; y < _size; y++)
            {
                for (int x = 0; x < _size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), centrePoint);
                    // Half-pixel soft edges on both sides of the ring give the anti-aliasing.
                    float outerEdge = Mathf.Clamp01(outerRadius - distance + 0.5f);
                    float innerEdge = Mathf.Clamp01(distance - innerRadius + 0.5f);
                    byte alpha = (byte)Mathf.RoundToInt(255f * outerEdge * innerEdge);
                    pixels[y * _size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        /// <summary>
        /// A white burst: a hot round core with a soft falloff, plus _streakCount thin radial streaks of varied length
        /// and width. Alpha carries the shape, so use it with an additive material (muzzle flashes, impact blooms).
        /// The texture stays readable so an editor tool can save it as a PNG.
        /// </summary>
        public static Texture2D CreateRadialStreakBurst(int _size, int _streakCount, string _name)
        {
            Texture2D texture = new Texture2D(_size, _size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.name = _name;

            StreakShape[] streaks = BuildStreakShapes(_streakCount);
            float halfSize = _size * 0.5f;
            Color32[] pixels = new Color32[_size * _size];

            for (int y = 0; y < _size; y++)
            {
                for (int x = 0; x < _size; x++)
                {
                    Vector2 offset = new Vector2((x + 0.5f - halfSize) / halfSize, (y + 0.5f - halfSize) / halfSize);
                    float brightness = BurstBrightness(offset, streaks);
                    byte alpha = (byte)Mathf.RoundToInt(255f * Mathf.Clamp01(brightness));
                    pixels[y * _size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        /// <summary>
        /// A soft white smoke puff: a round falloff broken up by two octaves of noise, transparent at the edges, so
        /// billboard particles never show their square. Readable, so an editor tool can save it as a PNG.
        /// </summary>
        public static Texture2D CreateSoftPuff(int _size, string _name)
        {
            Texture2D texture = new Texture2D(_size, _size, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.name = _name;

            float halfSize = _size * 0.5f;
            Color32[] pixels = new Color32[_size * _size];
            for (int y = 0; y < _size; y++)
            {
                for (int x = 0; x < _size; x++)
                {
                    float offsetX = (x + 0.5f - halfSize) / halfSize;
                    float offsetY = (y + 0.5f - halfSize) / halfSize;
                    float distance = Mathf.Sqrt(offsetX * offsetX + offsetY * offsetY);
                    float falloff = Mathf.Clamp01(1f - distance);
                    falloff = falloff * falloff * (3f - 2f * falloff);

                    float broadNoise = Mathf.PerlinNoise(x * 4f / _size + 3.1f, y * 4f / _size + 7.7f);
                    float fineNoise = Mathf.PerlinNoise(x * 11f / _size + 1.3f, y * 11f / _size + 5.9f);
                    float noise = 0.55f + 0.3f * broadNoise + 0.15f * fineNoise;

                    byte alpha = (byte)Mathf.RoundToInt(255f * Mathf.Clamp01(falloff * noise));
                    pixels[y * _size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        /// <summary>
        /// A white lamp lens: a rounded rectangle filling the texture, with a hot inner core and a soft edge, so an
        /// additive quad reads as a lit lamp (headlamps, tail lamps, light bars). Readable for saving as a PNG.
        /// </summary>
        public static Texture2D CreateLampLens(int _width, int _height, string _name)
        {
            Texture2D texture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.name = _name;

            Color32[] pixels = new Color32[_width * _height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    float brightness = LampLensBrightness(x, y, _width, _height);
                    byte alpha = (byte)Mathf.RoundToInt(255f * Mathf.Clamp01(brightness));
                    pixels[y * _width + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        /// <summary>
        /// A thin horizontal lens streak: bright at the centre, fading along its length and sharply across it. Used as
        /// an additive billboard on a light seen head-on. Readable for saving as a PNG.
        /// </summary>
        public static Texture2D CreateLensStreak(int _width, int _height, string _name)
        {
            Texture2D texture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.name = _name;

            Color32[] pixels = new Color32[_width * _height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    float along = Mathf.Abs((x + 0.5f) / _width * 2f - 1f);
                    float across = Mathf.Abs((y + 0.5f) / _height * 2f - 1f);
                    float lengthFade = Mathf.Pow(1f - along, 2.2f);
                    float widthFade = Mathf.Exp(-across * across * 18f);
                    float hotCore = Mathf.Exp(-(along * along + across * across) * 40f);
                    float brightness = lengthFade * widthFade + hotCore;
                    byte alpha = (byte)Mathf.RoundToInt(255f * Mathf.Clamp01(brightness));
                    pixels[y * _width + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        // Signed distance to a rounded rectangle inset from the texture edge, turned into a soft edge plus a hot core.
        private static float LampLensBrightness(int _x, int _y, int _width, int _height)
        {
            float halfWidth = _width * 0.5f;
            float halfHeight = _height * 0.5f;
            float cornerRadius = Mathf.Min(halfWidth, halfHeight) * 0.6f;
            float edgeSoftness = Mathf.Min(halfWidth, halfHeight) * 0.35f;

            float offsetX = Mathf.Abs(_x + 0.5f - halfWidth) - (halfWidth - edgeSoftness - cornerRadius);
            float offsetY = Mathf.Abs(_y + 0.5f - halfHeight) - (halfHeight - edgeSoftness - cornerRadius);
            float outsideX = Mathf.Max(offsetX, 0f);
            float outsideY = Mathf.Max(offsetY, 0f);
            float outsideDistance = Mathf.Sqrt(outsideX * outsideX + outsideY * outsideY);
            float insideDistance = Mathf.Min(Mathf.Max(offsetX, offsetY), 0f);
            float signedDistance = outsideDistance + insideDistance - cornerRadius;

            float body = Mathf.Clamp01(0.5f - signedDistance / edgeSoftness);
            float core = Mathf.Clamp01(-signedDistance / (Mathf.Min(halfWidth, halfHeight) * 0.8f));
            return body * 0.75f + core * 0.25f;
        }

        private struct StreakShape
        {
            public float Angle;
            public float Length;
            public float HalfWidth;
        }

        // Fixed seed so the same call always draws the same burst.
        private static StreakShape[] BuildStreakShapes(int _streakCount)
        {
            System.Random random = new System.Random(7919);
            StreakShape[] streaks = new StreakShape[_streakCount];
            for (int index = 0; index < _streakCount; index++)
            {
                float evenAngle = index * Mathf.PI * 2f / _streakCount;
                float jitter = (float)(random.NextDouble() - 0.5) * 0.5f;
                streaks[index].Angle = evenAngle + jitter;
                streaks[index].Length = 0.45f + (float)random.NextDouble() * 0.55f;
                streaks[index].HalfWidth = 0.006f + (float)random.NextDouble() * 0.02f;
            }

            return streaks;
        }

        private static float BurstBrightness(Vector2 _offset, StreakShape[] _streaks)
        {
            float distance = _offset.magnitude;
            float core = Mathf.Clamp01(1f - distance / 0.22f);
            float glow = Mathf.Pow(Mathf.Clamp01(1f - distance), 3f) * 0.55f;
            float streakLight = 0f;

            float angle = Mathf.Atan2(_offset.y, _offset.x);
            foreach (StreakShape streak in _streaks)
            {
                float angleDifference = Mathf.DeltaAngle(angle * Mathf.Rad2Deg, streak.Angle * Mathf.Rad2Deg) * Mathf.Deg2Rad;
                float sidewaysDistance = Mathf.Abs(Mathf.Sin(angleDifference)) * distance;
                bool isAhead = Mathf.Cos(angleDifference) > 0f;
                if (isAhead == false || distance > streak.Length)
                {
                    continue;
                }

                float across = Mathf.Clamp01(1f - sidewaysDistance / streak.HalfWidth);
                float along = 1f - distance / streak.Length;
                streakLight = Mathf.Max(streakLight, across * along);
            }

            return core + glow + streakLight;
        }
    }
}
