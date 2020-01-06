
using System.Diagnostics;
using System.IO;
using System.Threading;
using System;
using System.Linq;

using UnityEngine;

namespace HideTMPECrosswalks.Utils {
    using static HideTMPECrosswalks.Utils.ColorUtils;
    public static class TextureUtils {
        public delegate Texture2D TProcessor(Texture2D tex);
        public delegate Texture2D TProcessor2(Texture2D tex, Texture2D tex2);
        internal static int ID_Defuse => NetManager.instance.ID_MainTex;
        internal static int ID_APRMap => NetManager.instance.ID_APRMap;
        internal static int ID_XYSMap => NetManager.instance.ID_XYSMap;
        internal static string getTexName(int id) {
            if (id == ID_Defuse) return "_MainTex";
            if (id == ID_APRMap) return "_APRMap";
            if (id == ID_XYSMap) return "_XYSMap";
            throw new Exception("Bad Texture ID");
        }
        internal static int[] texIDs => new int[]{ID_Defuse, ID_APRMap,ID_XYSMap };

        public static Texture2D GetReadableTexture(this Material material, int id) {
            Texture2D texture = material.GetTexture(id) as Texture2D;
            texture = texture.TryMakeReadable();
            Extensions.Log($"GetReadableTexture: Material={material.name} texture={texture.name}");
            return texture;
        }

        public static Texture2D TryMakeReadable(this Texture tex) {
            //Extensions.Log($"TryMakeReadable:  texture={tex.name}");
            Texture2D ret;
            try{ ret = tex.MakeReadable() as Texture2D; }
            catch{ ret = tex as Texture2D; }
            ret.name = tex.name;
            Extensions.Log("TryMakeReadable: " + tex.name);
            return ret;
        }

        public static Texture2D Process(Texture tex, TProcessor func) {
            if (tex == null)
                throw new ArgumentNullException("tex is null");
            if (func == null)
                throw new ArgumentNullException("func is null");

            Texture2D newTexture = tex.TryMakeReadable();
            newTexture = func(newTexture);
            newTexture.anisoLevel = tex.anisoLevel;
            //newTexture.Compress(true);
            newTexture.name = tex.name + "-" + Extensions.GetPrettyFunctionName(func.Method);
            return newTexture;
        }

        public static Texture2D Process(Texture tex, Texture tex2, TProcessor2 func) {
            if (tex == null)
                throw new ArgumentNullException("tex is null");
            if (tex2 == null)
                throw new ArgumentNullException("tex2 is null");
            if (func == null)
                throw new ArgumentNullException("func is null");

            Texture2D nodeTex = tex.TryMakeReadable();
            Texture2D segTex = tex2.TryMakeReadable();
            Texture2D newTexture = func(nodeTex, segTex);
            newTexture.anisoLevel = nodeTex.anisoLevel;
            //newTexture.Compress(true);
            newTexture.name = tex.name + "-" + Extensions.GetPrettyFunctionName(func.Method);

            return newTexture;
        }

        public static Texture2D MeldDiff(Texture2D tex, Texture2D tex2) {
            Extensions.Log($"MeldDiff node:<{tex.name}> segment:<{tex2.name}>");
            Texture2D ret = new Texture2D(tex.width, tex.height);
            int yM = (int)(tex.height * 0.3f);
            int yM2 = (int)(tex2.height * 0.4f); // hackish code to avoid dashed lines in NExt2.

            //float ratio = (float)tex2.width / tex.width;

            Color[] colors = tex.GetPixels(0, 0, tex.width, 1);
            Color[] diff = tex2.GetPixels(0, yM2, tex2.width, 1);

            diff.Subtract(colors);
            diff.Clamp();
            diff.SmoothenAPR();

            for (int j = 0; j < yM; j++) {
                colors = tex.GetPixels(0, j, tex.width, 1);
                float w = 1f - (float)j / (float)yM;
                colors.Addw(diff, w);
                ret.SetPixels(0, j, tex.width, 1, colors);
            }
            ret.SetPixels(0, yM, tex.width, tex.height - yM, tex.GetPixels(0, yM, tex.width, tex.height - yM));

            ret.Apply();

            //DumpUtils.Dump(tex, DumpUtils.GetFilePath("tex", "", "melddiff"));
            //DumpUtils.Dump(tex2, DumpUtils.GetFilePath("tex2", "", "melddiff"));
            //DumpUtils.Dump(ret, DumpUtils.GetFilePath("ret", "", "melddiff"));

            return ret;
        }

