using ColossalFramework;
using HideCrosswalks.Lifecycle;
using KianCommons;
using KianCommons.Plugins;
using System;
using System.Collections.Generic;

namespace HideCrosswalks.Utils {

    /// <summary>
    /// Road encapsulation:
    /// A road can have several prefabs: elavated/slope/bridge/tunnel/ground
    /// </summary>
    public static class RoadUtils {
        internal static bool IsNormalRoad(this NetInfo info) {
            try {
                if (info == null) return false;
                string name = info.name;
                return
                    info.m_netAI is RoadBaseAI &&
                    !name.IsNullOrWhiteSpace()
                    && !name.ToLower().Contains("toll");

            } catch (Exception e) {
                try {
                    Log.Info("IsNormalRoad catched exception");
                    Log.Info($"exception: info = {info}");
                    Log.Info($"exception: info type = {info?.GetType()}");
                    Log.Info($"Exception: name = {info?.name} ");
                    Log.Exception(e);
                } catch (Exception e2) {
                    Log.Exception(e2, "error occured while trying to print error details!");
                }
            }
            return false;
        }

        internal static bool IsNormalGroundRoad(this NetInfo info) {
            return info?.m_netAI is RoadAI ai && info.IsNormalRoad();
        }

        /// <summary>
        /// Returns an array of roads for which TMPE can hide crossings.
        /// </summary>
        /// <returns></returns>
        public static string[] GetRoadNames() {
            List<string> ret = new List<string>();
            int n = PrefabCollection<NetInfo>.LoadedCount();
            for (uint i = 0; i < n; ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                if (CalculateCanHideCrossingsRaw(info) && IsNormalGroundRoad(info)) {
                    string name = GetRoadTitle(info);
                    if (name != null && !ret.Contains(name))
                        ret.Add(name);
                }
            }
            var ret2 = ret.ToArray();
            Array.Sort(ret2);
            return ret2;
        }

        public static NetInfo GetGroundInfo(this NetInfo info) {
            if (info.m_netAI is RoadAI)
                return info;
            int n = PrefabCollection<NetInfo>.LoadedCount();
            for (uint i = 0; i < n; ++i) {
                NetInfo info2 = PrefabCollection<NetInfo>.GetLoaded(i);
                if (IsNormalGroundRoad(info2)) {
                    RoadAI ai = info2.m_netAI as RoadAI;
                    bool b;
                    b = ai.m_elevatedInfo == info;
                    b |= ai.m_bridgeInfo == info;
                    b |= ai.m_tunnelInfo == info;
                    b |= ai.m_slopeInfo == info;
                    if (b)
                        return info2;
                }
            }
            return null;//in case of failure.
        }

        private static string GetRoadTitle(NetInfo info) {
            Assertion.Assert(info.IsNormalGroundRoad(), $"IsNormalGroundRoad(info={info.name})");

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

        public static bool IsExempt(NetInfo info) {
            if (info?.m_netAI is not RoadBaseAI ai)
                return true;
            if (ai is not RoadAI)
                info = GetGroundInfo(info);
            if (!IsNormalGroundRoad(info))
                return true;

            string name = GetRoadTitle(info);
            bool never = Options.instance?.Never?.Contains(name) ?? false;
            return never;
        }

        private static List<string> exempts_ = new List<string>(new[]{
            "AsymAvenueL2R4 Slope",
            "Gravel Road",
        });

        internal static bool CalculateCanHideMarkingsRaw(NetInfo info) =>
            info?.m_netAI is RoadBaseAI && !info.GetIsAdaptive();

        internal static bool CalculateCanHideCrossingsRaw(NetInfo info) {
            bool ret = CalculateCanHideMarkingsRaw(info);
            if (!ret)
                return false;

            RoadBaseAI ai = info.m_netAI as RoadBaseAI;
            ret &= info.m_hasPedestrianLanes || !ai.m_highwayRules || info.name.ToLower().Contains("toll");

            ret = ret && !exempts_.Contains(info?.name);
            ret = ret && !info.IsExemptedNExt();
            return ret;
        }



    }
}
