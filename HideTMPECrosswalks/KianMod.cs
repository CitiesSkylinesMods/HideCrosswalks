using Harmony;
using ICities;
using System.Reflection;
using UnityEngine;

namespace HideTMPECrosswalks {
    public class KianModInfo : IUserMod {
        public string Name => "Hide TMPE crosswalks";
        public string Description => "Automatically hide crosswalk textures on segment ends when TMPE bans crosswalks";

        static HarmonyInstance Harmony = null;

        public void OnEnabled() {
            Debug.Log("Installing Harmony *******************");
            Harmony = HarmonyInstance.Create("CS.kian.HideTMPECrosswalks"); // if created 2 times would it cause an issue?
            Debug.Log("Harmony=" + Harmony);
            Harmony?.PatchAll(Assembly.GetExecutingAssembly());
        }
        public void OnDisabled() {
            Harmony?.UnpatchAll();
            Harmony = null;
            //OnReleased();
        }
    }

    public class LoadingExtension : LoadingExtensionBase {

        public override void OnCreated(ILoading loading) {

        }

        public override void OnReleased() {

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

}
