using System;
using CompteEstBon;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestCeb
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        
        {
            new CebTirage().Resolve();
        }
    }
}
