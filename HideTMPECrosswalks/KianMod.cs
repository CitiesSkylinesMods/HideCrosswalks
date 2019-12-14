using Harmony;
using ICities;
using System;
using System.Reflection;
using UnityEngine;

namespace HideTMPECrosswalks {
    public class KianModInfo : LoadingExtensionBase, IUserMod {
        #region IUserMod
        public string Name => "Hide TMPE crosswalks";
        public string Description => "Automatically hide crosswalk textures on segment ends when TMPE bans crosswalks";
        public void OnEnabled() {
        }
        public void OnDisabled() {
            OnReleased();
        }
        #endregion

        #region LoadingExtension

        HarmonyInstance Harmony = null;
        public override void OnCreated(ILoading loading) {
            base.OnCreated(loading);
            InstallHarmony();
        }

        public override void OnReleased() {
            UninstallHarmony();
        }

        private void InstallHarmony() {
            Harmony = HarmonyInstance.Create("CS.kian.HideTMPECrosswalks"); // would creating 2 times cause an issue?
            Harmony?.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void UninstallHarmony() {
            Harmony?.UnpatchAll();
            Harmony = null;
        }

        #endregion
    }
}
