using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ColossalFramework.Plugins;

namespace HideTMPECrosswalks.Utils {
    using static Extensions;

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
            bool bStartNode = nodeID == segmentID.ToSegment().m_startNode;
            CSUtil.Commons.TernaryBool b = TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.GetPedestrianCrossingAllowed(segmentID, bStartNode);
            return b == CSUtil.Commons.TernaryBool.False;
        }
    }
}
