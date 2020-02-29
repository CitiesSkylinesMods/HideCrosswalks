using System;
using System.IO;
using UnityEngine;

namespace HideCrosswalks.Utils {
    public static class TMPEUTILS {
        private static bool warned=false;

        public static bool HasCrossingBan(ushort segmentID, ushort nodeID) {
            bool bStartNode = nodeID == segmentID.ToSegment().m_startNode;
            return HasCrossingBan(segmentID, bStartNode);
        }

        public static bool HasCrossingBan(ushort segmentID, bool bStartNode) {
            try {
                return _HasCrossingBan(segmentID, bStartNode);
            }
            catch (FileNotFoundException _) {
                if (!warned) {
                    Debug.Log("ERROR ****** TMPE not found! *****");
                    warned = true;
                }
            }
            catch (Exception e) {
                if (!warned) {
                    Debug.Log(e + "\n" + e.StackTrace);
                    warned = true;
                }
            }
            return false;
        }

        private static bool _HasCrossingBan(ushort segmentID, bool bStartNode) {
            return !TrafficManager.Manager.Impl.JunctionRestrictionsManager.Instance.IsPedestrianCrossingAllowed(segmentID, bStartNode);
        }
    }
}
