using System;
using ColossalFramework;
using UnityEngine;
using HideTMPECrosswalks.Utils;
using System.Reflection;
using Harmony;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace HideTMPECrosswalks.Patches {
    using Utils;
    [HarmonyPatch()]
    public static class NetNode_RenderInstance {
        public static bool ShouldHideCrossing(ushort nodeID, ushort segmentID) {

            NetInfo info = segmentID.ToSegment().Info;
            bool ret = info.m_netAI is RoadBaseAI && TMPEUTILS.HasCrossingBan(segmentID, nodeID);
            // roads without pedesterian lanes (eg highways) have no crossings to hide to the best of my knowledege.
            // not sure about custom highways. Processing texture for such roads may reduce smoothness of the transition.
            ret &= info.m_hasPedestrianLanes;

            //Texture cache is not broken.
            ret &= PrefabUtils.NodeMaterialTable != null;

            bool never = PrefabUtils.NeverZebra(info);
            bool always = PrefabUtils.AlwaysZebra(info);
            ret |= always;
            ret &= !never;

            return ret;
        }

        public static Material CalculateMaterial(Material material, ushort nodeID, ushort segmentID) {
            if (ShouldHideCrossing(nodeID, segmentID)) {
                material = PrefabUtils.HideCrossing(material, segmentID.ToSegment().Info);
            }
            return material;
        }

        static void Log(string m) => Extensions.Log("NetNode_RenderInstance Transpiler: " + m);

        //static bool Prefix(ushort nodeID) {
        //    NetNode node = nodeID.ToNode();
        //    return true;
        //}

        static MethodBase TargetMethod() {
            Log("TargetMethod");
            // RenderInstance(RenderManager.CameraInfo cameraInfo, ushort nodeID, NetInfo info, int iter, Flags flags, ref uint instanceIndex, ref RenderManager.Instance data)
            var ret = typeof(global::NetNode).GetMethod("RenderInstance", BindingFlags.NonPublic | BindingFlags.Instance);
            if (ret == null) {
                throw new Exception("did not manage to find original function to patch");
            }
            Log("aquired method " + ret);
            return ret;
        }

        #region Transpiler
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
        static Type[] args = new[] {
                typeof(Mesh),
                typeof(Vector3),
                typeof(Quaternion),
                typeof(Material),
                typeof(int),
                typeof(Camera),
                typeof(int),
                typeof(MaterialPropertyBlock) };
        static MethodInfo mDrawMesh => typeof(Graphics).GetMethod("DrawMesh", args);
        static FieldInfo fNodeMaterial => typeof(NetInfo.Node).GetField("m_nodeMaterial");
        static MethodInfo mCalculateMaterial => typeof(NetNode_RenderInstance).GetMethod("CalculateMaterial");
        static MethodInfo mGetSegment => typeof(NetNode).GetMethod("GetSegment");
        static MethodInfo mCheckRenderDistance => typeof(RenderManager.CameraInfo).GetMethod("CheckRenderDistance");
        static MethodInfo mShouldHideCrossing => typeof(NetNode_RenderInstance).GetMethod("ShouldHideCrossing");

        public static IEnumerable<CodeInstruction> Transpiler(ILGenerator il, IEnumerable<CodeInstruction> instructions) {
            try {
                if (mCalculateMaterial == null || mDrawMesh == null || fNodeMaterial == null || mGetSegment == null ||
                    mCheckRenderDistance == null || mShouldHideCrossing == null) {
                    string m = "Necessary methods and fields were not found. Cancelling transpiler!\n";
                    m += $" mCalculateMaterial={mCalculateMaterial} \n mDrawMesh={mDrawMesh} \n fMaterial={fNodeMaterial} \n";
                    m += $" mGetSegment={mGetSegment}\n mCheckDistance={mCheckRenderDistance}\n mShouldHideCrossing={mShouldHideCrossing}";
                    throw new Exception(m);
                }

                var originalCodes = new List<CodeInstruction>(instructions);
                var codes = new List<CodeInstruction>(originalCodes);

                patchDrawMesh(codes, 2, 13); // patch second draw mesh.

                Log("successfully patched NetNode.RenderInstance");

                return codes;
            }catch(Exception e) {
                Log(e + "\n" + Environment.StackTrace);
                throw e;
            }
        }

        // returns the position of First DrawMesh after index.
        public static void patchDrawMesh(List<CodeInstruction> codes, int counter, byte segmentID_loc) {
            int index = 0;
            index = SearchInstruction(codes, new CodeInstruction(OpCodes.Call, mDrawMesh), index, counter:counter );

            // find ldfld node.m_material
            index = SearchInstruction(codes, new CodeInstruction(OpCodes.Ldfld, fNodeMaterial), index, dir:-1);
            int insertIndex2 = index + 1;

            // find if (cameraInfo.CheckRenderDistance(data.m_position, node.m_lodRenderDistance))
            /* IL_0627: callvirt instance bool RenderManager CameraInfo::CheckRenderDistance(Vector3, float32)
             * IL_062c brfalse      IL_07e2 */
            index = SearchInstruction(codes, new CodeInstruction(OpCodes.Callvirt, mCheckRenderDistance), index, dir: -1);
            int insertIndex1 = index + 1; // at this point boloean is in stack

            { // Insert material = CalculateMaterial(material, nodeID, segmentID)
                var newInstructions = new[] {
                    new CodeInstruction(OpCodes.Ldarg_2),// ldarg.2 | push nodeID into the stack,
                    new CodeInstruction(OpCodes.Ldloc_S, segmentID_loc), // ldloc.s segmentID | push segmentID into the stack
                    new CodeInstruction(OpCodes.Call, mCalculateMaterial), // call Material CalculateMaterial(material, nodeID, segmentID).
                };
                InsertInstructions(codes, newInstructions, insertIndex2);
            }

            { // Insert ShouldHideCrossing(nodeID, segmentID)
                var newInstructions = new[]{
                    new CodeInstruction(OpCodes.Ldarg_2), // ldarg.2 | push nodeID into the stack
                    new CodeInstruction(OpCodes.Ldloc_S, segmentID_loc), // ldloc.s segmentID | push segmentID into the stack
                    new CodeInstruction(OpCodes.Call, mShouldHideCrossing), // call Material mShouldHideCrossing(nodeID, segmentID).
                    new CodeInstruction(OpCodes.Or) };

                InsertInstructions(codes, newInstructions, insertIndex1);
            } // end block
        } // end method


        static int SearchInstruction(List<CodeInstruction> codes, CodeInstruction instruction, int index, int dir = +1, int counter = 1) {
            int count = 0;
            for (; index < codes.Count; index += dir) {
                if (TranspilerUtils.IsSameInstruction(codes[index],instruction)) {
                    if (++count == counter)
                        break;
                }
            }
            if (index >= codes.Count || count != counter) {
                throw new Exception(" Did not found instruction: " + instruction);
            }
            Log(" Found : \n" + codes[index] + codes[index+1]);
            return index;
        }

        static void InsertInstructions(List<CodeInstruction> codes, CodeInstruction[] insertion, int index) {
            foreach(var code in insertion)
                if (code == null)
                    throw new Exception("Bad Instructions:\n" + insertion.IL2STR());
            Log($"Insert point:\n between: <{codes[index]}>  and  <{codes[index+1]}>");

            codes.InsertRange(index, insertion);

            //Log("\n" + insertion.IL2STR());
            //Log("TRANSPILER PEEK:\n" + codes.GetRange(index - 4, 14).IL2STR(););
        }
        #endregion Transpiler
    } // end class
} // end name space