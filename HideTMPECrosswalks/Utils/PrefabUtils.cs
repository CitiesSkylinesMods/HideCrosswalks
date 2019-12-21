using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HideTMPECrosswalks.Utils {
    using Patches;
    using Settings;
    using System.Text.RegularExpressions;

    public static class PrefabUtils {


        private static bool IsNormalGroundRoad(NetInfo info) {
            try {
                if (info.m_netAI is RoadAI) {
                    var ai = info.m_netAI as RoadAI;
                    return ai.m_elevatedInfo != null && ai.m_slopeInfo != null;
                }
                return false;
            }
            catch(Exception e) {
                Extensions.Log($"Exception occured for info {info.name} " + e);
                return false;
            }
        }

        private static string GetRoadTitle(NetInfo info) {
            Extensions.Assert(IsNormalGroundRoad(info), $"IsNormalGroundRoad(info={info.name})");

            //TODO: use Regular expressions instead of to lower.
            string name = info.GetUncheckedLocalizedTitle().ToLower();

            List<string> postfixes = new List<string>(new[]{ "with",  "decorative", "grass", "trees", });
            //postfixes.Add("one-way");
            if (info.m_isCustomContent) {
                postfixes = new List<string>(new[]{ "_data" });
            }

            foreach (var postfix in postfixes) {
                name = name.Replace(postfix, "");
            }

            name = name.Trim();
            return name;
        }

        public static string[] GetRoadNames() {
            List<string> ret = new List<string>();
            for(uint i=0; i< PrefabCollection<NetInfo>.LoadedCount(); ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                if(IsNormalGroundRoad(info)) {
                    string name = GetRoadTitle(info);
                    if( name!=null && !ret.Contains(name) )
                        ret.Add(name);
                }
            }
            var ret2 = ret.ToArray();
            Array.Sort(ret2);
            return ret2;
        }

        private static bool[] Always_array;
        private static bool[] Never_array;

        public static void CacheAlways(IEnumerable<string> roads) {
            int count = PrefabCollection<NetInfo>.LoadedCount();
            Always_array = new bool[count];
            for (uint i = 0; i < count; ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                if (IsNormalGroundRoad(info)) {
                    string name = GetRoadTitle(info);
                    bool b = (bool)roads.Contains(name);
                    RoadAI ai = info.m_netAI as RoadAI;
                    Always_array[i] = b;
                    //Extensions.Log($"naem converted to {name} | from {info.name}");
                    Always_array[ai.m_elevatedInfo.m_prefabDataIndex] = b;
                    Always_array[ai.m_slopeInfo.m_prefabDataIndex] = b;
                    Always_array[ai.m_bridgeInfo.m_prefabDataIndex] = b;
                    Always_array[ai.m_tunnelInfo.m_prefabDataIndex] = b;
                }
            }
        }

        public static void CacheNever(IEnumerable<string> roads) {
            int count = PrefabCollection<NetInfo>.LoadedCount();
            Never_array = new bool[count];
            for (uint i = 0; i < count; ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                if (IsNormalGroundRoad(info)) {
                    string name = GetRoadTitle(info);
                    bool b = (bool)roads.Contains(name);
                    RoadAI ai = info.m_netAI as RoadAI;
                    Never_array[i] = b;
                    Never_array[ai.m_slopeInfo.m_prefabDataIndex] = b;
                    Never_array[ai.m_elevatedInfo.m_prefabDataIndex] = b;
                    Never_array[ai.m_bridgeInfo.m_prefabDataIndex] = b;
                    Never_array[ai.m_tunnelInfo.m_prefabDataIndex] = b;
                }
            }
        }

        public static bool AlwaysZebra(NetInfo info) {
            return Always_array[info.m_prefabDataIndex];//' Options.instance.Always.Contains(roadName);
        }

        public static bool NeverZebra(NetInfo info) {
            return Never_array[info.m_prefabDataIndex];
        }


        public static Hashtable NodeMaterialTable = new Hashtable(100);
        public static string[] ARPMapExceptions = new[] { "" }; // TODO complete list.

        public static void CachePrefabs() {
            for (ushort segmentID = 0; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                foreach (bool bStartNode in new bool[] { false, true }) {
                    if (TMPEUTILS.HasCrossingBan(segmentID, bStartNode)) {
                        NetSegment segment = segmentID.ToSegment();
                        ushort nodeID = bStartNode ? segment.m_startNode : segment.m_endNode;
                        foreach (var node in segment.Info.m_nodes) {
                            //cache:
                            Extensions.Log("Caching " + segment.Info.name);
                            NetNode_RenderInstance.CalculateMaterial(node.m_nodeMaterial, nodeID, segmentID);
                        }
                    }
                }
            }
            Extensions.Log("all prefabs cached");
        }

        public static void ClearALLCache() {
            NodeMaterialTable.Clear();
            Always_array = Never_array = null;
            Extensions.Log("cache cleared");
        }

        public static Material HideCrossing(Material material, NetInfo info) {
            if (NodeMaterialTable.Contains(material)) {
                return (Material)NodeMaterialTable[material];
            }
            var ticks = System.Diagnostics.Stopwatch.StartNew();
            Material ret = new Material(material);
            //TextureUtils.GetMedianColor(ret);
            TextureUtils.SetMedianColor(ret);

            //TextureUtils.TProcessor func = TextureUtils.Crop;
            //TextureUtils.Process(ret, "_MainTex", TextureUtils.Crop);


            if (info.GetClassLevel() > ItemClass.Level.Level1 || info.m_isCustomContent) {
                TextureUtils.Process(ret, "_APRMap", TextureUtils.Crop);
                //TextureUtils.DumpJob.Lunch(info);
            }
            NodeMaterialTable[material] = ret;

            Extensions.Log($"Cached new texture for {info.name} ticks=" + ticks.ElapsedTicks.ToString("E2"));
            return ret;
        }
    }
}
