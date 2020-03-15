#if DEBUG
using ICities;
using ColossalFramework;

namespace HideCrosswalks {
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Utils;
    using static Utils.PrefabUtils;
    using static Utils.RoadUtils;

    public class TestOnLoad  {
        public void OnLevelLoaded(LoadMode mode) => Test();

        public static void Test() {
            if (!Extensions.IsActive)
                return;

            Log.Info("Benchmarking ...");
            //Benchmarks.MaterialCache();
            //Benchmarks.BannAllCrossings();
            Log.Info("Benchmarking Done!");

            //Log.Info("Testing ...");
            ////DebugTests.NameTest();
            ////DebugTests.Dumps();
            ////DebugTests.WierdNodeTest();
            ////DebugTests.UVTest();
            //_Test();

            //Log.Info("Testing Done!");
        }

        public static class Benchmarks {
            private static NetInfoExt[] NetInfoExtArray => NetInfoExt.NetInfoExtArray;
            private static List<Material> materialList;

            private static int CountCollisions(Material newMaterial) {
                return materialList.
                    Where(material => material.GetHashCode() == newMaterial.GetHashCode() && material != newMaterial).
                    Count();
            }

            public static void MaterialCache() {
                Log.Info("BENCHMARK> MaterialCache started ... ");
                materialList = new List<Material>();
                int count = PrefabCollection<NetInfo>.PrefabCount();
                int loadedCount = PrefabCollection<NetInfo>.LoadedCount();
                for (uint i = 0; i < loadedCount; ++i) {
                    NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                    if (info?.m_netAI != null && NetInfoExt.GetCanHideCrossings(info)) {
                        foreach(var nodeInfo in info.m_nodes) {
                            if (nodeInfo?.m_nodeMaterial == null || nodeInfo.m_directConnect )
                                continue;
                            var nodeMaterial = nodeInfo.m_nodeMaterial;

                            // processed and cache material
                            var material2 = MaterialUtils.HideCrossings(nodeMaterial, null, info);

                            // add to cached material list.
                            materialList.Add(nodeMaterial);
                        }
                    }
                } // end for

                int totalCollisions = 0;
                foreach (var material in materialList) {
                    totalCollisions += CountCollisions(material);
                }
                float averageCollisions = totalCollisions / (float)materialList.Count;
                Log.Info($"BENCHMARK> totalCollisions={totalCollisions} averageCollisions={averageCollisions}");

                Log.Info($"BENCHMARK> peforming cache speed benchmark:");
                for (int i = 0; i < 1000; ++i) {
                    foreach (var material in materialList) {
                        // its possible for some arguments to be null only if the material is cached already.
                        var material2 = MaterialUtils.HideCrossings(material, null, null);
                    }
                }
                Log.Info($"BENCHMARK> Done peforming cache speed benchmark {1000 * materialList.Count} times ");
            }

            public static void BannAllCrossings() {
                Log.Info("BENCHMARK> BannAllCrossings started ... ");
                for (ushort segmentID = 0; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                    foreach (bool bStartNode in new bool[] { false, true }) {
                        if (TMPEUTILS.HasCrossingBan(segmentID, bStartNode)) {
                            NetSegment segment = segmentID.ToSegment();
                            ushort nodeID = bStartNode ? segment.m_startNode : segment.m_endNode;
                            var flags = nodeID.ToNode().m_flags;
                            if ((flags & NetNode.Flags.Created & NetNode.Flags.Junction & NetNode.Flags.Deleted) !=
                                (NetNode.Flags.Created & NetNode.Flags.Junction))
                                continue;
                            Log.Info($"BENCHMARK> ban {segmentID} {nodeID} ");


                            //Ban:
                            TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.
                                SetPedestrianCrossingAllowed(segmentID, bStartNode, false);
                        }
                    }
                }
                Log.Info("BENCHMARK> BannAllCrossings Done!");
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
                //        Log.Info(Extensions.BIG(m));
                //    }
                //    catch { }
                //}
                node.m_material = node.m_nodeMaterial = seg.m_material;
                string m = $"{info.name}> Made node material == seg material ";
                Log.Info(Extensions.BIG(m));
            }


            public static void WierdNodeTest() {
                for (uint i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); ++i) {
                    NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                    if (RoadUtils.IsNormalGroundRoad(info)) {
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
                            Log.Info(m);
                        }
                    }
                }
                Log.Info(Extensions.BIG("DONE PRITING NAMES!"));
            }
            public static bool RoadNameEqual(string n1, string n2) => n1.Trim().ToLower() == n2.Trim().ToLower();

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
        }
    }
}

#endif
