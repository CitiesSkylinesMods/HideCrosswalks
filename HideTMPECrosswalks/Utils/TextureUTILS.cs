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

            for (int j = 0; j < yN0; j++) {
                int j2 = (int)(j * (stretchPortion - cropPortion) + yN * cropPortion);
                ret.SetPixels(0, j, xN, 1, original.GetPixels(0, j2, xN, 1));
            }
            ret.SetPixels(0, yN0, xN, yN - yN0, original.GetPixels(0, yN0, xN, yN - yN0));

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


            ret.SetPixels(0, 0, xN, yN, original.GetPixels(0, 0, xN, yN));
            for (int j = 0; j < yN-yN0; j++) {
                int j2 = (int)(j * (stretchPortion - cropPortion)) + yN0;
                ret.SetPixels(0, j, xN,1, original.GetPixels(0, j2,xN,1));
            }



            ret.Apply();
            return ret;
        }

    }
}
