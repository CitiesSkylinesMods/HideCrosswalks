using HarmonyLib;
using HideCrosswalks.Utils;

namespace HideCrosswalks.Patches
{
    public class HarmonyExtension
    {
        //Harmony harmony;
        bool installed = false;
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
            if (!installed)
            {
                Log.Info("HideCrosswalks Patching...");
#if DEBUG
                Harmony.DEBUG = true;
#endif
                Harmony harmony = new Harmony(HARMONY_ID);
                harmony.PatchAll(GetType().Assembly);
                installed = true;
            }
        }

        public void UninstallHarmony()
        {
            if (installed)
            {
                Harmony harmony = new Harmony(HARMONY_ID);
                harmony.UnpatchAll(HARMONY_ID);
                harmony = null;
                Log.Info("HideCrosswalks patches Reverted.");
            }
        }
    }
}