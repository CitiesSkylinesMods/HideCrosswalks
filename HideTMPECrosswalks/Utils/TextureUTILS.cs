using UnityEngine;

namespace HideTMPECrosswalks.Utils {
    public static class TextureUtils {
        public static void Flip(Material material, string name) {
            var nodeTextureMain = material.GetTexture(name) as Texture2D;
            byte[] bytes = nodeTextureMain.MakeReadable().EncodeToPNG();
            Texture2D newTexture = new Texture2D(1, 1);
            newTexture.LoadImage(bytes);
            newTexture.anisoLevel = 16;
            newTexture = FlipTexture(newTexture);
            newTexture.Compress(true);
            material.SetTexture(name, newTexture);
        }

        //Texture flipping script from https://stackoverflow.com/questions/35950660/unity-180-rotation-for-a-texture2d-or-maybe-flip-both
        static Texture2D FlipTexture(Texture2D original) {
            Texture2D flipped = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;
            for (int i = 0; i < xN; i++) {
                for (int j = 0; j < yN; j++) {
                    flipped.SetPixel(j, xN - i - 1, original.GetPixel(j, i)); // flip upside down
                }
            }

            flipped.Apply();
            return flipped;
        }
    }
}
