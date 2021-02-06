using System;
using UnityEngine;
using System.Collections;
using ColossalFramework.UI;
using KianCommons;

namespace HideCrosswalks.Utils {
    using static ColorUtils;
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
        internal static int[] texIDs => new int[] { ID_Defuse, ID_APRMap, ID_XYSMap };

        internal static Hashtable TextureCache = null;
        public static void Init() => TextureCache = new Hashtable(PrefabCollection<NetInfo>.PrefabCount()*3);
        public static void Clear() => TextureCache = null;

        public static UITextureAtlas GetAtlas(string name) {
            UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            foreach(var atlas in atlases) {
                if (atlas.name == name)
                    return atlas;
            }
            return null;
            
        }

        /// <summary>
        /// reteurns a copy of the texture with the differenc that: mipmap=false, linear=false, readable=true;
        /// </summary>
        public static Texture2D GetReadableCopy(this Texture2D tex, bool linear = false) {
            Assertion.Assert(tex != null, "tex!=null");
            Assertion.Assert(tex is Texture2D, $"tex is Texture2D");
            Texture2D ret = tex.MakeReadable(linear);
            ret.name = tex.name;
            ret.anisoLevel = tex.anisoLevel;
            ret.filterMode = tex.filterMode;
            return ret;
        }

        public static Texture2D MakeReadable(this Texture texture, bool linear) {
            RenderTextureReadWrite RW_mode = linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.Default;
            RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RW_mode);
            Graphics.Blit(texture, rt);
            texture = rt.ToTexture2D();
            RenderTexture.ReleaseTemporary(rt);
            return texture as Texture2D;
        }

        public static bool IsReadable(this Texture2D texture) {
            try {
                texture.GetPixel(0, 0);
                return true;
            }
            catch {
                return false;
            }
        }

        public static Texture2D TryMakeReadable(this Texture2D texture) {
            if (texture.IsReadable())
                return texture;
            else
                return texture.MakeReadable();
        }

        public static void Finalize(this Texture2D texture, bool lod = false) {
            texture.Compress(true);
            if (lod) texture.Apply();
            else texture.Apply(true, true);
        }


        public static bool IsInverted(this Texture tex) => tex.name.ToLower().Contains("inverted");
        public static bool IsElevated(this Texture tex) => tex.name.ToLower().Contains("elevated");

        public static int Offset(this Texture2D tex) {
            // TODO, check if the texture is lod instead.
            switch (tex.width) {
                case 2048:
                case 1024:
                case 512:
                    return 0;
                case 128:
                    return 16;
                case 64:
                    return 8;
                default:
                    string m = $"unxpected texture width:{tex.width}. texture:{tex.name}";
                    Log.Info(m);
                    return 0;
            }
        }

        public static float OffsetPortion(this Texture2D tex) => tex.Offset() / (float)tex.width;

        internal static Color MedianColor = new Color32(25, 25, 25, 255);
        public static Texture2D GetSimpleDefuseTexture() {
            Texture2D ret = new Texture2D(1, 1);
            ret.SetPixel(0,0,MedianColor);
            ret.Apply();
            return ret;
        }

        public static void MeldDiff(this Texture2D tex, Texture2D tex2) {
            Log.Info($"MeldDiff node:<{tex.name}> segment:<{tex2.name}>");
            //DumpUtils.Dump(tex, DumpUtils.GetFilePath("tex", "", "melddiff"));
            //DumpUtils.Dump(tex2, DumpUtils.GetFilePath("tex2", "", "melddiff"));
            int offset = tex.Offset();
            int yM = (int)((tex.height - offset * 2) * 0.3f) + offset;
            int yM2 = (int)(tex2.height * 0.4f); // 0.4 is hackish code to avoid dashed lines in NExt2.

            Color[] colors = tex.GetPixels(0, 0, tex.width, 1);
            Color[] diff = tex2.GetPixels(0, yM2, tex2.width, 1);

            diff.Subtract(colors);
            diff.Clamp();
            diff.SmoothenAPR();

            for (int j = 0; j < yM; j++) {
                colors = tex.GetPixels(0, j, tex.width, 1);
                int j2 = j - offset;
                float w = j2 < 0 ? 1f : 1f - j2 / (float)(yM - offset);
                //Log.Info(""+w);
                colors.Addw(diff, w);
                tex.SetPixels(0, j, tex.width, 1, colors);
            }

            tex.Apply();
            tex.name += $".MeldDiff({tex2.name})";

            //DumpUtils.Dump(ret, DumpUtils.GetFilePath("ret", "", "melddiff"));
        }

        public static void NOP(this Texture2D texture) {
            texture.SetPixels(texture.GetPixels());
            texture.Apply();
            texture.name += ".NOP()";
        }


        public static void CropAndStrech(this Texture2D texture) {
            int xN = texture.width;
            int yN = texture.height;
            float cropPortion = 0.30f;
            float stretchPortion = 0.40f;
            cropPortion += texture.OffsetPortion();
            stretchPortion += texture.OffsetPortion();

            int yN0 = (int)(yN * stretchPortion);

            for (int j = 0; j < yN0; j++) {
                int j2 = (int)(j * (stretchPortion - cropPortion) + yN * cropPortion);
                texture.SetPixels(0, j, xN, 1, texture.GetPixels(0, j2, xN, 1));
            }

            texture.Apply();
            texture.name += ".CropAndStrech()";
        }


        public static Texture2D CreateTempCopy(this Texture2D tex) {
            Texture2D ret = new Texture2D(tex.width, tex.height, tex.format, false);
            ret.SetPixels(tex.GetPixels());
            ret.Apply();
            return ret;
        }

        public static Texture2D CutToSize(this Texture2D texture, int w, int h) {
            Texture2D ret = new Texture2D(w, h, texture.format, false);
            ret.SetPixels(texture.GetPixels(0,0,w,h));
            ret.Apply();
            ret.name = texture.name + $".CutToSize(w={w}, h={h})";

            return ret;
        }

        /// <summary>
        /// stretches if ratiuo is bigger than 1.
        /// shrinks if ratio is smaller than 1.
        /// </summary>
        public static void Scale(this Texture2D texture, float ratio = 0.915f) {
            if (ratio == 1f)
                return;
            Log.Info($"Scaling {texture.name} raito:{ratio}");
            int xN = texture.width;
            int yN = texture.height;
            Texture2D tmp = texture.CreateTempCopy();

            int half = xN / 2;
            float start = half - ratio * half;
            for (int i = 0; i < xN; i++) {
                int i2 = (int)(start + ratio * i);
                if (i2 < 0 || i2 >= xN)
                    continue;

                Color[] colors = tmp.GetPixels(i, 0, 1, yN);
                texture.SetPixels(i2, 0, 1, yN, colors);
            }
            texture.Apply();
            texture.name += $".Scale(ratio={ratio})";
        }



        public static void Mirror(this Texture2D texture) {
            Log.Info("Mirror: texture name:" + texture.name);
            if (texture.IsInverted())
                MirrorInv(texture);
            int xN = texture.width;
            int yN = texture.height;

            int last = xN - 1;
            int first = 0;
            if (texture.IsElevated()) {
                last = xN * 722 / 1024;
                first = xN * 107 / 1024;
            }
            int width = (last - first + 1);
            int pivot = width / 2 + first;
            Log.Info($"first:{first} last:{last} width:{width} pivot:{pivot}");


            for (int i = pivot; i <= last; i++) {
                int i2 = last - i + first;
                Color[] colors = texture.GetPixels(i2, 0, 1, yN);
                texture.SetPixels(i, 0, 1, yN, colors);
            }
            texture.Apply();
            texture.name += ".Mirror()";

        }

        public static void MirrorInv(this Texture2D texture) {
            Log.Info("MirrorInv: texture name:" + texture.name);

            int xN = texture.width;
            int yN = texture.height;

            int last = xN - 1;
            int first = 0;
            if (texture.IsElevated()) {
                last = xN * 722 / 1024;
                first = xN * 167 / 1024;
            }
            int width = (last - first + 1);
            int pivot = width / 2 + first;
            Log.Info($"first:{first} last:{last} width:{width} pivot:{pivot}");

            for (int i = first; i < pivot; i++) {
                int i2 = last - i + first;
                Color[] colors = texture.GetPixels(i2, 0, 1, yN);
                texture.SetPixels(i, 0, 1, yN, colors);
            }

            texture.Apply();
            texture.name += ".MirrorInv()";
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
            Log.Info("GetMedianColor : min = " + colors[0]);

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
