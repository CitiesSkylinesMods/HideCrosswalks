using ICities;
using HideTMPECrosswalks.Patch;

namespace HideTMPECrosswalks {
    public class LoadingExtention : LoadingExtensionBase {
        public override void OnLevelUnloading() {
            base.OnLevelUnloading();
            Hook.UnHookAll();
        }
        public override void OnCreated(ILoading loading) {
            base.OnCreated(loading);
            Utils.TMPEUTILS.Init();
            if (Utils.TMPEUTILS.tmpeDetected) {
                Hook.Create();
                Hook.HookAll();
            }
        }

        public override void OnReleased() {
            Hook.Release();
            base.OnReleased();
        }
    }
}