        /// <summary>
        /// stretches if ratiuo is bigger than 1.
        /// shrinks if ratio is smaller than 1.
        /// </summary>
        public static Texture2D Scale(Texture2D original, float ratio = 0.915f) {
            Extensions.Log($"Scaling {original.name} raito:{ratio}");
            int xN = original.width;
            int yN = original.height;
            Texture2D ret = new Texture2D(xN, yN);

            int half = xN / 2;
            for (int i = 0; i < xN; i++) {
                int diff = half - i;
                diff = (int)(diff * ratio);
                int i2 = half - diff;
                if (i2 < 0 && i2 >= xN)
                    continue;

                Color[] colors = original.GetPixels(i, 0, 1, yN);
                ret.SetPixels(i2, 0, 1, yN, colors);
            }
            ret.Apply();
            return ret;
        }

        public static Texture2D Crop(Texture2D original) {
            int xN = original.width;
            int yN = original.height;
            Texture2D ret = new Texture2D(xN,yN);

            float cropPortion = 0.30f;
            float stretchPortion = 0.40f;
            int yN0 = (int)(yN * stretchPortion);

            for (int j = 0; j < yN0; j++) {
                int j2 = (int)(j * (stretchPortion - cropPortion) + yN * cropPortion);
                ret.SetPixels(0, j, xN, 1, original.GetPixels(0, j2, xN, 1));
            }
            ret.SetPixels(0, yN0, xN, yN - yN0, original.GetPixels(0, yN0, xN, yN - yN0));

            ret.Apply();
            return ret;
        }

        public static bool IsInverted(this Texture tex) => tex.name.ToLower().Contains("inverted");
        public static bool IsElevated(this Texture tex) => tex.name.ToLower().Contains("elevated");

        public static Texture2D Mirror(Texture2D original) {
            Extensions.Log("Mirror: texture name:" + original.name);
            if (original.IsInverted())
                return MirrorInv(original);
            int xN = original.width;
            int yN = original.height;

            Texture2D ret = new Texture2D(xN, yN);

            int last = xN - 1;
            int first = 0;
            if (original.IsElevated()) {
                last = xN * 722 / 1024;
                first = xN * 107 / 1024;
            }
            int width = (last - first + 1);
            int pivot = width / 2 + first;
            Extensions.Log($"first:{first} last:{last} width:{width} pivot:{pivot}");

            {
                Color[] colors = original.GetPixels(0, 0, pivot, yN);
                ret.SetPixels(0, 0, pivot, yN, colors);
            }
            for (int i = pivot; i <= last; i++) {
                int i2 = last - i + first;
                Color[] colors = original.GetPixels(i2, 0, 1, yN);
                ret.SetPixels(i, 0, 1, yN, colors);
            }
            if (last + 1 < xN) {
                Color[] colors = original.GetPixels(last + 1, 0, xN - last -1, yN);
                ret.SetPixels(last + 1, 0, xN - last-1, yN, colors);
            }
            ret.Apply();
            return ret;
        }

