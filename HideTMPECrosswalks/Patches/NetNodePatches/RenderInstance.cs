namespace HideCrosswalks.Patches.NetNodePatches {
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using KianCommons;
    using System.Linq;

    [HarmonyPatch()]
    public static class RenderInstance {
        static void Log(string m) => KianCommons.Log.Info("NetNode_RenderInstance Transpiler: " + m);

        static MethodBase TargetMethod() =>
            typeof(NetNode).GetMethod("RenderInstance", BindingFlags.NonPublic | BindingFlags.Instance, throwOnError: true);

        //static bool Prefix(ushort nodeID){}
        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions, MethodBase origin) {
            var codes = instructions.ToList();
            CalculateMaterialCommons.PatchCheckFlags(codes, occurance: 2, origin);
            return codes;
        }
    } // end class
} // end name space