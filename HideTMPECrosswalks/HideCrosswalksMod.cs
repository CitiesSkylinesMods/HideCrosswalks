using ICities;
using JetBrains.Annotations;
using HideCrosswalks.Utils;
using HideCrosswalks.Patches;

namespace HideCrosswalks {
    public class HideCrosswalksMod : IUserMod {
        public string Name => "RM Crossings V3.0";
        public string Description => "Hide Crosswalks when TMPE bans them or when NS2 removes them.";
        private static bool _isEnabled = false;
        public static bool IsEnabled => _isEnabled;
        internal static bool IsEnabledInternal => _isEnabled;

        [UsedImplicitly]
        public void OnEnabled() {
            _isEnabled = true;
            Extensions.ClearLog();
            LoadingWrapperPatch.OnPostLevelLoaded += PrefabUtils.CachePrefabs;
#if DEBUG
            LoadingWrapperPatch.OnPostLevelLoaded += TestOnLoad.Test;
#endif
            LoadingManager.instance.m_levelUnloaded += MaterialUtils.Clear;

            if (Extensions.InGame) {
                PrefabUtils.CachePrefabs();
            }
        }

        [UsedImplicitly]
        public void OnDisabled() {
            if (Extensions.InGame || Extensions.InAssetEditor) {
                LoadingExtension.Instance.OnLevelUnloading();
                LoadingExtension.Instance.OnReleased();
            }

            _isEnabled = false;
            PrefabUtils.ClearCache();

            LoadingWrapperPatch.OnPostLevelLoaded -= PrefabUtils.CachePrefabs;
#if DEBUG
            LoadingWrapperPatch.OnPostLevelLoaded -= TestOnLoad.Test;
#endif
            LoadingManager.instance.m_levelUnloaded -= PrefabUtils.ClearCache;
            //Options.instance = null;
        }

        //[UsedImplicitly]
        //public void OnSettingsUI(UIHelperBase helperBasae) {
        //    new Options(helperBasae);
        //}
    }

    public class LoadingExtension : LoadingExtensionBase {
        internal static LoadingExtension Instance;
        HarmonyExtension harmonyExt;
        public override void OnCreated(ILoading loading) {
            Instance = this;
            base.OnCreated(loading);
            harmonyExt = new HarmonyExtension();
            if (Extensions.InGame) {
                OnLevelLoaded(LoadMode.NewGame);
            } else if (Extensions.InAssetEditor) {
                OnLevelLoaded(LoadMode.NewAsset);
            }
        }
        public override void OnReleased() {
            harmonyExt = null;
            base.OnReleased();
        }
        public override void OnLevelLoaded(LoadMode mode) {
#if !DEBUG
            if (Extensions.InAssetEditor) {
                Extensions.Log("skipped InstallHarmony in asset editor release build");
                return;
            }
#endif

            harmonyExt.InstallHarmony();
        }
        public override void OnLevelUnloading() {
            harmonyExt?.UninstallHarmony();
        }
    }
}
