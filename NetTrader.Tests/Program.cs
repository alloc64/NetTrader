using System;

namespace NetTrader.Tests
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Core.Constants.IsRunningInTestMode = true;

            Console.WriteLine("Tests started...");

            TestRunner testRunner = new TestRunner();

            testRunner.Start().GetAwaiter().GetResult();
        }
    }
}
