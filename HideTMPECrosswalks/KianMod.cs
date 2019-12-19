using Harmony;
using ICities;
using JetBrains.Annotations;
using System;
using System.Reflection;
using TrafficManager;
using UnityEngine;
using HideTMPECrosswalks.Utils;
using HideTMPECrosswalks.Patches;

namespace HideTMPECrosswalks {
    public class KianModInfo : IUserMod {
        public string Name => "Hide TMPE crosswalks V2.1 [ALPHA]";
        public string Description => "Automatically hide crosswalk textures on segment ends when TMPE bans crosswalks";

        [UsedImplicitly]
        public void OnEnabled() {
            System.IO.File.WriteAllText("mod.debug.log", ""); // restart log.
            InstallHarmony();
            LoadingWrapperPatch.OnPostLevelLoaded += NetNode_RenderInstance.CacheAll;
            LoadingManager.instance.m_levelUnloaded += NetNode_RenderInstance.ClearCache;
        }

        [UsedImplicitly]
        public void OnDisabled() {
            UninstallHarmony();

            NetNode_RenderInstance.ClearCache();
        }

        #region Harmony
        HarmonyInstance harmony = null;
        const string HarmonyId = "CS.kian.HideTMPECrosswalks";
        void InstallHarmony() {
            if (harmony == null) {
                Debug.Log("HideTMPECrosswalks Patching...");
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
                Debug.Log("HideTMPECrosswalks Reverted...");
            }
        }
        #endregion

    }
}
