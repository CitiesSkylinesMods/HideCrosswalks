using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
//using ColossalFramework;

namespace HideTMPECrosswalks.Patch {
    public class Hook {
        public static Hook instance;
        public static List<HookBase> hooks = new List<HookBase>();

        public Hook() {
            NetNodeDetours.Init();
            hooks.Add(new RenderInstance());
            //hooks.Add(new RenderLod());
        }

        public static void Create() => instance = new Hook();

        public static void Release() {
            UnHookAll();
            hooks.Clear();
            instance = null;
        }

        public static void HookAll() {
            UnHookAll();
            foreach (var h in hooks) {
                h.Hook();
            }
            Debug.Log("hooked everything");
        }
        public static void UnHookAll() {
            foreach (var h in hooks) {
                h.UnHook();
            }
            Debug.Log("unhooked everything");
        }

        public class RenderInstance : HookBase {
            private BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            public override MethodInfo From => typeof(NetNode).GetMethod("RenderInstance", flags);
            public override MethodInfo To => typeof(NetNodeDetours).GetMethod("RenderInstance");
        }

        public class RenderLod : HookBase {
            public override MethodInfo From => typeof(NetNode).GetMethod("RenderLod");
            public override MethodInfo To => typeof(NetNodeDetours).GetMethod("RenderLod");
        }



    } // end class Hook
}