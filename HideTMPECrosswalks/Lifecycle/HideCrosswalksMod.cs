namespace HideCrosswalks.Lifecycle {
    using ICities;
    using JetBrains.Annotations;
    using KianCommons;
    using System;

    public class HideCrosswalksMod : IUserMod {
        public string Name => "RM Crossings " + VersionString + " " + BRANCH;
        public string Description => "Hide Crosswalks when TMPE bans them or when NS2 removes them.";
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

            LoadingManager.instance.m_levelPreLoaded -= LoadingExtension.Preload;
            LoadingManager.instance.m_levelPreLoaded += LoadingExtension.Preload;

            if (!Helpers.InStartupMenu) {
                LoadingExtension.HotReload();
            }

            CitiesHarmony.API.HarmonyHelper.EnsureHarmonyInstalled();
        }

        [UsedImplicitly]
        public void OnDisabled() {
            Log.Info("OnDisabled() called Name:" + Name);
            LoadingManager.instance.m_levelPreLoaded -= LoadingExtension.Preload;

            if (!Helpers.InStartupMenu) {
                LoadingExtension.Unload();
            }

            Options.instance = null;
        }

        [UsedImplicitly]
        public void OnSettingsUI(UIHelperBase helperBasae) {
            new Options(helperBasae);
        }
    }
}

