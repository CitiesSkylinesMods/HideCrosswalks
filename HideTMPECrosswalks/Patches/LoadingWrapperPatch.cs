using HarmonyLib;
using System;
using KianCommons;

namespace HideCrosswalks.Patches {
    [HarmonyPatch(typeof(LoadingWrapper), "OnLevelLoaded")]
    public static class LoadingWrapperPatch {
        public delegate void Handler();
        public static event Handler OnPostLevelLoaded;
        public static void Postfix() => OnPostLevelLoaded.Invoke();
        public static void Finalizer(Exception __exception) {
            if (__exception == null) return;
            Log.Exception(__exception);
        }
    }
}
