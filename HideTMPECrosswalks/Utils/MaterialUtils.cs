using System;
using System.Collections;
using UnityEngine;

// TODO check out material.MainTextureScale
// regarding weird nodes, what if we return a copy of the material?
// Loading screens Mod owner wrote this about LODs: https://steamcommunity.com/workshop/filedetails/discussion/667342976/1636416951459546732/
namespace HideCrosswalks.Utils {
    using static TextureUtils;
    using static PrefabUtils;
    public static class MaterialUtils {
        internal static Hashtable MaterialCache = null;
        public static void Init() => MaterialCache = new Hashtable(500);
        public static void Clear() => MaterialCache = null;

        public static Texture2D TryGetTexture2D(this Material material, int textureID) {
            try {
                Texture texture = material.GetTexture(textureID);
                if (texture is Texture2D)
                    return texture as Texture2D;
            }
            catch { }
            Extensions.Log("Warning: failed to get texture from material :" + material.name);
            return null;
        }

        public static Material HideCrossings(Material material, Material segMaterial, NetInfo info, bool lod = false) {
            try {
                if (MaterialCache == null) {
                    return material; // program is loading/unloading
                }
                if (MaterialCache.Contains(material)) {
                    return (Material)MaterialCache[material];
                }
                if (HasSameNodeAndSegmentTextures(info, material, ID_Defuse)) {
                    // TODO why this works but the WierdNodeTest() fails.
                    string m = $"{info.name} is {info.category} is without proper node texture.";
                    Extensions.Log(m);
                    MaterialCache[material] = material;
                    return material;
                }

                var ticks = System.Diagnostics.Stopwatch.StartNew();
                Material ret = new Material(material);
                HideCrossings0(ret, segMaterial, info, lod);
                MaterialCache[material] = ret;
                Extensions.Log($"Cached new texture for {info.name} ticks=" + ticks.ElapsedTicks.ToString("E2"));
                return ret;
            }
            catch (Exception e) {
                Extensions.Log(e.ToString());
                MaterialCache[material] = material; // do not repeat the same mistake!
                return material;
            }
        }

        public static void HideCrossings0(Material material, Material segMaterial, NetInfo info, bool lod = false) {
            if (material == null) throw new ArgumentNullException("material");
            if (segMaterial == null) throw new ArgumentNullException("segMaterial");
            if (info == null) throw new ArgumentNullException("info");

            Texture2D tex, tex2;
            bool dump = false;
#if DEBUG
            //dump = lod;
#endif
            //if (dump) DumpUtils.Dump(info);

            tex = material.TryGetTexture2D(ID_Defuse);
            if (tex != null) {
                if (dump) DumpUtils.Dump(tex, info);
                if (TextureCache.Contains(tex)) {
                    tex = TextureCache[tex] as Texture2D;
                    Extensions.Log("Texture cache hit: " + tex.name);
                } else {
                    Extensions.Log("POINT A tex = " + tex.name);
                    tex = tex.GetReadableCopy();
                    tex.CropAndStrech(); if (dump) DumpUtils.Dump(tex, info);
                    tex.Finalize(lod);
                    TextureCache[tex] = tex;
                }
                if (dump) DumpUtils.Dump(tex, info);
                material.SetTexture(ID_Defuse, tex);
                if (dump) DumpUtils.Dump(tex, DumpUtils.GetFilePath(ID_Defuse, "node-processed", info));
            }

            if (info.category != "RoadsSmall" || !info.m_isCustomContent || info.isAsym()) {
                tex = material.TryGetTexture2D(ID_APRMap);
                tex2 = segMaterial.TryGetTexture2D(ID_APRMap); Extensions.Log("POINT B: tex.width=" + tex.width);
                if (tex != null && tex2 != null) {
                    if (dump) DumpUtils.Dump(tex, info);
                    if (dump) DumpUtils.Dump(tex2, info);
                    if (TextureCache.Contains(tex)) {
                        tex = TextureCache[tex] as Texture2D;
                        Extensions.Log("Texture cache hit: " + tex.name);
                    } else {
                        Extensions.Log("POINT B tex = " + tex.name);
                        bool linear = lod && !info.IsNExt();
                        tex = tex.GetReadableCopy(linear: linear);
                        tex2 = tex2.GetReadableCopy(linear: linear);

                        tex.CropAndStrech(); if (dump) DumpUtils.Dump(tex, info);
                        if (info.m_netAI is RoadAI) {
                            if (info.isAsym() && !info.isOneWay()) {
                                tex2.Mirror();
                                if (dump) DumpUtils.Dump(tex2, info);
                            }
                            tex2.Scale(info.ScaleRatio());
                            if (info.ScaleRatio() != 1f && dump) DumpUtils.Dump(tex2, info);
                        }
                        tex.MeldDiff(tex2); if (dump) DumpUtils.Dump(tex, info);

                        tex.Finalize(lod);
                        TextureCache[tex] = tex;
                    }
                    material.SetTexture(ID_APRMap, tex);
                    if (dump) DumpUtils.Dump(tex, DumpUtils.GetFilePath(ID_APRMap, "node-processed", info));
                } // end if cache
            } // end if tex
        } // end if category

        public static void NOPMaterial(Material material, bool lod = false) {
            if (material == null) throw new ArgumentNullException("material");

            Texture2D tex = material.TryGetTexture2D(ID_Defuse);
            if (tex != null) {
                tex = tex.GetReadableCopy();
                tex.NOP();
                material.SetTexture(ID_Defuse, tex);
            } // end if

            tex = material.TryGetTexture2D(ID_APRMap);
            if (tex != null) {
                tex = tex.GetReadableCopy(linear: lod);
                tex.NOP();
                material.SetTexture(ID_APRMap, tex);
            } // end if
        } // end method
    } // end class
} // end namesapce

