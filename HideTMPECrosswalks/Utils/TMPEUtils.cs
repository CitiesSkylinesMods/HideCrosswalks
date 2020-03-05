using System;
using System.IO;
using UnityEngine;

namespace HideCrosswalks.Utils {
    internal static class TMPEUTILS {
        private static bool exists = true;
        internal static void Init() => exists = true;

        public static bool HasCrossingBan(ushort segmentID, ushort nodeID) {
            bool bStartNode = nodeID == segmentID.ToSegment().m_startNode;
            return HasCrossingBan(segmentID, bStartNode);
        }

        public static bool HasCrossingBan(ushort segmentID, bool bStartNode) {
            if (!exists)
                return false;
            try {
                return _HasCrossingBan(segmentID, bStartNode);
            }
            catch (TypeLoadException _) {
                Log.Info("WARNING ****** TM:PE not found! *****");
                exists = false;
            }
            catch (NullReferenceException _) {
                Log.Info("WARNING ****** TM:PE is disabled! *****");
                exists = false;
            }
            catch (Exception e) {
                Log.Error(e.ToString());
                exists = false;
            }
            return false;
        }

        private static bool _HasCrossingBan(ushort segmentID, bool bStartNode) {
            CSUtil.Commons.TernaryBool b = TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.GetPedestrianCrossingAllowed(segmentID, bStartNode);
            return b == CSUtil.Commons.TernaryBool.False;
        }
    }
}
