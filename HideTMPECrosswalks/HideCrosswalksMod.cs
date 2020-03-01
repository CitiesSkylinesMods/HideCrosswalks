using ICities;
using JetBrains.Annotations;
using HideCrosswalks.Utils;
using HideCrosswalks.Patches;
using HideCrosswalks.Settings;

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

            LoadingWrapperPatch.OnPostLevelLoaded += PrefabUtils.CachePrefabs;
#if DEBUG
            LoadingWrapperPatch.OnPostLevelLoaded += TestOnLoad.Test;
#endif
            LoadingManager.instance.m_levelUnloaded += PrefabUtils.ClearCache;

            if (Extensions.InGame) {
                PrefabUtils.CachePrefabs();
            }
        }

        [UsedImplicitly]
        public void OnDisabled() {
            _isEnabled = false;

            if(Extensions.InGame | Extensions.InAssetEditor) {
                LoadingExtension.Instance.OnReleased();
            }

            PrefabUtils.ClearCache();
            LoadingWrapperPatch.OnPostLevelLoaded -= PrefabUtils.CachePrefabs;
            LoadingManager.instance.m_levelUnloaded -= PrefabUtils.ClearCache;

#if DEBUG
            LoadingWrapperPatch.OnPostLevelLoaded -= TestOnLoad.Test;
#endif

            Options.instance = null;
        }

        [UsedImplicitly]
        public void OnSettingsUI(UIHelperBase helperBasae) {
            new Options(helperBasae);
        }
    }

    public class LoadingExtension : LoadingExtensionBase {
        internal static LoadingExtension Instance;
        HarmonyExtension harmonyExt;
        public override void OnCreated(ILoading loading) {
            base.OnCreated(loading);
            Instance = this;
            harmonyExt = new HarmonyExtension();
            harmonyExt.InstallHarmony();
        }
        public override void OnReleased() {
            harmonyExt?.UninstallHarmony();
            harmonyExt = null;
            base.OnReleased();
        }
    }
}

