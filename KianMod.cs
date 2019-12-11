using ICities;
using ColossalFramework;
using UnityEngine;

using Kian.HoverTool;
using Kian.Patch;
using System;
using System.Collections.Generic;

namespace Kian.Mod
{
    public class KianModInfo : IUserMod {
        public string Name => "Kian toggle color";
        public string Description => "kian hovering tool that toggles a single segmetns color";
    }

    public class LoadingExtention : LoadingExtensionBase {
        public override void OnLevelLoaded(LoadMode mode) {
            base.OnLevelLoaded(mode);
            Debug.Log("OnLevelLoaded kian mod");
            GameObject toolModControl = ToolsModifierControl.toolController.gameObject;
            //toolModControl.AddComponent<KianTool>();
        }
        public override void OnLevelUnloading() {
            Debug.Log("OnLevelUnloading kian mod");
            base.OnLevelUnloading();
            GameObject toolModControl = ToolsModifierControl.toolController.gameObject;
            //var tool = toolModControl.GetComponent<KianTool>();
            //GameObject.DestroyObject(tool, 10);
            Hook.UnHookAll();
        }
        public override void OnCreated(ILoading loading) {
            Debug.Log("OnCreated kian mod");
            base.OnCreated(loading);
            Hook.Create();
            Hook.HookAll();
            //Utils.TextureUtils.SetAllLodDistances();
            Utils.TMPEUTILS.FindTMPE();
        }

        public override void OnReleased() {
            Debug.Log("OnReleased kian mod");
            Hook.Release();
            base.OnReleased();
        }

    }
} // end namesapce
