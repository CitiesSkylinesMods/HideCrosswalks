using Harmony;
using HideCrosswalks.Utils;

namespace HideCrosswalks.Patches
{
    public class HarmonyExtension
    {
        HarmonyInstance harmony;
        public const string HARMONY_ID = "CS.Kian.HideCrosswalks";

        public void InstallHarmony()
        {
#if !DEBUG
            // TODO: this does not work because Before OnCreate we don't know if we are in asset editor.
            if (Extensions.InAssetEditor) {
                Log.Info("skipped InstallHarmony in asset editor release build");
                return;
            }
#endif
            if (harmony == null)
            {
                Log.Info("HideCrosswalks Patching...");
#if DEBUG
                HarmonyInstance.DEBUG = true;
#endif
                HarmonyInstance.SELF_PATCHING = false;
                harmony = HarmonyInstance.Create(HARMONY_ID);
                harmony.PatchAll(GetType().Assembly);
            }
            Log.Info("HideCrosswalks: All patches were successfull.");
        }

        public void UninstallHarmony()
        {
            if (harmony != null)
            {
                harmony.UnpatchAll(HARMONY_ID);
                harmony = null;
                Log.Info("HideCrosswalks patches Reverted.");
            }
        }
    }
}