using ColossalFramework;

namespace HideTMPECrosswalks.Utils {

    public static class Extensions {
        internal static ref NetNode ToNode(this ushort id) => ref Singleton<NetManager>.instance.m_nodes.m_buffer[id];
        internal static ref NetSegment ToSegment(this ushort id ) => ref Singleton<NetManager>.instance.m_segments.m_buffer[id];
        internal static void Log(string m) {
            var st = System.Environment.StackTrace;
            //m  = st + " : \n" + m;
            UnityEngine.Debug.Log(m);
            System.IO.File.AppendAllText("mod.debug.log", m+"\n\n");
        }
    }
}