        public static Texture2D MirrorInv(Texture2D original) {
            Extensions.Log("MirrorInv: texture name:" + original.name);

            int xN = original.width;
            int yN = original.height;
            Texture2D ret = new Texture2D(xN, yN);

            int last = xN - 1;
            int first = 0;
            if (original.IsElevated()) {
                last = xN * 722 / 1024;
                first = xN * 167 / 1024;
            }
            int width = (last - first + 1);
            int pivot = width / 2 + first;
            Extensions.Log($"first:{first} last:{last} width:{width} pivot:{pivot}");

            if (first > 0) {
                Color[] colors = original.GetPixels(0, 0, first, yN);
                ret.SetPixels(0, 0, first, yN, colors);
            }
            for (int i = first; i < pivot; i++) {
                int i2 = last - i + first;
                Color[] colors = original.GetPixels(i2, 0, 1, yN);
                ret.SetPixels(i, 0, 1, yN, colors);
            }
            {
                Color[] colors = original.GetPixels(pivot, 0, xN-pivot, yN);
                ret.SetPixels(pivot, 0, xN - pivot, yN, colors);
            }

            ret.Apply();
            return ret;
        }

#if OLD_CODE
       public static void Process(Material material, string name, TProcessor func) {
            Texture2D texture = material.GetReadableTexture(name);
            Texture2D newTexture = func(texture);
            newTexture.anisoLevel = texture.anisoLevel;
            newTexture.Compress(true);
            material.SetTexture(name, newTexture);
        }

        public static void Process(Material nodeMaterial, Material segmentMaterial, string name, TProcessor2 func) {
            Texture2D nodeTex = nodeMaterial.GetReadableTexture(name);
            Texture2D segTex = segmentMaterial.GetReadableTexture(name);

            Texture2D newTexture = func(nodeTex,segTex);
            newTexture.anisoLevel = nodeTex.anisoLevel;
            newTexture.Compress(true);
            nodeMaterial.SetTexture(name, newTexture);
        }

        ///CropOld => Crop where stretch=1f
        // I chose to write repeated code for the sake of simplicity and ease debugging.
        public static Texture2D CropOld(Texture2D original) {
            Texture2D ret = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;

            float cropPortion = 0.30f;
            //float stretchPortion = 1f;
            //int yN0 = (int)(yN * stretchPortion) = yN;

            for (int j = 0; j < yN; j++) {
                int j2 = (int)(j * (1 - cropPortion) + yN * cropPortion);
                ret.SetPixels(0, j, xN, 1, original.GetPixels(0, j2, xN, 1));
            }

            ret.Apply();
            return ret;
        }

        //Texture flipping script from https://stackoverflow.com/questions/35950660/unity-180-rotation-for-a-texture2d-or-maybe-flip-both
        public static Texture2D Flip(Texture2D original) {
            Texture2D ret = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;
            for (int i = 0; i < xN; i++) {
                for (int j = 0; j < yN; j++) {
                    ret.SetPixel(j, xN - i - 1, original.GetPixel(j, i)); // flip upside down
                }
            }

            ret.Apply();
            return ret;
        }



        public static Texture2D CropEnd(Texture2D original) {
            Texture2D ret = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;

            float cropPortion = 0.20f;
            float stretchPortion = 0.30f;
            int yN0 = (int)(yN * (1- stretchPortion));


            ret.SetPixels(0, 0, xN, yN, original.GetPixels(0, 0, xN, yN));
            for (int j = 0; j < yN-yN0; j++) {
                int j2 = (int)(j * (stretchPortion - cropPortion)) + yN0;
                ret.SetPixels(0, j, xN,1, original.GetPixels(0, j2,xN,1));
            }

            ret.Apply();
            return ret;
        }

                public static Color MedianColor = new Color(0.106f, 0.098f, 0.106f, 1.000f);
        public static Color GetMedianColor(Material material) {
            var ticks = System.Diagnostics.Stopwatch.StartNew();

            Texture2D texture = material.GetReadableTexture(TextureNames.Defuse);
            int xN = texture.width;
            int yN = texture.height;
            //int yN0 = (int)(yN * 3)/10; //yN*30%

            Color[] colors = texture.GetPixels(0, yN / 2, xN, 1);
            // sort the colors.
            //int Compare(Color c1, Color c2) => Math.Sign(c1.grayscale - c2.grayscale);
            //Array.Sort(colors, Compare);

            Color ret = colors[xN / 2]; // middle
            ticks.LogLap($"GetMedianColor : middle = {ret} ");
            Extensions.Log("GetMedianColor : min = " + colors[0]);

            return ret;
        }

        public static void SetMedianColor(Material material, Color ?color=null) {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color ?? MedianColor);
            tex.Apply();
            material.SetTexture("_MainTex", tex);
        }
#endif // OLD_CODE
    }
}
