using ColossalFramework;

namespace HideTMPECrosswalks.Utils {
    public static class Extensions {
        internal static ref NetNode Node(ushort ID) => ref Singleton<NetManager>.instance.m_nodes.m_buffer[ID];
        internal static ref NetSegment Segment(ushort ID) => ref Singleton<NetManager>.instance.m_segments.m_buffer[ID];
    }
}
