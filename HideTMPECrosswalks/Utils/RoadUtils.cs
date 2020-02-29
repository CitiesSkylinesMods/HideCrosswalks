using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace HideCrosswalks.Utils {
    using static PrefabUtils;
    public static class RoadUtils {
        internal static bool IsNormalRoad(this NetInfo info) {
            try {
                bool ret = info?.m_netAI is RoadBaseAI;
                string name = info.name;
                ret &= name != null;
                ret &= name.Trim() != "";
                ret &= !name.ToLower().Contains("toll");
                return ret;
            }
            catch (Exception e) {
                Log.Info(e.Message);
                Log.Info("IsNormalRoad catched exception");
                Log.Info($"exception: info = {info}");
                Log.Info($"exception: info is {info.GetType()}");
                Log.Info($"Exception: name = {info?.name} ");
                return false;
            }
        }

        internal static bool IsNormalGroundRoad(this NetInfo info) {
            bool ret = info.IsNormalRoad();
            if (ret && info?.m_netAI is RoadAI) {
                var ai = info.m_netAI as RoadAI;
                return ai.m_elevatedInfo != null && ai.m_slopeInfo != null;
            }
            return false;
        }

        public static string[] GetRoadNames() {
            List<string> ret = new List<string>();
            foreach(NetInfo info in Networks()) { 
                if (IsNormalGroundRoad(info)) {
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
            Extensions.Assert(info.IsNormalGroundRoad(), $"IsNormalGroundRoad(info={info.name})");

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

        // TODO make array.
        private static Hashtable Never_Table;
        //private static bool[] Never_array;

        public static void CacheNever(IEnumerable<string> roads) {
            int count = PrefabCollection<NetInfo>.LoadedCount();
            Never_Table = new Hashtable(count * 10);
            for (uint i = 0; i < count; ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                if (IsNormalGroundRoad(info)) {
                    string name = GetRoadTitle(info);
                    bool b = (bool)roads.Contains(name);
                    RoadAI ai = info.m_netAI as RoadAI;
                    Never_Table[i] = b;
                    Never_Table[ai.m_slopeInfo.m_prefabDataIndex] = b;
                    Never_Table[ai.m_elevatedInfo.m_prefabDataIndex] = b;
                    Never_Table[ai.m_bridgeInfo.m_prefabDataIndex] = b;
                    Never_Table[ai.m_tunnelInfo.m_prefabDataIndex] = b;
                }
            }
        }

        public static bool NeverZebra(NetInfo info) {
            try { return (bool)Never_Table[info.m_prefabDataIndex]; }
            catch {
                Log.Info($"NeverZebra:Never_array[{info.m_prefabDataIndex}] index out of range. info:{info.name}");
                return false;
            }
        }

    }
}
