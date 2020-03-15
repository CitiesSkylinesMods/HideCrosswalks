using UnityEngine;
namespace HideCrosswalks.Tests
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
            Log("Test1 ...");
            Experiments.Test1();

            //Log("Test2 ...");
            //Experiments.Test2();


            Log("End of tests");
            System.Console.ReadKey();
        }


    }

    public static class Experiments {
        class X { }
        class Y : X { }
        public static void Test1() {
            X y = null;
            bool b = y is Y;
            Log($"b={b}");
        }


    }
}
