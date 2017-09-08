using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NGuava.Base;

namespace NGuava.Tests.Base
{
    [TestClass]
    public class SplitterTest
    {
        [TestMethod]
        public void TestSplitter()
        {
            var splitter = Splitter.On(',');
            var enumerable = splitter.split("a, b, b,c");
            var result = new List<string>(enumerable);
            CollectionAssert.AreEqual(new List<string> {"a", " b", " b", "c"}, result);
        }


        [TestMethod]
        public void TestSplitter2()
        {
            var splitter = Splitter.On(',').OmitEmptyStrings();
            var enumerable = splitter.split("a,,,,d,c");
            var result = new List<string>(enumerable);
            CollectionAssert.AreEqual(new List<string> { "a", "d",  "c" }, result);
        }
    }
}