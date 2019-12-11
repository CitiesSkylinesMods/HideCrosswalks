using ICities;
using ColossalFramework;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using ColossalFramework.Plugins;
using static HideTMPECrosswalks.Utils.ShortCuts;

namespace HideTMPECrosswalks.Utils {
    public static class TMPEUTILS {
        public static bool tmpeDetected = false;
        public static readonly UInt64[] TMPE_IDs = { 583429740, 1637663252, 1806963141 };

        // called when level loading begins
        public static void FindTMPE() {
            tmpeDetected = false;
            foreach (PluginManager.PluginInfo current in PluginManager.instance.GetPluginsInfo()) {
                if (!tmpeDetected && current.isEnabled && (current.name.Contains("TrafficManager") || TMPE_IDs.Contains(current.publishedFileID.AsUInt64))) {
                    tmpeDetected = true;
                }
            }
            if (tmpeDetected) Debug.Log("Found TMPE!");
        }

        public static bool HasCrossingBan(ushort segmentID, ushort nodeID) {
            if (!tmpeDetected)
                return true;
            bool bStartNode = nodeID == Segment(segmentID).m_startNode;
            CSUtil.Commons.TernaryBool b = TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.GetPedestrianCrossingAllowed(segmentID, bStartNode);
            return b == CSUtil.Commons.TernaryBool.False;
        }

    }


    public static class TextureUtils {
        //Texture flipping script from https://gist.github.com/Cgameworld/f22cfe649a222faf8226e1d65a0782e1
        public static void FlipRoadNodeTextures() {
            string road = "Basic Road";
            var node = PrefabCollection<NetInfo>.FindLoaded(road).m_nodes[0];
            var nodeMaterial = node.m_nodeMaterial;
            Flip(nodeMaterial, "_MainTex");
            //Flip(nodeMaterial, "_APRMap");

        }

        public static void Flip(Material material, string name) {
            var nodeTextureMain = material.GetTexture(name) as Texture2D;
            byte[] bytes = nodeTextureMain.MakeReadable().EncodeToPNG();
            Texture2D newTexture = new Texture2D(1, 1);
            newTexture.LoadImage(bytes);
            newTexture.anisoLevel = 16;
            newTexture = FlipTexture(newTexture);
            newTexture.Compress(true);
            material.SetTexture(name, newTexture);
        }

        public static void Clean(Material material) {
            Texture2D newTexture = new Texture2D(1, 1);
            newTexture.anisoLevel = 16;

            //float luminance = 24f / 255f;
            Color color = Median(material);
            newTexture.SetPixel(0, 0, color);

            newTexture.Apply();
            newTexture.Compress(true);
            material.SetTexture("_MainTex", newTexture);
        }

        static Color Median(Material material) {
            var nodeTextureMain = material.GetTexture("_MainTex") as Texture2D;
            byte[] bytes = nodeTextureMain.MakeReadable().EncodeToPNG();
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            texture.anisoLevel = 16;
            int xN = texture.width;
            int yN = texture.height;
            Color[] arColors = new Color[xN*yN];
            Debug.Log("Begin calculateing median");

            for (int i = 0; i < xN; i++) {
                for (int j = 0; j < yN; j++) {
                    int idx = i * yN + yN;
                    arColors[idx] = texture.GetPixel(i,j);
                }
            }
            Debug.Log("Begin calculateing median");

            float Luminance(Color c) => (0.299f * c.r + 0.587f * c.g + 0.114f * c.b);
            int Sign(Color a, Color b) => Math.Sign(Luminance(a) - Luminance(b));
            Array.Sort(arColors, (a, b) => Sign(a, b) );

            return arColors[arColors.Length/2];
        }


        //Texture flipping script from https://stackoverflow.com/questions/35950660/unity-180-rotation-for-a-texture2d-or-maybe-flip-both
        static Texture2D FlipTexture(Texture2D original) {
            Texture2D flipped = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;
            for (int i = 0; i < xN; i++) {
                for (int j = 0; j < yN; j++) {
                    flipped.SetPixel(j, xN - i - 1, original.GetPixel(j, i)); // flip upside down
                }
            }

            flipped.Apply();
            return flipped;
        }

        internal static void SetAllLodDistances() {
            foreach (NetInfo prefab in from p in Resources.FindObjectsOfTypeAll<NetInfo>()
                                       where p != null
                                       select p) {
                foreach (NetInfo.Segment segment in prefab.m_segments) {
                    segment.m_lodRenderDistance = 100000f;
                }
                foreach (NetInfo.Node node in prefab.m_nodes) {
                    node.m_lodRenderDistance = float.MaxValue;
                }
            }
        }
    }


    public static class ShortCuts {
        internal static ref NetNode Node(ushort ID) => ref Singleton<NetManager>.instance.m_nodes.m_buffer[ID];
        internal static ref NetSegment Segment(ushort ID) => ref Singleton<NetManager>.instance.m_segments.m_buffer[ID];
        internal static NetManager netMan => Singleton<NetManager>.instance;
        internal static bool IsJunction(ushort nodeID) => (Node(nodeID).m_flags & NetNode.Flags.Junction) != 0;
    }

    public static class LogOnceT {
        private static List<string> listLogs = new List<string>();
        public static void LogOnce(string m) {
            Debug.Log(m);
            return;
            var st = new System.Diagnostics.StackTrace();
            var sf = st.GetFrame(2);
            string key = sf.GetMethod().Name + ": " + m;
            //if (!listLogs.Contains(key))
            {
                Debug.Log(key);
                //listLogs.Add(key);
            }
        }
    }

    public static class Bin {
        //Texture flipping script from https://gist.github.com/boformer/6524899363e97cedf45b
        public static void ReplaceTextures() {
            var networkName = "Basic Road"; // replace with the one you want to edit

            var segmentMaterial = PrefabCollection<NetInfo>.FindLoaded(networkName).m_segments[0].m_segmentMaterial;
            Texture2D texture2D;
            if (File.Exists("tt/" + networkName + "_D.png")) {
                texture2D = new Texture2D(1, 1);
                texture2D.LoadImage(File.ReadAllBytes("tt/" + networkName + "_D.png"));
                texture2D.anisoLevel = 0;
                segmentMaterial.SetTexture("_MainTex", texture2D);
            }
            if (File.Exists("tt/" + networkName + "_APR.png")) {
                texture2D = new Texture2D(1, 1);
                texture2D.LoadImage(File.ReadAllBytes("tt/" + networkName + "_APR.png"));
                texture2D.anisoLevel = 0;
                segmentMaterial.SetTexture("_APRMap", texture2D);
            }
            if (File.Exists("tt/" + networkName + "_XYS.png")) {
                texture2D = new Texture2D(1, 1);
                texture2D.LoadImage(File.ReadAllBytes("tt/" + networkName + "_XYS.png"));
                texture2D.anisoLevel = 0;
                segmentMaterial.SetTexture("_XYSMap", texture2D);
            }
        }

        //foreach (NetInfo netInfo in Resources.FindObjectsOfTypeAll<NetInfo>())

    }


}
