using System;
using System.IO;
using UnityEngine;

namespace HideTMPECrosswalks.Utils {
    public static class NS2Utils {
        static bool exists = true;

        public static bool HideJunction(ushort segmentID) {
            if (!exists)
                return false;
            try {
                return _HideJunction(segmentID);
            }
            catch (FileNotFoundException _) {
                Debug.Log("ERROR ****** NS2 not found! *****");
                exists = false;
            }
            catch (Exception e) {
                Debug.Log(e + "\n" + e.StackTrace);
                exists = false;
            }
            return false;

        }

        private static bool _HideJunction(ushort segmentID) {
            try {
                return _HideJunction1(segmentID);
            }
            catch {
                return _HideJunction2(segmentID);
            }
        }
        private static bool _HideJunction1(ushort segmentID) {
            var skin = NetworkSkins.Skins.NetworkSkinManager.SegmentSkins[segmentID];
            bool ret = skin != null && skin.m_nodeMarkingsHidden;
            //Extensions.Log($"_HideJunction1 segment:{segmentID} m_nodeMarkingsHidden:{skin.m_nodeMarkingsHidden} return:{ret}");
            return ret;
        }
        private static bool _HideJunction2(ushort segmentID) {
            var skin = NetworkSkins.Skins.NetworkSkinManager.SegmentSkins[segmentID];
            bool ret = skin != null && (skin.m_color.b - 0.506f < 0.001f);
            //Extensions.Log($"_HideJunction2 segment:{segmentID} skin_color:{skin.m_color} return:{ret}");
            return ret;
        }
    }
}
