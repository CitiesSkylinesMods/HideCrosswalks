using ColossalFramework;

namespace HideTMPECrosswalks.Utils {
    public static class Extensions {
        //internal static ref NetNode Node(ushort ID) => ref Singleton<NetManager>.instance.m_nodes.m_buffer[ID];
        //internal static ref NetSegment Segment(ushort ID) => ref Singleton<NetManager>.instance.m_segments.m_buffer[ID];

        internal static ref NetNode ToNode(this ushort id) => ref Singleton<NetManager>.instance.m_nodes.m_buffer[id];
        internal static ref NetSegment ToSegment(this ushort id ) => ref Singleton<NetManager>.instance.m_segments.m_buffer[id];
    }
}
