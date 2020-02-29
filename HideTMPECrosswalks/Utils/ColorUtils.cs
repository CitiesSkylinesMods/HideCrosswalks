using UnityEngine;

namespace HideCrosswalks.Utils {
    public static class ColorUtils {
        public static void Subtract(this Color[] diff, Color[] colors) {
            float ratio = (float)colors.Length / diff.Length;
            for (int i = 0; i < diff.Length; ++i) {
                int i2 = (int)(i * ratio);
                diff[i] -= colors[i2];
            }
        }

        public static void Addw(this Color[] colors, Color[] colors2, float w) {
            float ratio = (float)colors2.Length / colors.Length;
            if (ratio == 1f) {
                Addw_fast(colors, colors2, w);
                return;
            }
            for (int i = 0; i < colors.Length; ++i) {
                int i2 = (int)(i * ratio);
                colors[i] += colors2[i2] * w;
            }
        }

        public static void Addw_fast(Color[] colors, Color[] colors2, float w) {
            int n = colors.Length;
            for (int i = 0; i < n; i += 1) {
                colors[i] += colors2[i] * w;
            }
        }

        public static void Flip(this Color[] colors) {
            int last = colors.Length - 1;
            for (int i = 0; i < colors.Length; ++i) {
                colors[i] = colors[last - i];
            }
        }

        #region smoothen
        public const float gradient = 0.005f;
        public const float edge = 0.075f;

        public static float Smoothen(float c, float c_prev, float gradient = gradient, float edge = edge) {
            c = SmoothenUp(c, c_prev, gradient, edge);
            c = SmoothenDown(c, c_prev, gradient, edge);
            return c;
        }

        public static float SmoothenDown(float c, float c_prev, float gradient = gradient, float edge = edge) {
            if (c_prev - c > edge) c = c_prev - gradient;
            return c;
        }
        public static float SmoothenUp(float c, float c_prev, float gradient = gradient, float edge = edge) {
            if (c_prev - c < -edge) c = c_prev + gradient;
            return c;
        }

        public static Color SmoothenAPR(Color c, Color c_prev, float gradient = gradient, float edge = edge) {
            c.r = SmoothenUp(c.r, c_prev.r, gradient, edge); //alpha
            c.g = SmoothenDown(c.g, c_prev.g, gradient, edge); // pavement
            c.b = SmoothenDown(c.b, c_prev.b, gradient, edge); // road
            return c;
        }


        public static Color Smoothen(Color c, Color c_prev, float gradient = gradient, float edge = edge) {
            c.r = Smoothen(c.r, c_prev.r, gradient, edge);
            c.g = Smoothen(c.g, c_prev.g, gradient, edge);
            c.b = Smoothen(c.b, c_prev.b, gradient, edge);
            c.a = Smoothen(c.a, c_prev.a, gradient, edge);
            return c;
        }

        public static void Smoothen(this Color[] colors, float gradient = gradient) {
            gradient *= 1024f / colors.Length;
            float edge = ColorUtils.edge;
            if (colors.Length == 128) edge *= 2;
            for (int i = 1; i < colors.Length; ++i
                ) {
                colors[i] = Smoothen(colors[i], colors[i - 1], gradient, edge);
            }
            for (int i = colors.Length - 2; i >= 0; --i) {
                colors[i] = Smoothen(colors[i], colors[i + 1], gradient, edge);
            }
        }

        public static void SmoothenAPR(this Color[] colors, float gradient = gradient) {
            gradient *= 1024f / colors.Length;
            float edge = ColorUtils.edge;
            if (colors.Length <= 256) edge *= 2;
            for (int i = 1; i < colors.Length; ++i) {
                colors[i] = SmoothenAPR(colors[i], colors[i - 1], gradient, edge);
            }
            for (int i = colors.Length - 2; i >= 0; --i) {
                colors[i] = SmoothenAPR(colors[i], colors[i + 1], gradient, edge);
            }
        }
        #endregion smoothen

        #region clamp
        public const float max = 0.4f;

        public static float Clamp(float c, float max = max) {
            return Mathf.Clamp(c, -max, max);
        }

        public static Color Clamp(Color c, float max = max) {
            c.r = Clamp(c.r, max);
            c.g = Clamp(c.g, max);
            c.b = Clamp(c.b, max);
            c.a = Clamp(c.a, max);
            return c;
        }

        public static void Clamp(this Color[] colors, float max = max) {
            if (colors.Length <= 256) max *= 1.5f;
            for (int i = 0; i < colors.Length; ++i) {
                colors[i] = Clamp(colors[i], max);
            }
        }
        #endregion clamp
    }
}

