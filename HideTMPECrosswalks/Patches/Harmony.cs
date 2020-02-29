using Harmony;
using ICities;
using JetBrains.Annotations;
using UnityEngine;
using HideCrosswalks.Utils;
using HideCrosswalks.Patches;
using HideCrosswalks.Settings;
using System.Collections.Generic;

namespace HideCrosswalks.Patches
{
    public class HarmonyExtension
    {
        HarmonyInstance harmony;
        const string HarmonyId = "CS.Kian.HideCrosswalks";

        public  void InstallHarmony()
        {
            if (harmony == null)
            {
                Extensions.Log("HideCrosswalks Patching...", true);
#if DEBUG
                HarmonyInstance.DEBUG = true;
#endif
                HarmonyInstance.SELF_PATCHING = false;
                harmony = HarmonyInstance.Create(HarmonyId);
                harmony.PatchAll(GetType().Assembly);
            }
        }

        public void UninstallHarmony()
        {
            if (harmony != null)
            {
                harmony.UnpatchAll(HarmonyId);
                harmony = null;
                Extensions.Log("HideCrosswalks patches Reverted.", true);
            }
        }
    }
}