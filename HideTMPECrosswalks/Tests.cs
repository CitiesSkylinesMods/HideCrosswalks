#if DEBUG
using ICities;
using UnityEngine;
using HideTMPECrosswalks.Utils;

namespace HideTMPECrosswalks {
    using static TextureUtils;

    public class TestOnLoad : LoadingExtensionBase {
        public override void OnCreated(ILoading loading) { base.OnCreated(loading); Test(); }
        public override void OnLevelLoaded(LoadMode mode) => Test();

        public static void Test() {
            if (!Extensions.InGame && !Extensions.InAssetEditor)
                return;

            //Extensions.Log("Testing ...");
            ////DebugTests.NameTest();
            ////DebugTests.Dumps();
            ////DebugTests.WierdNodeTest();
            //DebugTests.UVTest();

            //Extensions.Log("Testing Done!");
        }

    }

    public static class DebugTests {
        public static string R6L => "Six-Lane Road";
        public static string R4L => "Four-Lane Road";

        public static void MakeSameNodeSegMat(NetInfo info) {
            var node = info.m_nodes[0];
            var seg = info.m_segments[0];
            //foreach (string texType in TextureNames.Names) {
            //    try {
            //        Texture tseg = seg.m_material.GetTexture(texType);
            //        node.m_material.SetTexture(texType, tseg);
            //        string m = $"{info.name} - {texType}> Made node tex == seg tex ";
            //        Extensions.Log(Extensions.BIG(m));
            //    }
            //    catch { }
            //}
            node.m_material = node.m_nodeMaterial = seg.m_material;
            string m = $"{info.name}> Made node material == seg material ";
            Extensions.Log(Extensions.BIG(m));
        }

        public static void WierdNodeTest() {
            for (uint i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                if (PrefabUtils.IsNormalGroundRoad(info)) {
                    if (info.GetUncheckedLocalizedTitle() == R6L) {
                        info = (info.m_netAI as RoadAI).m_elevatedInfo;
                        MakeSameNodeSegMat(info);
                    }
                }
            }
        }
        public static void NameTest() {
            for (uint i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                if (info?.m_netAI is RoadAI) {
                    string name = info.GetUncheckedLocalizedTitle();
                    bool b;
                    //b = name.ToLower().Contains("asym");
                    b = true;
                    if (b) {
                        string m = name;
                        RoadAI ai = info.m_netAI as RoadAI;
                        //m += "|" + ai?.m_elevatedInfo?.name;
                        //m += "|" + ai?.m_bridgeInfo?.name;
                        //m += "|" + ai?.m_slopeInfo?.name;
                        //m += "|" + ai?.m_tunnelInfo?.name;
                        m += "|| level:" + info.GetClassLevel();
                        m += " class:" + info.m_class;
                        m += " category:" + info.category;
                        Extensions.Log(m);
                    }
                }
            }
            Extensions.Log(Extensions.BIG("DONE PRITING NAMES!"));
        }
        public static bool RoadNameEqual(string n1, string n2) => n1.Trim().ToLower() == n2.Trim().ToLower();

        public static void Dumps() {
            for (uint i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                if (info?.m_netAI is RoadAI) {
                    string name = info.GetUncheckedLocalizedTitle().Trim();
                    bool b = false;
                    b |= name == "Six-Lane Road";
                    //b |= name == "Six-Lane Road with Median";
                    //b |= name == "Eight-Lane Road";
                    //b |= name == "Four-Lane Road";
                    //b |= name == "Four-Lane Road with Median";
                    //b |= name.ToLower().Contains("suburb");
                    b |= name.ToLower().Contains("2+3");
                    b |= name.ToLower().Contains("2+4");
                    //b = info.category == "RoadsLarge";
                    if (b) {
                        Extensions.Log("found " + name);
                        info = (info.m_netAI as RoadAI).m_elevatedInfo;
                        DumpDebugTextures(info);
                    }
                }
            }
        }

        public static void UVTest() {
            NetInfo info1 = null, info2 = null;
            for (uint i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                if (info.IsNormalGroundRoad()) {
                    string name = info.GetUncheckedLocalizedTitle().Trim();
                    bool b1 = name == "Six-Lane Road";
                    bool b2 = name.Contains("Six-Lane Road") && name.ToLower().Contains("grass");
                    if (b1) info1 = info;
                    if (b2) info2 = info;
                    if (info1 != null && info2 != null) {
                        DumpUtils.LogUVs(info1, info2);
                        break;
                    }
                }
            }

        }

        public static void DumpDebugTextures(NetInfo info) {
            Material material = info.m_nodes[0].m_nodeMaterial;
            DumpUtils.Dump(material, ID_APRMap, baseName: "node-original ", info);
            Material material2 = info.m_segments[0].m_segmentMaterial;
            DumpUtils.Dump(material2, ID_APRMap, baseName: "segment-original", info);

            var tex = material2.GetTexture(ID_APRMap);
            if (info.isAsym()) tex = Process(tex, Mirror);
            float ratio = info.ScaleRatio();
            if (ratio != 1f) {
                Texture2D ScaleRatio(Texture2D t) => Scale(t, ratio);
                tex = Process(tex, ScaleRatio);
            }
            string s = ratio == 1 ? "segment-mirrored" : "segment-mirrored-scaled";
            string path = DumpUtils.GetFilePath(ID_APRMap, s, info);
            DumpUtils.Dump(tex, path);

            material = PrefabUtils.HideCrossing(material, info);
            DumpUtils.Dump(material, ID_APRMap, baseName: "node-processed ", info);
        }
    }
}

#endif
