using Microsoft.VisualStudio.TestTools.UnitTesting;
using NGuava.Base;

namespace NGuava.Tests.Base
{
    [TestClass]
    public class JoinerTest
    {
        [TestMethod]
        public void TestTwoStingArguments()
        {
            var testInput = new[] {"a", "b"};
            var result = Joiner.On(", ").Join(testInput);
            Assert.AreEqual("a, b", result);
        }
    }
}
