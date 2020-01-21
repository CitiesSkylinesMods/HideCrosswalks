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
        public string Name => "RM Crossings V2.5";
        public string Description => "Hide Crosswalks when TMPE bans them or when NS2 removes them.";

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

#endif

}
