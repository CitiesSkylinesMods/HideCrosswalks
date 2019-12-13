using Harmony;
using ICities;
using System;
using System.Reflection;
using UnityEngine;

namespace HideTMPECrosswalks
{
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
            Debug.Log("OnCreate");
            base.OnCreated(loading);
            if (Utils.TMPEUTILS.Init()) {
                Patch.NetNode_RenderInstance.Init();
                Harmony = HarmonyInstance.Create("CS.kian.HideTMPECrosswalks"); // would creating 2 times cause an issue?
                Debug.Log("Harmony="+ Harmony);
                Harmony?.PatchAll(Assembly.GetExecutingAssembly());
            }
        }

        public override void OnReleased() {
            Harmony?.UnpatchAll(); // TODO Test: Disableing a mod will call this. Does it cause an issue?
            Harmony = null;
        }
        #endregion
    }
}
