using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Relay_Tests
{
    public class MainClass
    {
        public static void Main(string[] arguments)
        {
            Relay_Tests.tests.RelayTwoTests t = new Relay_Tests.tests.RelayTwoTests();
            t.MergeTwoSubsets();
        }
    }
}
