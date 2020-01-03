using Harmony;
using ICities;
using JetBrains.Annotations;
using UnityEngine;
using HideTMPECrosswalks.Utils;
using HideTMPECrosswalks.Patches;
using HideTMPECrosswalks.Settings;
using System.Collections.Generic;

namespace HideTMPECrosswalks {
    public class KianModInfo : IUserMod {
        public string Name => "RM TLM Crossings V2.4";
        public string Description => "Automatically hide crosswalk textures on segment ends when TMPE bans crosswalks";

        [UsedImplicitly]
        public void OnEnabled() {
            System.IO.File.WriteAllText("mod.debug.log", ""); // restart log.
            InstallHarmony();

            LoadingWrapperPatch.OnPostLevelLoaded += PrefabUtils.CachePrefabs;
#if DEBUG
            LoadingWrapperPatch.OnPostLevelLoaded += TestOnLoad.Test;
#endif
            LoadingManager.instance.m_levelUnloaded += PrefabUtils.ClearALLCache;
            try {
                AppMode mode = Extensions.currentMode;
                if (mode == AppMode.Game || mode == AppMode.AssetEditor) {
                    PrefabUtils.CachePrefabs();
                }
            }
            catch { }
        }

        [UsedImplicitly]
        public void OnDisabled() {
            UninstallHarmony();
            PrefabUtils.ClearALLCache();
            LoadingWrapperPatch.OnPostLevelLoaded -= PrefabUtils.CachePrefabs;
#if DEBUG
            LoadingWrapperPatch.OnPostLevelLoaded -= TestOnLoad.Test;
#endif
            LoadingManager.instance.m_levelUnloaded -= PrefabUtils.ClearALLCache;
            Options.instance = null;
        }


        //[UsedImplicitly]
        //public void OnSettingsUI(UIHelperBase helperBasae) {
        //    new Options(helperBasae);
        //}

        #region Harmony
        HarmonyInstance harmony = null;
        const string HarmonyId = "CS.kian.HideTMPECrosswalks";
        void InstallHarmony() {
            if (harmony == null) {
                Extensions.Log("HideTMPECrosswalks Patching...",true);
#if DEBUG
                HarmonyInstance.DEBUG = true;
#endif
                HarmonyInstance.SELF_PATCHING = false;
                harmony = HarmonyInstance.Create(HarmonyId);
                harmony.PatchAll(GetType().Assembly);
            }
        }

        void UninstallHarmony() {
            if (harmony != null) {
                harmony.UnpatchAll(HarmonyId);
                harmony = null;
                Extensions.Log("HideTMPECrosswalks patches Reverted.",true);
            }
        }
        #endregion
    }

#if DEBUG
    public class TestOnLoad : LoadingExtensionBase {
        public override void OnCreated(ILoading loading) { base.OnCreated(loading) ;Test(); }
        public override void OnLevelLoaded(LoadMode mode) => Test();

        public static void Test() {
            if (!Extensions.InGame && !Extensions.InAssetEditor)
                return;

            //Extensions.Log("Testing ...");
            //PrefabUtils.DebugTests.NameTest();
            //PrefabUtils.DebugTests.Dumps();
            //Extensions.Log("Testing Done!");
        }

    }
#endif

}
