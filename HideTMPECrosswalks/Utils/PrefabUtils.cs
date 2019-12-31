using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace HideTMPECrosswalks.Utils {
    using Patches;
    using Settings;
    using static TextureUtils;

    public static class PrefabUtils {


        private static bool IsNormalGroundRoad(NetInfo info) {
            try {
                if (info != null && info.m_netAI is RoadAI) {
                    var ai = info.m_netAI as RoadAI;
                    return ai.m_elevatedInfo != null && ai.m_slopeInfo != null;
                }
                return false;
            }
            catch (Exception e) {
                Extensions.Log(e.Message);
                Extensions.Log("IsNormalGroundRoad catched exception");
                Extensions.Log($"exception: info = {info}");
                Extensions.Log($"exception: info is {info.GetType()}");
                Extensions.Log($"Exception: name = {info?.name} ");
                return false;
            }
        }

        private static string GetRoadTitle(NetInfo info) {
            Extensions.Assert(IsNormalGroundRoad(info), $"IsNormalGroundRoad(info={info.name})");

            //TODO: use Regular expressions instead of to lower.
            string name = info.GetUncheckedLocalizedTitle().ToLower();

            List<string> postfixes = new List<string>(new[] { "with", "decorative", "grass", "trees", });
            //postfixes.Add("one-way");
            if (info.m_isCustomContent) {
                postfixes = new List<string>(new[] { "_data" });
            }

            foreach (var postfix in postfixes) {
                name = name.Replace(postfix, "");
            }

            name = name.Trim();
            return name;
        }


#if DEBUG

        public static class DebugTests {
            public static string R6L => "Six-Lane Road";
            public static string R4L => "Four-Lane Road";

            public static void NameTest() {
                for (uint i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); ++i) {
                    NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                    if (info?.m_netAI is RoadAI) {
                        string name = info.GetUncheckedLocalizedTitle();
                        bool b;
                        b = name.ToLower().Contains("asym");
                        //b = true;
                        if (b) {
                            string m = name;
                            RoadAI ai = info.m_netAI as RoadAI;
                            m += "|" + ai?.m_elevatedInfo?.name;
                            m += "|" + ai?.m_bridgeInfo?.name;
                            m += "|" + ai?.m_slopeInfo?.name;
                            m += "|" + ai?.m_tunnelInfo?.name;
                            Extensions.Log(m);
                        }

                    }
                }
            }
            public static bool RoadNameEqual(string n1, string n2) => n1.Trim().ToLower() == n2.Trim().ToLower();

            public static void Dumps() {
                for (uint i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); ++i) {
                    NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                    if (info?.m_netAI is RoadAI) {
                        string name = info.GetUncheckedLocalizedTitle().Trim();
                        bool b = false;
                        //b |= name.ToLower().Contains("12");
                        //b |= name == "Six-Lane Road";
                        //b |= name == "Six-Lane Road with Median";
                        //b |= name == "Eight-Lane Road";
                        //b |= name == "Four-Lane Road";
                        //b |= name == "Four-Lane Road with Median";
                        //b |= name.ToLower().Contains("suburb");
                        b |= name.ToLower().Contains("2+3");
                        b |= name.ToLower().Contains("2+4");
                        if (b) {
                            Extensions.Log("found " + name);
                            DumpDebugTextures(info);
                        }
                    }
                }
            }

            public static void DumpDebugTextures(NetInfo info) {
                string name = info.GetUncheckedLocalizedTitle();
                string alpha = TextureNames.AlphaMAP;
                Material material = info.m_nodes[0].m_nodeMaterial;
                DumpJob.Dump(material, alpha, baseName: "node-original ", dir: name);
                Material material2 = info.m_segments[0].m_segmentMaterial;
                DumpJob.Dump(material2, alpha, baseName: "segment-original", dir: name);

                string texName = TextureNames.AlphaMAP;
                var tex = material2.GetTexture(texName);
                if (info.isAsym()) tex = Process(tex, Mirror);
                string path = DumpJob.GetFilePath(texName, "segment-mirrored", info.GetUncheckedLocalizedTitle());
                DumpJob.Dump(tex, path);

                material = HideCrossing(material, info);
                DumpJob.Dump(material, alpha, baseName: "node-processed ", dir: name);


            }
        }
