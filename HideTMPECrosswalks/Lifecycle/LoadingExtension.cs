namespace HideCrosswalks.Lifecycle {
    using HideCrosswalks.Patches;
    using HideCrosswalks.Utils;
    using ICities;
    using KianCommons;
    using KianCommons.Plugins;
    using System;

    public class LoadingExtension : LoadingExtensionBase {
        public const string HARMONY_ID = "CS.Kian.HideCrosswalks";

        public override void OnLevelLoaded(LoadMode mode) => Load();
        public override void OnLevelUnloading() {
            base.OnLevelUnloading();
            Log.Called();
        }

        public static void Preload() {
            try {
                Log.Called();
                PluginUtil.LogPlugins();
                HarmonyUtil.InstallHarmony(HARMONY_ID, null, null);
                LoadingWrapperPatch.OnPostLevelLoaded -= Postload;
                LoadingWrapperPatch.OnPostLevelLoaded += Postload;
                Log.Succeeded();
            } catch (Exception ex) { ex.Log(); }
        }

        public static void Load() {
            Log.Called();
            TMPEUTILS.Init();
            NS2Utils.Init();
            NetInfoExt.InitNetInfoExtArray();
            Log.Succeeded();
#if DEBUG
            TestOnLoad.Test();
#endif
        }

        public static bool Loaded { get; private set; }
        public static void Postload() {
            try {
                Log.Called();
                LoadingWrapperPatch.OnPostLevelLoaded -= Postload; // prevent double load
                PrefabUtils.CachePrefabs();
                Loaded = true;
                Log.Succeeded();
            } catch (Exception ex) { ex.Log(); }
        }

        public static void HotReload() {
            try {
                Log.Called();
                Preload();
                Load();
                Postload();
                Log.Succeeded();
            } catch (Exception ex) { ex.Log(); }

        }

        public static void Unload() {
            try {
                Log.Called();
                Loaded = false;
                HarmonyUtil.UninstallHarmony(HARMONY_ID);
                PrefabUtils.ClearCache();
                NetInfoExt.NetInfoExtArray = null;
                Log.Succeeded();
            } catch (Exception ex) { ex.Log(); }

        }
    }
}

