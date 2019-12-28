using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            Log("Asserting: " + m);
            System.Diagnostics.Debug.Assert(con, m);
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
            try {
                throw new System.IO.FileNotFoundException("Some test exception");
            }
            catch {
                Log("Catched exception");
            }
            Log("POINT A");
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
