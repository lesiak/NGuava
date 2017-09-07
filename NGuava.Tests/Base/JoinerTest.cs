using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NGuava.Base;

namespace NGuava.Tests.Base
{
    [TestClass]
    public class JoinerTest
    {

        [TestMethod]
        public void TestOneString()
        {
            var testInput = new[] { "a" };
            var result = Joiner.On(", ").Join(testInput);
            Assert.AreEqual("a", result);
        }


        [TestMethod]
        public void TestOneInt()
        {
            var testInput = new[] { 1 };
            var result = Joiner.On(", ").Join(testInput);
            Assert.AreEqual("1", result);
        }


        [TestMethod]
        public void TestTwoInts()
        {
            var testInput = new[] {1, 2};
            var result = Joiner.On(", ").Join(testInput);
            Assert.AreEqual("1, 2", result);
        }


        [TestMethod]
        public void TestTwoStrings()
        {
            var testInput = new[] { "a", "b" };
            var result = Joiner.On(", ").Join(testInput);
            Assert.AreEqual("a, b", result);
        }


        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void TestStringAndNull()
        {
            var testInput = new[] {"a", null};
            Joiner.On(", ").Join(testInput);
        }


        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void TestNullAndString()
        {
            var testInput = new[] { null, "b" };
            Joiner.On(", ").Join(testInput);
        }


        [TestMethod]
        public void TestStringAndNullSubstituted()
        {
            var testInput = new[] {"a", null};
            var result = Joiner.On(", ").UseForNull("null").Join(testInput);
            Assert.AreEqual("a, null", result);
        }


        [TestMethod]
        public void TestStringAndNullSkipped()
        {
            var testInput = new[] { "a", null };
            var result = Joiner.On(", ").SkipNulls().Join(testInput);
            Assert.AreEqual("a", result);
        }


        [TestMethod]
        public void TestNullAndStringSkipped()
        {
            var testInput = new[] { null, "b" };
            var result = Joiner.On(", ").SkipNulls().Join(testInput);
            Assert.AreEqual("b", result);
        }


        [TestMethod]
        public void TestNullInTheMiddleSkipped()
        {
            var testInput = new[] { "a", null, "b" };
            var result = Joiner.On(", ").SkipNulls().Join(testInput);
            Assert.AreEqual("a, b", result);
        }


        [TestMethod]
        public void TestMultipleNullsInFrontSkipped()
        {
            var testInput = new[] { null, null, "b" };
            var result = Joiner.On(", ").SkipNulls().Join(testInput);
            Assert.AreEqual("b", result);
        }
    }
}