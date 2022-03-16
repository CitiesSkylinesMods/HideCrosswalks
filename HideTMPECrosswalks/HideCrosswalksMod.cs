using ICities;
using JetBrains.Annotations;
using HideCrosswalks.Utils; using KianCommons;
using HideCrosswalks.Patches;
using HideCrosswalks.Settings;
using System;

namespace HideCrosswalks {
    public class HideCrosswalksMod : IUserMod  {
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

            
            if (HelpersExtensions.InGameOrEditor) {
                LoadingExtension.Load();
            }

            CitiesHarmony.API.HarmonyHelper.EnsureHarmonyInstalled();
        }

        [UsedImplicitly]
        public void OnDisabled() {
            Log.Info("OnDisabled() called Name:" + Name);

            _isEnabled = false;

            if(HelpersExtensions.InGameOrEditor) {
                LoadingExtension.Unload();
            }

            Options.instance = null;
        }

        [UsedImplicitly]
        public void OnSettingsUI(UIHelperBase helperBasae) {
            new Options(helperBasae);
        }
    }

    public class LoadingExtension : LoadingExtensionBase {
        public const string HARMONY_ID = "CS.Kian.HideCrosswalks";

        public override void OnLevelLoaded(LoadMode mode) => Load();
        public override void OnLevelUnloading() {
            base.OnLevelUnloading();
        }


        public static void Load() {
            HarmonyUtil.InstallHarmony(HARMONY_ID, null, null);
            PrefabUtils.CachePrefabs();
#if DEBUG
            TestOnLoad.Test();
#endif
        }
        
        public static void Unload() {
            HarmonyUtil.UninstallHarmony(HARMONY_ID);
            PrefabUtils.ClearCache();
        }
    }
}

