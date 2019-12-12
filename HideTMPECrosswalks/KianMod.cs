using ICities;
using HideTMPECrosswalks.Patch;
using ColossalFramework;

namespace HideTMPECrosswalks
{
    public class KianModInfo : LoadingExtensionBase, IUserMod {
        #region IUserMod
        public string Name => "Hide TMPE crosswalks";
        public string Description => "Automatically hide crosswalk textures on segment ends when TMPE bans crosswalks";
        public void OnDisabled() => OnReleased();
        #endregion

        #region LoadingExtension
        public override void OnCreated(ILoading loading) {
            base.OnCreated(loading);
            if (Utils.TMPEUTILS.Init()) {
                Hook.Create();
                Hook.HookAll();
            }
        }

        public override void OnReleased() {
            Hook.Release();
        }
        #endregion
    }
}
