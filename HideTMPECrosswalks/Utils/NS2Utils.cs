using ICities;
using System;
using System.IO;
using UnityEngine;

namespace HideTMPECrosswalks.Utils
{
    using static Extensions;

    public static class NS2Utils
    {
        private static bool warned = false;

        public static bool HideJunction(ushort segmentID)
        {
            try
            {
                return _HideJunction(segmentID);
            }
            catch (FileNotFoundException _)
            {
                if (!warned)
                {
                    Debug.Log("ERROR ****** NS2 not found! *****");
                    warned = true;
                }
            }
            catch (Exception e)
            {
                if (!warned)
                {
                    Debug.Log(e + "\n" + e.StackTrace);
                    warned = true;
                }
            }
            return false;

        }

        static Color32 c = new Color32(127, 127, 128, 255);
        private static bool _HideJunction(ushort segmentID)
        {
            var skin = NetworkSkins.Skins.NetworkSkinManager.SegmentSkins[segmentID];
            //return skin != null && skin.m_hidePedestrianCrossings;
            return skin != null && skin.m_color == c;
        }
    }
}
