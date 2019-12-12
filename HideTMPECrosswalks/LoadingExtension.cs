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
            if (Utils.TMPEUTILS.Init()) {
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
