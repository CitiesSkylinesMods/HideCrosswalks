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

        internal ushort Index;
        internal bool TMPE_Exempt; //can't hide using TMPE but still possible using NS2
        internal bool CanHideCrossings;

        public NetInfo Info {
            get => PrefabCollection<NetInfo>.GetPrefab(Index);
            set => Index = (ushort)value.m_prefabDataIndex;
        }

        // TODO provide list of harcoded roads. (dirt road, next tunnel asym, ...)
        private static List<string> exempts_ = new List<string>(new[]{
            "",
        });

        internal void UpdateInfo() {
            NetInfo info = Info;
            TMPE_Exempt = RoadUtils.IsExempt(info);
            TMPE_Exempt |= exempts_.Contains(info.name);

            // roads without pedesterian lanes (eg highways) have no crossings to hide to the best of my knowledege.
            // not sure about custom highways. Processing texture for such roads may reduce smoothness of the transition.
            CanHideCrossings =
                !TMPE_Exempt && info.m_hasPedestrianLanes && info.m_hasForwardVehicleLanes;
        }

        private static bool CanHideMarkings(NetInfo info) => (info.m_netAI is RoadBaseAI) & HideCrosswalksMod.IsEnabled;

        internal static NetInfoExt[] NetInfoExtArray;
        internal static void InitNetInfoExtArray() {
            int count = PrefabCollection<NetInfo>.PrefabCount();
            int loadedCount = PrefabCollection<NetInfo>.LoadedCount();
            NetInfoExtArray = new NetInfoExt[count];
            for (uint i = 0; i < loadedCount; ++i) {
                NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                if (CanHideMarkings(info)) {
                    ushort index = (ushort)info.m_prefabDataIndex;
                    NetInfoExtArray[index] = new NetInfoExt(index);
                }
            } // end for
        } // end method
    } // end class
} // end namespace