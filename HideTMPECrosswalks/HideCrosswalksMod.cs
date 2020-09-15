using ICities;
using JetBrains.Annotations;
using HideCrosswalks.Utils; using KianCommons;
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
            Log.Info("OnEnabled() called Name:" + Name);
            _isEnabled = true;

            LoadingWrapperPatch.OnPostLevelLoaded += PrefabUtils.CachePrefabs;
#if DEBUG
            LoadingWrapperPatch.OnPostLevelLoaded += TestOnLoad.Test;
#endif
            LoadingManager.instance.m_levelUnloaded += PrefabUtils.ClearCache;

            if (HelpersExtensions.InGameOrEditor) {
                LoadingWrapperPatch.Postfix();
            }

            CitiesHarmony.API.HarmonyHelper.EnsureHarmonyInstalled();
        }

        [UsedImplicitly]
        public void OnDisabled() {
            Log.Info("OnDisabled() called Name:" + Name);

            _isEnabled = false;

            if(HelpersExtensions.InGameOrEditor) {
                HarmonyUtil.UninstallHarmony(LoadingExtension.HARMONY_ID);
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
        public const string HARMONY_ID = "CS.Kian.HideCrosswalks";

        public override void OnCreated(ILoading loading) =>
            HarmonyUtil.InstallHarmony(HARMONY_ID);

        public override void OnReleased() =>
            HarmonyUtil.UninstallHarmony(HARMONY_ID);
    }
}

