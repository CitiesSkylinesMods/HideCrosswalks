using UnityEngine;

using System.Diagnostics;
using System.IO;
using System.Threading;

namespace HideTMPECrosswalks.Utils {
    public static class TextureUtils {
        public delegate Texture2D TProcessor(Texture2D tex);
        public delegate Texture2D TProcessor2(Texture2D tex, Texture2D tex2);

        public static class DumpJob {
            public static bool Lunch(NetInfo info) =>
                ThreadPool.QueueUserWorkItem((_) => Dump(info));
            static object locker = new object();
            public static void Dump(NetInfo info) {
                // not intersted in multi-thread processing. I just wan it to be outside of simulation thread.
                lock (locker) {
                    string dir = info.name;
                    //for (int i = 0; i < info.m_segments.Length; ++i)
                    {
                        int i = 0;
                        var seg = info.m_segments[i];
                        Material material = seg.m_segmentMaterial;
                        string baseName = "segment";
                        //if (info.m_segments.Length > 1) baseName += i;
                        Dump(material, "_MainTex", baseName, dir);
                        //Dump(material, "_ARPMap", baseName, dir);
                        //try { Dump(material, "_XYSMap", baseName, dir); } catch { }
                    }
                    for (int i = 0; i < info.m_nodes.Length; ++i) {
                        var node = info.m_nodes[i];
                        Material material = node.m_nodeMaterial;
                        string baseName = "node";
                        if (info.m_nodes.Length > 1) baseName += i;
                        Dump(material, "_MainTex", baseName, dir);
                        Dump(material, "_ARPMap", baseName, dir);
                        //try { Dump(material, "_XYSMap", baseName, dir); } catch { }
                    }
                }
            }

            public static void Dump(Material material, string texName, string baseName, string dir = "dummy") {
                Extensions.Log($"BEGIN : {texName} {baseName} {dir}");
                Texture2D texture = material.GetTexture(texName).MakeReadable() as Texture2D;
                byte[] bytes = texture.MakeReadable().EncodeToPNG();

                string path = @"mod debug dumps\" + dir;
                Directory.CreateDirectory(path);
                path += @"\" + baseName + texName + ".png";
                File.WriteAllBytes(path, bytes);
                Extensions.Log($"END: {texName} {baseName} {dir}");
            }
        }



        public static void Process(Material material, string name, TProcessor func) {
            Texture2D texture = material.GetTexture(name).MakeReadable() as Texture2D;
            Texture2D newTexture = func(texture);
            newTexture.anisoLevel = texture.anisoLevel;
            newTexture.Compress(true);
            material.SetTexture(name, newTexture);
        }

        public static void Process(Material nodeMaterial, Material segmentMaterial, string name, TProcessor2 func) {
            Texture2D nodeTex = nodeMaterial.GetTexture(name).MakeReadable() as Texture2D;
            Texture2D segTex = segmentMaterial.GetTexture(name).MakeReadable() as Texture2D;
            Texture2D newTexture = func(nodeTex,segTex);
            newTexture.anisoLevel = nodeTex.anisoLevel;
            newTexture.Compress(true);
            nodeMaterial.SetTexture(name, newTexture);
        }

        public static void Meld(Texture2D tex, Texture2D tex2) {

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

        public static Texture2D Crop(Texture2D original) {
            Texture2D ret = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;

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

        public static Texture2D CropOld(Texture2D original) {
            Texture2D ret = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;

            float cropPortion = 0.30f;
            float stretchPortion = 0.40f;
            int yN0 = (int)(yN * stretchPortion);

            for (int i = 0; i < xN; i++) {
                for (int j = 0; j < yN0; j++) {
                    int j2 = (int)(j * (stretchPortion - cropPortion) + yN * cropPortion);
                    ret.SetPixel(i, j, original.GetPixel(i, j2));
                }
                for (int j = yN0; j < yN; j++) {
                    ret.SetPixel(i, j, original.GetPixel(i, j));
                }
            }

            ret.Apply();
            Extensions.Log("ret.mipmapCount =" + ret.mipmapCount);
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

    }
}
