namespace HideCrosswalks.Patches {
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using System.Reflection;
    using UnityEngine;
    using Utils;
    using KianCommons;
    using KianCommons.Patches;

    public static class CalculateMaterialCommons {
        public static bool ShouldHideCrossing(ushort nodeID, ushort segmentID) {
            // TODO move to netnode.updateflags or something
            NetInfo info = segmentID.ToSegment().Info;

            // this assertion can fail due to race condition (when node controller post chanes node).
            // therefore i am commenting this out.
            // it would be nice to move as much of this code in simulation thread (eg netnode.updateflags)
            //bool isJunction = nodeID.ToNode().m_flags.IsFlagSet(NetNode.Flags.Junction);
            //Assertion.Assert(isJunction, $"isJunction | segmentID:{segmentID} nodeID:{nodeID}");

            bool hideMarkings = NetInfoExt.GetCanHideMarkings(info) & NS2Utils.HideJunctionMarkings(segmentID);
            bool hideCrossings = TMPEUTILS.HasCrossingBan(segmentID, nodeID) & NetInfoExt.GetCanHideCrossings(info);
            bool ret =  hideMarkings | hideCrossings;
            // Log.Debug($"ShouldHideCrossing segmentID={segmentID} nodeID={nodeID} ret0:{ret0} ret1:{ret1} ret2:{ret2} ret:{ret}");
            return ret;
        }

        public static Material CalculateMaterial(Material material, ushort nodeID, ushort segmentID) {
            if (ShouldHideCrossing(nodeID, segmentID)) {
                NetInfo netInfo = segmentID.ToSegment().Info;
                material = MaterialUtils.HideCrossings(material, null, netInfo, lod:false);
            }
            return material;
        }

        delegate void DrawMesh(Mesh mesh, Vector3 position, Quaternion rotation, Material material, int layer, Camera camera, int submeshIndex, MaterialPropertyBlock properties);
        static MethodInfo mDrawMesh = typeof(Graphics).GetMethod<DrawMesh>(throwOnError: true);
        static FieldInfo fNodeMaterial =
            ReflectionHelpers.GetField<NetInfo.Node>(nameof(NetInfo.Node.m_nodeMaterial));
        static MethodInfo mCalculateMaterial =
            typeof(CalculateMaterialCommons).GetMethod("CalculateMaterial", throwOnError: true);
        static MethodInfo mCheckRenderDistance =
            typeof(RenderManager.CameraInfo).GetMethod("CheckRenderDistance", throwOnError: true);
        static MethodInfo mShouldHideCrossing =
            typeof(CalculateMaterialCommons).GetMethod("ShouldHideCrossing", throwOnError: true);
        static MethodInfo mGetSegment =
            typeof(NetNode).GetMethod("GetSegment", throwOnError: true);

        // returns the position of First DrawMesh after index.
        public static void PatchCheckFlags(List<CodeInstruction> codes, int occurance, MethodBase method) {
            //Assertion.Assert(mDrawMesh != null, "mDrawMesh!=null failed");
            //Assertion.Assert(fNodeMaterial != null, "fNodeMaterial!=null failed"); 
            //Assertion.Assert(mCalculateMaterial != null, "mCalculateMaterial!=null failed"); 
            //Assertion.Assert(mCheckRenderDistance != null, "mCheckRenderDistance!=null failed"); 
            //Assertion.Assert(mShouldHideCrossing != null, "mShouldHideCrossing!=null failed");

            int index = 0;
            index = codes.Search(c => c.Calls(mDrawMesh), startIndex: index, count: occurance);
            Assertion.Assert(index != 0, "index!=0");


            // find ldfld node.m_material
            index = codes.Search(c => c.LoadsField(fNodeMaterial), startIndex: index, count: -1);
            int insertIndex2 = index + 1;

            // find: if (cameraInfo.CheckRenderDistance(data.m_position, node.m_lodRenderDistance))
            /* IL_0627: callvirt instance bool RenderManager CameraInfo::CheckRenderDistance(Vector3, float32)
             * IL_062c brfalse      IL_07e2 */
            index = codes.Search(c => c.Calls(mCheckRenderDistance), startIndex: index, count: -1);
            int insertIndex1 = index + 1; // at this point boloean is in stack


            CodeInstruction LDArg_NodeID = TranspilerUtils.GetLDArg(method, "nodeID"); // push nodeID into stack
            CodeInstruction LDLoc_segmentID = BuildSegnentLDLocFromPrevSTLoc(codes, index, counter: 1); // push segmentID into stack

            { // Insert material = CalculateMaterial(material, nodeID, segmentID)
                var newInstructions = new[] {
                    LDArg_NodeID,
                    LDLoc_segmentID,
                    new CodeInstruction(OpCodes.Call, mCalculateMaterial), // call Material CalculateMaterial(material, nodeID, segmentID).
                };
                codes.InsertInstructions(insertIndex2, newInstructions);
            }

            { // Insert ShouldHideCrossing(nodeID, segmentID)
                var newInstructions = new[]{
                    LDArg_NodeID, 
                    LDLoc_segmentID, 
                    new CodeInstruction(OpCodes.Call, mShouldHideCrossing), // call Material mShouldHideCrossing(nodeID, segmentID).
                    new CodeInstruction(OpCodes.Or) };
                codes.InsertInstructions(insertIndex1, newInstructions);
            } // end block
        } // end method

        public static CodeInstruction BuildSegnentLDLocFromPrevSTLoc(List<CodeInstruction> codes, int index, int counter=1) {
            Assertion.Assert(mGetSegment != null, "mGetSegment!=null");
            index = codes.Search(c => c.Calls(mGetSegment), startIndex: index, count: -counter);

            var code = codes[index + 1];
            Assertion.Assert(code.IsStloc(), $"IsStLoc(code) | code={code}");

            return code.BuildLdLocFromStLoc();
            
        }



    }
}
