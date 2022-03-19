using HarmonyLib;
using System;
using KianCommons;

namespace HideCrosswalks.Patches {
    [HarmonyPatch(typeof(LoadingWrapper), "OnLevelLoaded")]
    public static class LoadingWrapperPatch {
        public static event Action OnPostLevelLoaded;
        public static void Finalizer(Exception __exception) {
            // use finalizer instead of postfix to ensure failure of other mods does not prevent this mod from loading.
            __exception?.Log();
            OnPostLevelLoaded();
        }
    }
}
