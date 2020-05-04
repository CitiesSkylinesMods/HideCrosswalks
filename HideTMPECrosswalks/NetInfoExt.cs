namespace HideCrosswalks {
    using System;
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
            get {
                try {
                    return PrefabCollection<NetInfo>.GetPrefab(Index);
                }catch(Exception e) {
                    Log.Info($"Index={Index}, PrefabCount={PrefabCollection<NetInfo>.PrefabCount()}\n" + e);
                    throw e;
                }
            }
            set => Index = (ushort)value.m_prefabDataIndex;
        }

        internal void UpdateInfo() {
            NetInfo info = Info;
            CanHideCrossings = RoadUtils.CalculateCanHideCrossingsRaw(info) && !RoadUtils.IsExempt(info);
        }

        #region static
        internal static NetInfoExt[] NetInfoExtArray;

        internal static void InitNetInfoExtArray() {
            Extensions.Init();
            int prefabCount = PrefabCollection<NetInfo>.PrefabCount();
            int loadedCount = PrefabCollection<NetInfo>.LoadedCount();
            NetInfoExtArray = new NetInfoExt[prefabCount];
            Log.Info($"initializing NetInfoExtArray: prefabCount={prefabCount} LoadedCount={loadedCount}");
            for (uint i = 0; i < loadedCount; ++i) {
                try {
                    NetInfo info = PrefabCollection<NetInfo>.GetLoaded(i);
                    if (info == null) {
                        Log.Warning("Bad prefab with null info");
                        continue;
                    } else if (info.m_netAI == null) {
                        Log.Warning("Bad prefab with null info.m_NetAI");
                        continue;
                    }
                    if (RoadUtils.CalculateCanHideMarkingsRaw(info)) {
                        ushort index = (ushort)info.m_prefabDataIndex;
                        NetInfoExtArray[index] = new NetInfoExt(index);
                    }
                } catch(Exception e) {
                    Log.Error(e.ToString());
                }
            } // end for
            Extensions.Assert(NetInfoExtArray != null, "NetInfoExtArray!=null");
            Log.Info($"NetInfoExtArray initialized");

        } // end method

        public static bool GetCanHideCrossings(NetInfo info) {
            if(info.m_prefabDataIndex>= NetInfoExtArray.Length) {
                Log.Error($"bad prefab index: {info.m_prefabDataIndex} >= {NetInfoExtArray.Length}\n" +
                    $"prefabCount={PrefabCollection<NetInfo>.PrefabCount()} LoadedCount={PrefabCollection<NetInfo>.LoadedCount()}");
                return false;
            }
            return GetCanHideMarkings(info) && NetInfoExtArray[info.m_prefabDataIndex].CanHideCrossings;
        }

        public static bool GetCanHideMarkings(NetInfo info) {
            if (!HideCrosswalksMod.IsEnabled)
                return false;
            Extensions.Assert(NetInfoExtArray != null, "NetInfoExtArray!=null");
            if (info.m_prefabDataIndex >= NetInfoExtArray.Length) {
                Log.Error($"bad prefab index: {info.m_prefabDataIndex} >= {NetInfoExtArray.Length}\n" +
                    $"prefabCount={PrefabCollection<NetInfo>.PrefabCount()} LoadedCount={PrefabCollection<NetInfo>.LoadedCount()}");
                return false;
            }
            return Extensions.IsActiveFast && NetInfoExtArray?[info.m_prefabDataIndex] != null;
        } // end method
        #endregion
    } // end class
} // end namespace