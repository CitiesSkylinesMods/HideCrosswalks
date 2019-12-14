using System;
using ICities;
using UnityEngine;
using Harmony;
using System.Reflection;
using ColossalFramework;

namespace HideTMPECrosswalks {
    public class KianModInfo : LoadingExtensionBase, IUserMod {
        public string Name => "Test harmony";
        public string Description => "simplest CS harmony program 2";

        public void OnEnabled() {

        }
        public void OnDisabled() {

        }
    }

    public class LoadingExtension : LoadingExtensionBase {
        static HarmonyInstance Harmony = null;

        public override void OnCreated(ILoading loading) {
            Debug.Log("OnCreate CALLED *******************");
            Harmony = HarmonyInstance.Create("CS.kian.HideTMPECrosswalks"); // would creating 2 times cause an issue?
            Debug.Log("Harmony=" + Harmony);
            Harmony?.PatchAll(Assembly.GetExecutingAssembly());

            class1 c = new class1(5);
            c.m(5);
        }

        public override void OnReleased() {
            Harmony?.UnpatchAll();
            Harmony = null;
            OnReleased();
        }
    }

    [HarmonyPatch()]
    class harmoney_test {
        static MethodBase TargetMethod() {
            //	private void RenderInstance(RenderManager.CameraInfo cameraInfo, ushort nodeID, NetInfo info, int iter, NetNode.Flags flags, ref uint instanceIndex, ref RenderManager.Instance data)
            Debug.Log("TargetMethod called ");
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            var ret = typeof(NetNode).GetMethod("RenderInstance", flags);
            Debug.Log("**** TargetMethod : " + ret);
            return ret;
        }

        static bool Prefix() {
            // do not render!
            return false;
        }

    }

    public class class1 {

        public int _a;
        public class1(int a) => _a = a;
        public void m(int b) {
            Debug.Log(_a * b);
        }
    }

    [HarmonyPatch]
    static class class1_harmoniesed {
        static MethodBase TargetMethod() {
            return typeof(class1).GetMethod("m");
        }

        static bool Prefix(class1 __instance, int b) {
            Debug.Log(__instance._a * -b);

            return false;
        }
    }


}