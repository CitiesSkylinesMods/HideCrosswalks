namespace HideTMPECrosswalks.Utils {
    using HideCrosswalks;
    public static class PrephabUtils {
        public static bool CanHideMarkings(NetInfo info) => NetInfoExt.GetCanHideMarkings(info);
    }    
}
