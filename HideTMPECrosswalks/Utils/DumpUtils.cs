
using System.IO;
using System;
using UnityEngine;

namespace HideTMPECrosswalks.Utils {
    using static TextureUtils;
    public static class DumpUtils {
        public static void LogUVs(NetInfo info1, NetInfo info2) {
            string m = "info1: " + info1.GetUncheckedLocalizedTitle() + "\n";
            m += "info2: " + info2.GetUncheckedLocalizedTitle() + "\n\n";
            //      info1 info2
            // S.<x y>
            // N.<x y>
            Vector2[] UVseg1 = info1.m_segments[0].m_mesh.uv;
            Vector2[] UVseg2 = info2.m_segments[0].m_mesh.uv;
            Vector2[] UVnode1 = info1.m_nodes[0].m_mesh.uv;
            Vector2[] UVnode2 = info2.m_nodes[0].m_mesh.uv;
            Vector2[] v11 = UVseg1;
            Vector2[] v21 = UVnode1;
            Vector2[] v12 = UVseg2;
            Vector2[] v22 = UVnode2;

            int n = Math.Min(Math.Min(UVseg1.Length, UVseg2.Length), Math.Min(UVnode1.Length, UVnode2.Length));
            string str(Vector2 v) => "<" + v.x.ToString("0.000") + " " + v.y.ToString("0.000") + ">";
            for (int i= 0; i < n; ++i){
                string s1 = str(v11[i]) + " | " + str(v12[i]);
                string s2 = str(v21[i]) + " | " + str(v22[i]);
                string s = i.ToString("000") + "   " + s1 + "\n";
                s += $"      " + s2 + "\n\n";
                m += s;
            }

            Extensions.Log(m);
        }

        public static void Dump(NetInfo info) {
            // not intersted in multi-thread processing. I just wan it to be outside of simulation thread.
            string roadName = info.GetUncheckedLocalizedTitle();
            for (int i = 0; i < info.m_segments.Length; ++i) {
                var seg = info.m_segments[i];
                Material material = seg.m_segmentMaterial;
                string baseName = "segment";
                //if (info.m_segments.Length > 1) baseName += i;
                Dump(material, baseName, info);
                break; //first one is enough
            }
            for (int i = 0; i < info.m_nodes.Length; ++i) {
                var node = info.m_nodes[i];
                Material material = node.m_nodeMaterial;
                string baseName = "node";
                if (info.m_nodes.Length > 1) baseName += i;
                Dump(material, baseName, info);
                break; //first one is enough
            }
        }

        public static void Dump(Material material, string baseName, NetInfo info ) {
            if (baseName == null) baseName = material.name;
            Dump(material, ID_Defuse, baseName, info);
            Dump(material, ID_APRMap, baseName, info);
            try { Dump(material, ID_XYSMap, baseName, info); } catch { }

        }

        public static void Dump(Material material, int texID, string baseName, NetInfo info) {
            if (material == null) throw new ArgumentNullException("material");
            Texture2D texture = material.GetReadableTexture(texID);
            string path = GetFilePath(texID, baseName ?? material.name, info);
            Dump(texture, path);
        }

        public static void Dump(Texture tex, string path) {
            Texture2D texture = tex.TryMakeReadable();
            Extensions.Log($"Dumping texture " + texture.name);
            byte[] bytes = texture.EncodeToPNG();
            Extensions.Log("Dumping to " + path);
            File.WriteAllBytes(path, bytes);
        }

        public static void Dump(Texture tex, NetInfo info) {
            string path = GetFilePath(
                texType: "",
                baseName: tex.name ?? throw new NullReferenceException("tex.name is null"),
                dir: info.GetUncheckedLocalizedTitle());
            Dump(tex, path);
        }

        public static string GetFilePath(int texID, string baseName, NetInfo info) {
            string dir = info?.GetUncheckedLocalizedTitle() ?? "dummy";
            return GetFilePath(getTexName(texID), baseName, dir);
        }

        public static string GetFilePath(string texType, string baseName, string dir) {
            string filename = baseName + texType + ".png";
            foreach (char c in @"\/:<>|" + "\"") {
                filename = filename.Replace(c.ToString(), "");
            }
            foreach (char c in @":<>|" + "\"") {
                dir = dir.Replace(c.ToString(), "");
            }

            string path = Path.Combine("mod debug dumps", dir);
            Directory.CreateDirectory(path);
            path = Path.Combine(path, filename);
            return path;
        }

        public static Texture2D Load(string path) {
            Extensions.Log("Loading Texture from " + path);
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            texture.anisoLevel = 16;
            texture.Compress(true);
            texture.name = path;
            return texture;
        }

    }
}
