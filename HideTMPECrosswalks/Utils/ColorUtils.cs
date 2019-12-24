using UnityEngine;

namespace HideTMPECrosswalks.Utils {
    internal class ColorUtils {
        internal static float Smoothen(float c, float c_prev) {
            if (c_prev - c > 0.05f) c = c_prev - 0.005f;
            if (c_prev - c < -0.05f) c = c_prev + 0.005f;
            return c;
        }

        internal static Color Smoothen(Color c, Color c_prev) {
            if (c_prev.r - c.r > 0.05f) c.r = c_prev.r - 0.005f;
            if (c_prev.g - c.g > 0.05f) c.g = c_prev.g - 0.005f;
            if (c_prev.b - c.r > 0.05f) c.b = c_prev.b - 0.005f;
            if (c_prev.a - c.a > 0.05f) c.a = c_prev.a - 0.005f;
            return c;
        }


        internal static float Clamp(float c, float max = 0.3f) {
            if (c > max) c = max;
            if (c < -max) c = -max;
            return c;
        }

        internal static Color Clamp(Color c, float max = 0.3f) {
            c.r = Clamp(c.r, max);
            c.g = Clamp(c.g, max);
            c.b = Clamp(c.b, max);
            c.a = Clamp(c.a, max);
            return c;
        }
    }
}
