using UnityEngine;
namespace HideTMPECrosswalks.Tests
{
    using static Program;
    public static class Program {
        public static void Log(string m) {
            System.Diagnostics.Debug.WriteLine(m);
            System.Console.WriteLine(m);
        }
        public static void Assert(bool con, string m) {
            //Log("Asserting: " + m);
            if (!con)
                Log("Assertion FAILED!!!! " + m);
            else
                Log("Assertion passed:" + m);
        }

        static void Main(string[] args)
        {
            //Log("Test1 ...");
            //TestColorUtils.Test1();

            //Log("Test2 ...");
            //TestColorUtils.Test2();

            Test();

            Log("End of tests");
            System.Console.ReadKey();
        }

        static void Test() {
            void ScaleRatio() {
                TheFunctionName();
            }
            FType lamdaScale = () => TheFunctionName();

            string s = GetName(lamdaScale);
            Log(s);
        }

        public static string GetName(FType f) {
            string s =  f.Method.Name;
            //string[] ss = s.Split(new[] { "g__", "|" }, System.StringSplitOptions.RemoveEmptyEntries);
            //if (ss.Length == 3)
            //    return ss[1];
            return s;
        }

        public delegate void FType();
        public static void TheFunctionName() {

        }
    }

    public static class TestColorUtils {
        public static void Test1() {
            Color c1 = new Color(.1f, .2f, .3f, .4f);
            Color c2 = Color.clear - c1;
            Color expected = new Color(-.1f, -.2f, -.3f, -.4f);
            string m = $"{Color.clear} - {c1} -> {c2} expected {expected}";
            Assert(c2 == expected, m);
        }

        public static void Test2() {
            Color c1 = new Color(1f, 1f, 1f, 1f);
            float w = 0.5f;
            Color c2 = c1 * w;
            Color expected = new Color(w,w,w,w);
            string m = $" {c1} * {w} -> {c2} expected {expected}";
            Assert(c2 == expected, m);

        }

    }
}
