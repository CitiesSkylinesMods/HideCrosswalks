using System;
using ColossalFramework;
using System.Collections;
using UnityEngine;
using HideTMPECrosswalks.Utils;
using System.Reflection;
using Harmony;
using System.Collections.Generic;
using System.Reflection.Emit;


namespace HideTMPECrosswalks.Patch {

    [HarmonyPatch()]
    public class NetNode_RenderInstance {
        public static Hashtable NodeMaterialTable = new Hashtable(100);

        public static Material HideCrossing(Material material) {
            if (NodeMaterialTable.Contains(material)) {
                return (Material)NodeMaterialTable[material];
            }

            Material ret = new Material(material);
            TextureUtils.Flip(ret, "_MainTex");
            TextureUtils.Flip(ret, "_APRMap");
            try { TextureUtils.Flip(ret, "_XYSMap"); } catch (Exception) { }
            //TextureUtils.Clean(ret);
            NodeMaterialTable[material] = ret;
            return ret;
        }

        private static bool ShouldHideCrossing(ushort nodeID, ushort segmentID) {
            return nodeID.ToNode().Info.m_netAI is RoadAI && TMPEUTILS.HasCrossingBan(segmentID, nodeID);
        }

        public static Material CalculateMaterial(Material material, ushort nodeID, ushort segmentID) {
            if (ShouldHideCrossing(nodeID, segmentID)) {
                material = HideCrossing(material);
            }
            return material;
        }


        // extra arguments passed for flexibality of future use.
        //private static Material GetMaterial(ushort nodeID, ushort segmentID, ushort prefabNodeIDX, NetInfo info, bool hideCrossing ) {
        //    Material material = info.m_nodes[prefabNodeIDX].m_material;
        //    if (hideCrossing) {
        //        material= HideCrossing(material);
        //    }
        //    return material;
        //}


        static MethodBase TargetMethod() {
            Debug.Log("TargetMethod");
            // RenderInstance(RenderManager.CameraInfo cameraInfo, ushort nodeID, NetInfo info, int iter, Flags flags, ref uint instanceIndex, ref RenderManager.Instance data)
            var ret = typeof(global::NetNode).GetMethod("RenderInstance", BindingFlags.NonPublic | BindingFlags.Instance);
            if (ret == null) {
                var m = "ERRRRRRORRRRRR!!!!: did not manage to find original function to patch" + Environment.StackTrace;
                Debug.Log(m);
                throw new Exception(m);
            }
            Debug.Log("NetNode: aquired private method " + ret);
            return ret;
        }


        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions) {
            /* public static void DrawMesh(
                      Mesh mesh,
                      Vector3 position,
                      Quaternion rotation,
                      Material material,
                      int layer,
                      Camera camera,
                      int submeshIndex,
                      MaterialPropertyBlock properties);
                      */
            var args = new[] {
                typeof(Mesh),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(Material),
                typeof(int),
                typeof(Camera),
                typeof(int),
                typeof(MaterialPropertyBlock) };
            var mDrawMesh = typeof(Graphics).GetMethod("DrawMesh", args);
            var fMaterial = typeof(NetInfo.Node).GetField("m_material");
            var mCalculateMaterial = typeof(NetNode_RenderInstance).GetMethod("CalculateMaterial");
            var mGetSegment = typeof(global::NetNode).GetMethod("GetSegment");

            if (mCalculateMaterial == null || mDrawMesh == null || fMaterial == null || mGetSegment == null) {
                Debug.LogError("NetNode_RenderInstance Transpiler: Necessary methods and field not found. Cancelling transpiler!");
                Debug.LogError($"mCalculateMaterial={mCalculateMaterial} \n mDrawMesh={mDrawMesh} \n fMaterial={fMaterial} \n mGetSegment={mGetSegment}");
                return instructions;
            }

            var originalCodes = new List<CodeInstruction>(instructions);
            var codes = new List<CodeInstruction>(originalCodes);

            int index = 0;
            {   //find the second draw mesh:
                int count = 0;

                for (; index < codes.Count; ++index) {
                    var code = codes[index];
                    if (code.opcode == OpCodes.Call && code.operand == mDrawMesh) {
                        if (++count == 2)
                            break;
                    }
                }
                if (count != 2) {
                    Debug.LogError("NetNode_RenderInstance Transpiler: Did not found second call to DrawMesh()!");
                    return instructions;
                }
            }

            // find ldfld node.m_material
            while (--index > 0) {
                var code = codes[index];
                if (code.opcode == OpCodes.Ldfld && code.operand == fMaterial)
                    break;
            }
            var insertIndex = index + 1; // at this point material is in stack

            CodeInstruction segmentLocalVarLdloc = null;
            // find segmentID = GetSegment(...) and build ldloc.s segmentID
            while (--index > 0) {
                var code = codes[index];
                // call instance uint16 NetNode::GetSegment(int32)
                if (code.opcode == OpCodes.Call && code.operand == mGetSegment && TranspilerUtils.IsStLoc(codes[index + 1])) {
                    // stloc.s segment
                    Debug.Log("Found GetSegment");
                    segmentLocalVarLdloc = TranspilerUtils.BuildLdLocFromStLoc(codes[index + 1]);
                    break;
                }
            }

            // material = CalculateMaterial(material, nodeID, segmentID)
            var i1 = new CodeInstruction(OpCodes.Ldarg_2); // ldarg.2 | push nodeID into the stack
            var i2 = new CodeInstruction(OpCodes.Call, mCalculateMaterial); // call Material CalculateMaterial(material, nodeID, segmentID).
            if (i1 == null || i2 == null || segmentLocalVarLdloc == null) {
                Debug.LogError("NetNode_RenderInstance Transpiler: Did not manage to generate valid code!");
                Debug.Log($"i1 {i1} \n segmentLocalVarLdloc {segmentLocalVarLdloc} \n i2 {i2} ");
                return instructions;
            }

            var instructionsCalculateMaetrial = new[] {
                i1,
                segmentLocalVarLdloc, // ldloc.s segmentID | push segmentID into the stack
                i2
            };

            codes.InsertRange(insertIndex, instructionsCalculateMaetrial);

            Debug.Log("transpiler successfully patched NetNode.RenderInstance !!!!!!!*******");

            return codes;
        }
    }
}