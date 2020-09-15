using System;
using System.IO;
using UnityEngine;
using KianCommons;

namespace HideCrosswalks.Utils {
    internal static class NS2Utils {
        private static bool exists = true;
        internal static void Init() => exists = true;

        public static bool HideJunctionMarkings(ushort segmentID) {
            if (!exists)
                return false;
            try {
                return _HideJunction(segmentID);
            }catch(FileNotFoundException) {
                Log.Info("WARNING ****** NS2 not found! *****");
            }
            catch (TypeLoadException) {
                Log.Info("WARNING ****** unsupported NS2 version! *****");
            }
            catch (NullReferenceException) {
                Log.Info("WARNING ****** NS2 is disabled! *****");
            }
            catch (Exception e) {
                Log.Error(e.ToString());
            }
            exists = false;
            return false;
        }

        private static bool _HideJunction(ushort segmentID) {
            var skin = NetworkSkins.Skins.NetworkSkinManager.SegmentSkins[segmentID];
            bool ret = skin != null && skin.m_nodeMarkingsHidden;
            //Log.Info($"_HideJunction1 segment:{segmentID} m_nodeMarkingsHidden:{skin.m_nodeMarkingsHidden} return:{ret}");
            return ret;
        }
    }
}
