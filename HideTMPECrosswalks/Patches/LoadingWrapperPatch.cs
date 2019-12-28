using Harmony;

namespace HideTMPECrosswalks.Patches {
    [HarmonyPatch(typeof(LoadingWrapper), "OnLevelLoaded")]
    public static class LoadingWrapperPatch {
        public delegate void Handler();
        public static event Handler OnPostLevelLoaded;
        public static void Postfix() => OnPostLevelLoaded.Invoke();
    }
}
