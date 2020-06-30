using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HideCrosswalks.Utils {
    using ColossalFramework;
    using Patches;
    using Settings;

    public static class PrefabUtils {
        public static string[] ARPMapExceptions = new[] { "" }; // TODO complete list.
        public static bool PrefabsLoaded = false;

        public static void CachePrefabs() {
            Log.Info("CachePrefabs() called ...");
            Log.Info("Assembly is " + typeof(PrefabUtils).Assembly);

            Extensions.Init();
            TMPEUTILS.Init();
            NS2Utils.Init();
#if !DEBUG
            if (!Extensions.IsActive) {
                Log.Info("skipped caching prefabs in asset editor/map/scenario/... release build");
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

                        if (!segment.Info)
                            continue;

                        ushort nodeID = bStartNode ? segment.m_startNode : segment.m_endNode;

                        foreach (var node in segment.Info.m_nodes) {
                            if(node.m_directConnect)
                                continue;
                            var flags = nodeID.ToNode().m_flags;

                            //cache:
                            //Log.Info("Caching " + segment.Info.name);
                            if(nodeID.ToNode().m_flags.IsFlagSet(NetNode.Flags.Junction))
                                CalculateMaterialCommons.CalculateMaterial(node.m_nodeMaterial, nodeID, segmentID);
                        }
                    }
                }
            }
            PrefabsLoaded = true;
            Log.Info("all prefabs cached");
        }

        public static void ClearCache() {
            Log.Info("PrefabUtils.ClearCache() called");
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

        /// <summary>
        /// This method is very slow
        /// </summary>
        public static bool IsExemptedNExt(this NetInfo info) {
            if (!info.IsNExt() || info.IsNormalGroundRoad())
                return false;
            Log._Debug("info.GetGroundInfo()?.category=" + info.GetGroundInfo()?.category);
            return info.GetGroundInfo()?.category?.StartsWith("RoadsSmall") ?? false; //small and small heavy
        }
    } // end class
} // end namespace
