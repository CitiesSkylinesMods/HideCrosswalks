using Harmony;
using ICities;
using JetBrains.Annotations;
using System;
using System.Reflection;
using TrafficManager;
using UnityEngine;
using HideTMPECrosswalks.Utils;

namespace HideTMPECrosswalks {

    public class KianModInfo : IUserMod {
        HarmonyInstance Harmony = null;
        string HarmonyID = "CS.kian.HideTMPECrosswalks";

        public string Name => "Hide TMPE crosswalks V2.0 [ALPHA]";
        public string Description => "Automatically hide crosswalk textures on segment ends when TMPE bans crosswalks";
        public void OnEnabled() {
            System.IO.File.WriteAllText("mod.debug.log", ""); // restart log.
            HarmonyInstance.DEBUG = true;
            Harmony = HarmonyInstance.Create(HarmonyID); // would creating 2 times cause an issue?
            Harmony?.PatchAll(Assembly.GetExecutingAssembly());
        }

        [UsedImplicitly]
        public void OnDisabled() {
            Harmony?.UnpatchAll(HarmonyID);
            Harmony = null;
            Patches.NetNode_RenderInstance.NodeMaterialTable.Clear();
        }
    }

    public class LoadingExtension : LoadingExtensionBase {
        public override void OnLevelLoaded(LoadMode mode) {
            Extensions.Log("OnLevelLoaded");
            //TODO execute post TMPE
            for (ushort segmentID = 0; segmentID < NetManager.MAX_SEGMENT_COUNT; ++segmentID) {
                foreach (bool bStartNode in new bool[] { false, true }) {
                    if (TMPEUTILS.HasCrossingBan(segmentID, bStartNode)){
                        NetSegment segment = segmentID.ToSegment();
                        ushort nodeID = bStartNode ? segment.m_startNode : segment.m_endNode;
                        foreach( var node in segment.Info.m_nodes) {
                            //cache:
                            Extensions.Log("Caching " + segment.Info.name);
                            Patches.NetNode_RenderInstance.CalculateMaterial(node.m_nodeMaterial, nodeID, segmentID);
                        }
                    }
                }
            }
        }
    }
}
