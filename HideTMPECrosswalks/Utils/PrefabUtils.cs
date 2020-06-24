using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HideCrosswalks.Utils {
    using Patches;
    using Settings;

    public static class PrefabUtils {
        public static string[] ARPMapExceptions = new[] { "" }; // TODO complete list.
        public static bool PrefabsLoaded = false;

        public static void CachePrefabs() {
            Log.Info("CachePrefabs() called ...");
            TMPEUTILS.Init();
            NS2Utils.Init();
#if !DEBUG
            if (Extensions.InAssetEditor) {
                Log.Info("skipped caching prefabs in asset editor release build");
                return;
            }
#endif
            NetInfoExt.InitNetInfoExtArray();
            TextureUtils.Init();
            MaterialUtils.Init();
            for (ushort segmentID = 0; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                foreach (bool bStartNode in new bool[] { false, true }) {
                    if (TMPEUTILS.HasCrossingBan(segmentID, bStartNode)) {
                        NetSegment segment = segmentID.ToSegment();
                        ushort nodeID = bStartNode ? segment.m_startNode : segment.m_endNode;
                        foreach (var node in segment.Info.m_nodes) {
                            if(node == null || node.m_directConnect)
                                continue;
                            var flags = nodeID.ToNode().m_flags;

                            //cache:
                            Log.Info("Caching " + segment.Info.name);
                            CalculateMaterialCommons.CalculateMaterial(node.m_nodeMaterial, nodeID, segmentID);
                        }
                    }
                }
            }
            PrefabsLoaded = true;
            Log.Info("all prefabs cached");
        }

        public static void ClearCache() {
            PrefabsLoaded = false;
            NetInfoExt.NetInfoExtArray = null;
            MaterialUtils.Clear();
            TextureUtils.Clear();
        }

        public static bool isAsym(this NetInfo info) => info.m_forwardVehicleLaneCount != info.m_backwardVehicleLaneCount;
        public static bool isOneWay(this NetInfo info) => info.m_forwardVehicleLaneCount == 0 || info.m_backwardVehicleLaneCount == 0;

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

        public static float ScaleRatio(this NetInfo info) {
            float ret = 1f;
            if (info.m_netAI is RoadAI) {
                bool b = info.HasDecoration() || info.HasMedian() || info.m_isCustomContent;
                b |= info.isAsym() && !info.isOneWay() && info.name != "AsymAvenueL2R3";
                if (!b)
                    ret = 0.91f;
                Log.Info(info.name + " : Scale: " + ret);
            }
            return ret;
        }

        public static bool IsNExt(this NetInfo info) {
            string c = info.m_class.name.ToLower();
            bool ret = c.StartsWith("next");
            //Log.Info($"IsNExt returns {ret} : {info.GetUncheckedLocalizedTitle()} : " + c);
            return ret;
        }

        public static bool NodeTextureIsNotUsed(NetInfo info, Material nodeMaterial, int texID) {
            Texture t1 = nodeMaterial.GetTexture(texID);
            if (t1 == null || t1.width == t1.height)
                return false;
            foreach (var seg in info.m_segments) {
                Texture t2 = seg.m_segmentMaterial.GetTexture(texID);
                if (t1 == t2)
                    return true;
            }
            return false;
        }
    } // end class
} // end namespace
