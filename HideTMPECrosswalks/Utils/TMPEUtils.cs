using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ColossalFramework.Plugins;

namespace HideTMPECrosswalks.Utils {
    using static Extensions;

    // Code from roundabout builder: https://github.com/Strdate/AutomaticRoundaboutBuilder/blob/master/RoundaboutBuilder/ModLoadingExtension.cs
    public static class TMPEUTILS {
        public static bool tmpeDetected = false;
        public static readonly UInt64[] TMPE_IDs = { 583429740, 1637663252, 1806963141 };

        public static bool Init() {
            tmpeDetected = false;
            foreach (PluginManager.PluginInfo current in PluginManager.instance.GetPluginsInfo()) {
                if (!tmpeDetected && current.isEnabled && (current.name.Contains("TrafficManager") || TMPE_IDs.Contains(current.publishedFileID.AsUInt64))) {
                    tmpeDetected = true;
                }
            }
            if (tmpeDetected)
                Debug.Log("Found TMPE!");
            else
                Debug.LogError("TMPE  not found!");
            return tmpeDetected;
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
