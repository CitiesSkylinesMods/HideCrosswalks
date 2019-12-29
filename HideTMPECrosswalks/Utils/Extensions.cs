using ColossalFramework;
using ICities;
using System.Diagnostics;

namespace HideTMPECrosswalks.Utils {

    public static class Extensions {
        internal static ref NetNode ToNode(this ushort id) => ref Singleton<NetManager>.instance.m_nodes.m_buffer[id];
        internal static ref NetSegment ToSegment(this ushort id ) => ref Singleton<NetManager>.instance.m_segments.m_buffer[id];
        //internal static NetLane ToLane(this int id) => Singleton<NetManager>.instance.m_lanes.m_buffer[id];

        public static AppMode currentMode => SimulationManager.instance.m_ManagersWrapper.loading.currentMode;

        internal static void LogLap(this Stopwatch ticks, string prefix = "") {
            ticks.Stop();
            Log(prefix + "TICKS elapsed: " + ticks.ElapsedTicks.ToString("E2"));
            ticks.Reset();
            ticks.Start();
        }
        internal static void Lap(this Stopwatch ticks) {
            ticks.Reset();
            ticks.Start();
        }

        static object LogLock = new object();
        internal static void Log(string m, bool unitylog = false) {
            lock (LogLock) {
                //var st = System.Environment.StackTrace;
                //m  = st + " : \n" + m;
#if DEBUG
                UnityEngine.Debug.Log(m);
#else
                if(unitylog)UnityEngine.Debug.Log(m);
#endif


                System.IO.File.AppendAllText("mod.debug.log", m + "\n\n");
            }
        }
        internal static void Assert(bool con, string m="") {
            if (!con) throw new System.Exception("Assertion failed: " + m);
        }

    }
}
