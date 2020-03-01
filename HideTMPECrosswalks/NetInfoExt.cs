namespace HideCrosswalks {
    using System.Collections.Generic;
    using Utils;

    public class NetInfoExt {
        public NetInfoExt(ushort index) {
            Index = index;
            UpdateInfo();
        }
        public NetInfoExt(NetInfo info) {
            Info = info;
            UpdateInfo();
        }

        internal ushort Index { get; private set; }
        internal bool CanHideCrossings { get; private set; }

        public NetInfo Info {
            get => PrefabCollection<NetInfo>.GetPrefab(Index);
            set => Index = (ushort)value.m_prefabDataIndex;
        }

        internal void UpdateInfo() {
            NetInfo info = Info;
            CanHideCrossings = RoadUtils.CalculateCanHideCrossingsRaw(info) && !RoadUtils.IsExempt(info);
        }

        #region static
        internal static NetInfoExt[] NetInfoExtArray;
        internal static void InitNetInfoExtArray() {
            int count = PrefabCollection<NetInfo>.PrefabCount();
            int loadedCount = PrefabCollection<NetInfo>.LoadedCount();
            NetInfoExtArray = new NetInfoExt[count];
            for (uint i = 0; i < loadedCount; ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                if (RoadUtils.CalculateCanHideMarkingsRaw(info)) {
                    ushort index = (ushort)info.m_prefabDataIndex;
                    NetInfoExtArray[index] = new NetInfoExt(index);
                }
            } // end for
        } // end method

        public static bool GetCanHideCrossings(NetInfo info) {
            return GetCanHideMarkings(info) && NetInfoExtArray[info.m_prefabDataIndex].CanHideCrossings;
        }

        public static bool GetCanHideMarkings(NetInfo info) {
            return HideCrosswalksMod.IsEnabled && NetInfoExtArray?[info.m_prefabDataIndex] != null;
        } // end method
        #endregion
    } // end class
} // end namespace