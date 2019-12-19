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
        HarmonyInstance Harmony = null;
        string HarmonyID = "CS.kian.HideTMPECrosswalks";

        public string Name => "Hide TMPE crosswalks V2.1 [ALPHA]";
        public string Description => "Automatically hide crosswalk textures on segment ends when TMPE bans crosswalks";
        public void OnEnabled() {
            System.IO.File.WriteAllText("mod.debug.log", ""); // restart log.
            HarmonyInstance.DEBUG = true;
            Harmony = HarmonyInstance.Create(HarmonyID); // would creating 2 times cause an issue?
            Harmony?.PatchAll(Assembly.GetExecutingAssembly());

            LoadingWrapperPatch.OnPostLevelLoaded += NetNode_RenderInstance.CacheAll;
            LoadingManager.instance.m_levelUnloaded += NetNode_RenderInstance.ClearCache;
        }

        [UsedImplicitly]
        public void OnDisabled() {
            Harmony?.UnpatchAll(HarmonyID);
            Harmony = null;
            NetNode_RenderInstance.ClearCache();
        }
    }
}
