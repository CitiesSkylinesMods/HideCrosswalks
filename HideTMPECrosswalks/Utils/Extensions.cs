using ColossalFramework;
using ICities;
using System.Reflection;

namespace HideCrosswalks.Utils {

    public static class Extensions {
        internal static NetNode[] NodeBuffer { get; private set; }
        internal static NetSegment[] SegmentBuffer { get; private set; }
        internal static NetLane[] LaneBuffer { get; private set; }
        private static bool InGameFast { get; set; }

        public static void Init() {
            NodeBuffer = Singleton<NetManager>.instance.m_nodes.m_buffer;
            SegmentBuffer = Singleton<NetManager>.instance.m_segments.m_buffer;
            LaneBuffer = Singleton<NetManager>.instance.m_lanes.m_buffer;
            InGameFast = InGame;
        }

        internal static ref NetNode ToNode(this ushort id) => ref Singleton<NetManager>.instance.m_nodes.m_buffer[id];
        internal static ref NetSegment ToSegment(this ushort id) => ref Singleton<NetManager>.instance.m_segments.m_buffer[id];
        internal static NetLane ToLane(this uint id) => Singleton<NetManager>.instance.m_lanes.m_buffer[id];

        internal static AppMode currentMode => SimulationManager.instance.m_ManagersWrapper.loading.currentMode;
        internal static bool CheckGameMode(AppMode mode) {
            try {
                if (currentMode == mode)
                    return true;
            } catch { }
            return false;
        }
        internal static bool InGame => CheckGameMode(AppMode.Game);
        internal static bool InAssetEditor => CheckGameMode(AppMode.AssetEditor);
        internal static bool IsActive =>
#if DEBUG
            InGame || InAssetEditor;
#else
            InGame;
#endif

        internal static bool IsActiveFast =>
#if DEBUG
            InGameFast || InAssetEditor;
#else
            InGameFast;
#endif



        internal static void Assert(bool con, string m="") {
            if (!con) {
                m = "Assertion failed: " + m;
                Log.Error(m);
                throw new System.Exception(m);
            }
        }

        internal static string BIG(string m) {
            string mul(string s, int i) {
                string ret_ = "";
                while (i-- > 0) ret_ += s;
                return ret_;
            }
            m = "  " + m + "  ";
            int n = 120;
            string stars1 = mul("*", n);
            string stars2 = mul("*", (n - m.Length) / 2);
            string ret = stars1 + "\n" + stars2 + m + stars2 + "\n" + stars1;
            return ret;
        }

        internal static string GetPrettyFunctionName(MethodInfo m) {
            string s = m.Name;
            string[] ss = s.Split(new[] { "g__", "|" }, System.StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length == 3)
                return ss[1];
            return s;
        }

    }
}
