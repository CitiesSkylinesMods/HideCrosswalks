using ICities;
using JetBrains.Annotations;
using HideCrosswalks.Utils;
using HideCrosswalks.Patches;
using HideCrosswalks.Settings;
using System;

namespace HideCrosswalks {
    public class HideCrosswalksMod : IUserMod {
        public string Name => "RM Crossings " + VersionString + " " + BRANCH;
        public string Description => "Hide Crosswalks when TMPE bans them or when NS2 removes them.";
        private static bool _isEnabled = false;
        internal static bool IsEnabled => _isEnabled;

#if DEBUG
        public const string BRANCH = "DEBUG";
#else
        public const string BRANCH = "";
#endif

        public static Version ModVersion => typeof(HideCrosswalksMod).Assembly.GetName().Version;

        // used for in-game display
        public static string VersionString => ModVersion.ToString(2);

        [UsedImplicitly]
        public void OnEnabled() {
            _isEnabled = true;

            LoadingWrapperPatch.OnPostLevelLoaded += PrefabUtils.CachePrefabs;
#if DEBUG
            LoadingWrapperPatch.OnPostLevelLoaded += TestOnLoad.Test;
#endif
            LoadingManager.instance.m_levelUnloaded += PrefabUtils.ClearCache;

            if (Extensions.InGame) {
                LoadingWrapperPatch.Postfix(); 
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

