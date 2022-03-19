namespace HideCrosswalks.Lifecycle {
    using HideCrosswalks.Patches;
    using HideCrosswalks.Utils;
    using ICities;
    using KianCommons;

    public class LoadingExtension : LoadingExtensionBase {
        public const string HARMONY_ID = "CS.Kian.HideCrosswalks";

        public override void OnLevelLoaded(LoadMode mode) => Load();
        public override void OnLevelUnloading() {
            base.OnLevelUnloading();
        }

        public static void Preload() {
            HarmonyUtil.InstallHarmony(HARMONY_ID, null, null);
            LoadingWrapperPatch.OnPostLevelLoaded -= Postload;
            LoadingWrapperPatch.OnPostLevelLoaded += Postload;
        }

        public static void Load() {
            TMPEUTILS.Init();
            NS2Utils.Init();
            NetInfoExt.InitNetInfoExtArray();
#if DEBUG
            TestOnLoad.Test();
#endif
        }

        public static bool Loaded { get; private set; }
        public static void Postload() {
            LoadingWrapperPatch.OnPostLevelLoaded -= Postload; // prevent double load
            PrefabUtils.CachePrefabs();
            Loaded = true;
        }

        public static void HotReload() {
            Preload();
            Load();
            Postload();
        }

        public static void Unload() {
            Loaded = false;
            HarmonyUtil.UninstallHarmony(HARMONY_ID);
            PrefabUtils.ClearCache();
            NetInfoExt.NetInfoExtArray = null;
        }
    }
}

