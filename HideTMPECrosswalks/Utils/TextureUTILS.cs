using UnityEngine;

namespace HideTMPECrosswalks.Utils {
    public static class TextureUtils {
        public delegate Texture2D TProcessor(Texture2D tex);

        public static void Process(Material material, string name, TProcessor func) {
            var nodeTextureMain = material.GetTexture(name) as Texture2D;
            byte[] bytes = nodeTextureMain.MakeReadable().EncodeToPNG();
            Texture2D newTexture = new Texture2D(1, 1);
            newTexture.LoadImage(bytes);
            newTexture.anisoLevel = 16;
            newTexture = func(newTexture);
            newTexture.Compress(true);
            material.SetTexture(name, newTexture);
        }

        //Texture flipping script from https://stackoverflow.com/questions/35950660/unity-180-rotation-for-a-texture2d-or-maybe-flip-both
        public static Texture2D Flip(Texture2D original) {
            Texture2D ret = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;
            for (int i = 0; i < xN; i++) {
                for (int j = 0; j < yN; j++) {
                    ret.SetPixel(j, xN - i - 1, original.GetPixel(j, i)); // flip upside down
                }
            }

            ret.Apply();
            return ret;
        }

        public static Texture2D Crop(Texture2D original) {
            Texture2D ret = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;

            float cropPortion = 0.30f;
            float stretchPortion = 0.40f;
            int yN0 = (int)(yN * stretchPortion);

            for (int i = 0; i < xN; i++) {
                for (int j = 0; j < yN0; j++) {
                    int j2 = (int)(j * (stretchPortion - cropPortion) + yN * cropPortion);
                    ret.SetPixel(i,j, original.GetPixel( i, j2) );
                }
                for (int j = yN0; j < yN; j++) {
                    ret.SetPixel(i, j, original.GetPixel(i, j));
                }
            }

            ret.Apply();
            return ret;
        }


        public static Texture2D CropEnd(Texture2D original) {
            Texture2D ret = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;

            float cropPortion = 0.20f;
            float stretchPortion = 0.30f;
            int yN0 = (int)(yN * (1- stretchPortion));

            for (int i = 0; i < xN; i++) {
                for (int j = 0; j < yN0; j++) {
                    ret.SetPixel(i, j, original.GetPixel(i, j));
                }
                for (int j = 0; j < yN-yN0; j++) {
                    int j2 = (int)(j * (stretchPortion - cropPortion)) + yN0;
                    ret.SetPixel(i, j, original.GetPixel(i, j2));
                }
            }

            ret.Apply();
            return ret;
        }

    }
}
