using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using KianCommons;
using KianCommons.Plugins;

namespace HideCrosswalks.Utils {
    internal static class TMPEUTILS {
        private static bool exists;
        internal static void Init() {
            exists = PluginUtil.GetTrafficManager().IsActive();
            if (!exists) {
                Log.Error("****** TM:PE not found! *****");
            }
        }

        public static bool HasCrossingBan(ushort segmentID, ushort nodeID) {
            bool bStartNode = nodeID == segmentID.ToSegment().m_startNode;
            return HasCrossingBan(segmentID, bStartNode);
        }

        public static bool HasCrossingBan(ushort segmentID, bool bStartNode) {
            if (!exists)
                return false;
            try {
                return _HasCrossingBan(segmentID, bStartNode);
            } catch(FileNotFoundException) {
                Log.Info("WARNING ****** TM:PE not found! *****");
            }
            catch (TypeLoadException) {
                Log.Info("WARNING ****** unsupported TM:PE version! *****");
            }
            catch (NullReferenceException) {
                Log.Info("WARNING ****** TM:PE is disabled! *****");
            }
            catch (Exception e) {
                Log.Error(e.ToString());
            }
            exists = false;
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool _HasCrossingBan(ushort segmentID, bool bStartNode) {
            return !TrafficManager.API.Implementations.ManagerFactory.JunctionRestrictionsManager.
                IsPedestrianCrossingAllowed(segmentID, bStartNode);
        }
    }
}
