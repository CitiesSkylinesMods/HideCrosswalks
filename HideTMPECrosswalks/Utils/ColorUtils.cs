using UnityEngine;

namespace HideTMPECrosswalks.Utils {
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
                colors[i] += colors2[i2]*w;
            }
        }

        public static void Addw_fast(Color[] colors, Color[] colors2, float w) {
            int n = colors.Length;
            for (int i = 0; i < n; i+=1) {
                colors[i] += colors2[i] * w;
            }
        }

        public static void Flip(this Color[] colors) {
            int last = colors.Length-1;
            for (int i = 0; i < colors.Length; ++i) {
                colors[i] = colors[last - i];
            }
        }

        #region smoothen
        public const float gradient = 0.005f;
        public const float edge = 0.05f;

        public static float Smoothen(float c, float c_prev, float gradient = gradient) {
            if (c_prev - c > edge) c = c_prev - gradient;
            if (c_prev - c < -edge) c = c_prev + gradient;
            return c;
        }

        public static float SmoothenDown(float c, float c_prev, float gradient = gradient) {
            if (c_prev - c > edge) c = c_prev - gradient;
            return c;
        }
        public static float SmoothenUp(float c, float c_prev, float gradient = gradient) {
            if (c_prev - c < -edge) c = c_prev + gradient;
            return c;
        }

        public static Color SmoothenAPR(Color c, Color c_prev, float gradient = gradient) {
            c.r = SmoothenUp(c.r, c_prev.r, gradient);
            c.g = SmoothenDown(c.g, c_prev.g, gradient);
            c.b = SmoothenDown(c.b, c_prev.b, gradient);
            return c;
        }


        public static Color Smoothen(Color c, Color c_prev, float gradient = gradient) {
            c.r = Smoothen(c.r, c_prev.r, gradient);
            c.g = Smoothen(c.g, c_prev.g, gradient);
            c.b = Smoothen(c.b, c_prev.b, gradient);
            c.a = Smoothen(c.a, c_prev.a, gradient);
            return c;
        }

        public static void Smoothen(this Color[] colors, float gradient = gradient) {
            for (int i = 1; i < colors.Length; ++i
                ) {
                colors[i] = Smoothen(colors[i], colors[i - 1], gradient * colors.Length / 1024f);
            }
            for (int i = colors.Length - 2; i >= 0; --i) {
                colors[i] = Smoothen(colors[i], colors[i + 1], gradient * colors.Length / 1024f);
            }
        }

        public static void SmoothenAPR(this Color[] colors, float gradient = gradient) {
            for (int i = 1; i < colors.Length; ++i) {
                colors[i] = SmoothenAPR(colors[i], colors[i - 1], gradient* colors.Length / 1024f);
            }
            for (int i = colors.Length - 2; i >= 0; --i) {
                colors[i] = SmoothenAPR(colors[i], colors[i + 1], gradient * colors.Length / 1024f);
            }
        }
        #endregion smoothen

        #region clamp
        public const float max = 0.25f;

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
            for (int i = 0; i < colors.Length; ++i) {
                colors[i] = Clamp(colors[i], max);
            }
        }
        #endregion clamp
    }
}