#endif

        public static string[] GetRoadNames() {
            List<string> ret = new List<string>();
            for (uint i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                if (IsNormalGroundRoad(info)) {
                    string name = GetRoadTitle(info);
                    if (name != null && !ret.Contains(name))
                        ret.Add(name);
                }
            }
            var ret2 = ret.ToArray();
            Array.Sort(ret2);
            return ret2;
        }

        // TODO make array.
        private static Hashtable Always_Table;
        private static Hashtable Never_Table;
        //private static bool[] Always_array;
        //private static bool[] Never_array;

        public static void CacheAlways(IEnumerable<string> roads) {
            int count = PrefabCollection<NetInfo>.LoadedCount();
            //Always_array = new bool[count];
            Always_Table = new Hashtable(count * 10);
            for (uint i = 0; i < count; ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                if (IsNormalGroundRoad(info)) {
                    string name = GetRoadTitle(info);
                    bool b = (bool)roads.Contains(name);
                    RoadAI ai = info.m_netAI as RoadAI;
                    Always_Table[i] = b;
                    //Extensions.Log($"naem converted to {name} | from {info.name}");
                    Always_Table[ai.m_elevatedInfo.m_prefabDataIndex] = b;
                    Always_Table[ai.m_slopeInfo.m_prefabDataIndex] = b;
                    Always_Table[ai.m_bridgeInfo.m_prefabDataIndex] = b;
                    Always_Table[ai.m_tunnelInfo.m_prefabDataIndex] = b;
                }
            }
        }

        public static void CacheNever(IEnumerable<string> roads) {
            int count = PrefabCollection<NetInfo>.LoadedCount();
            Never_Table = new Hashtable(count * 10);
            for (uint i = 0; i < count; ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                if (IsNormalGroundRoad(info)) {
                    string name = GetRoadTitle(info);
                    bool b = (bool)roads.Contains(name);
                    RoadAI ai = info.m_netAI as RoadAI;
                    Never_Table[i] = b;
                    Never_Table[ai.m_slopeInfo.m_prefabDataIndex] = b;
                    Never_Table[ai.m_elevatedInfo.m_prefabDataIndex] = b;
                    Never_Table[ai.m_bridgeInfo.m_prefabDataIndex] = b;
                    Never_Table[ai.m_tunnelInfo.m_prefabDataIndex] = b;
                }
            }
        }

        public static bool AlwaysZebra(NetInfo info) {
            try { return (bool)Always_Table[info.m_prefabDataIndex]; }
            catch {
                Extensions.Log($"AlwaysZebra:Always_array[{info.m_prefabDataIndex}] index out of range. info:{info.name}");
                return false;
            }
        }

        public static bool NeverZebra(NetInfo info) {
            try { return (bool)Never_Table[info.m_prefabDataIndex]; }
            catch {
                Extensions.Log($"NeverZebra:Never_array[{info.m_prefabDataIndex}] index out of range. info:{info.name}");
                return false;
            }
        }


        public static Hashtable MaterialCache = null;
        public static Hashtable TextureCache = null;
        public static string[] ARPMapExceptions = new[] { "" }; // TODO complete list.

        public static void CachePrefabs() {
            if (MaterialCache == null) {
                MaterialCache = new Hashtable(100);
            }
            if (TextureCache == null) {
                TextureCache = new Hashtable(100);
            }

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
            //MaterialCache = null;
            //TextureCache = null;
            Always_Table = Never_Table = null;
            Extensions.Log("cache cleared");
        }

        public static bool isAsym(this NetInfo info) => info.m_forwardVehicleLaneCount != info.m_backwardVehicleLaneCount;

        public static bool HasMedian(this NetInfo info) {
            foreach (var lane in info.m_lanes) {
                if (lane.m_laneType == NetInfo.LaneType.None) {
                    return true;
                }
            }
            return false;
        }

        public static bool HasDecoration(this NetInfo info) {
            string title = info.GetUncheckedLocalizedTitle().ToLower();
            return title.Contains("tree") || title.Contains("grass") || title.Contains("arterial");
        }

        public static bool ScaledNode(this NetInfo info) {
            bool ret = !info.HasDecoration() && !info.HasMedian() && !info.isAsym() && !info.m_isCustomContent;
            ret |= info.name == "AsymAvenueL2R3";
            Extensions.Log(info.name + " : Scale: " + ret);
            return ret;
        }

        public static Material HideCrossing(Material material, NetInfo info) {
            try {
                if (MaterialCache == null) {
                    return material; // exiting game.
                }
                if (MaterialCache.Contains(material)) {
                    return (Material)MaterialCache[material];
                }

                bool asym = info.isAsym();

                var ticks = System.Diagnostics.Stopwatch.StartNew();
                string defuse = TextureNames.Defuse;
                string alpha = TextureNames.AlphaMAP;
                Material ret = new Material(material);

                Texture tex = material.GetTexture(defuse);
                if (tex != null) {
                    if (TextureCache.Contains(tex)) {
                        tex = TextureCache[tex] as Texture;
                        Extensions.Log("Texture cache hit: " + tex.name);
                    } else {
                        //if (asym) tex = Process(tex, Mirror);
                        tex = Process(tex, Crop);
                        (tex as Texture2D).Compress(false);
                        TextureCache[tex] = tex;
                    }
                    ret.SetTexture(defuse, tex);
                }

                if (info.GetClassLevel() > ItemClass.Level.Level1 || info.m_isCustomContent) {
                    tex = material.GetTexture(alpha);
                    if (tex != null) {
                        if (TextureCache.Contains(tex)) {
                            tex = TextureCache[tex] as Texture;
                            Extensions.Log("Texture cache hit: " + tex.name);
                        } else {
                            tex = Process(tex, Crop);
                            Material material2 = info.m_segments[0]?.m_material;
                            Texture tex2 = material2?.GetTexture(alpha);
                            if (tex2 != null) {
                                Extensions.Log($"melding {info.name} - node material = {material.name} -> {ret} | segment material = {material2.name}");
                                if (asym) tex2 = Process(tex2, Mirror);
                                if (info.ScaledNode()) {
                                    tex2 = Process(tex2, Stretch);
                                }
                                tex = Process(tex, tex2, MeldDiff);
                            }
                            (tex as Texture2D).Compress(false); //TODO make un-readable?
                            TextureCache[tex] = tex;
                        }
                        ret.SetTexture(alpha, tex);
                    }
                }
                MaterialCache[material] = ret;

                Extensions.Log($"Cached new texture for {info.name} ticks=" + ticks.ElapsedTicks.ToString("E2"));
                return ret;
            }
            catch (Exception e){
                Extensions.Log(e.ToString());
                return material;
            }
        }
    }
}
