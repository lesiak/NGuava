using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NGuava.Base;

namespace NGuava.Tests.Base
{
    [TestClass]
    public class JoinerTest
    {

        private static readonly Joiner J = Joiner.On("-");

        // <Integer> needed to prevent warning :(
        private static readonly IEnumerable<string> ITERABLE_ = new List<string>();

        private static readonly IEnumerable<string> ITERABLE_1 = new List<string> {"1"};
        private static readonly IEnumerable<string> ITERABLE_12 = new List<string> {"1", "2"};
        private static readonly IEnumerable<string> ITERABLE_123 = new List<string> {"1", "2", "3"};
        private static readonly IEnumerable<string> ITERABLE_NULL = new List<string> {null};
        private static readonly IEnumerable<string> ITERABLE_NULL_NULL = new List<string> {null, null};
        private static readonly IEnumerable<string> ITERABLE_NULL_1 = new List<string> {null, "1"};
        private static readonly IEnumerable<string> ITERABLE_1_NULL = new List<string> {"1", null};
        private static readonly IEnumerable<string> ITERABLE_1_NULL_2 = new List<string> {"1", null, "2"};
        private static readonly IEnumerable<string> ITERABLE_FOUR_NULLS = new List<string> {null, null, null, null};

        [TestMethod]
        public void TestNoSpecialNullBehavior()
        {
            CheckNoOutput(J, ITERABLE_);
            CheckResult(J, ITERABLE_1, "1");
            CheckResult(J, ITERABLE_12, "1-2");
            CheckResult(J, ITERABLE_123, "1-2-3");

            try
            {
                J.Join(ITERABLE_NULL);
                Assert.Fail();
            }
            catch (NullReferenceException)
            {
            }
            try
            {
                J.Join(ITERABLE_1_NULL_2);
                Assert.Fail();
            }
            catch (NullReferenceException)
            {
            }

            try
            {
                J.Join(ITERABLE_NULL.GetEnumerator());
                Assert.Fail();
            }
            catch (NullReferenceException)
            {
            }
            try
            {
                J.Join(ITERABLE_1_NULL_2.GetEnumerator());
                Assert.Fail();
            }
            catch (NullReferenceException)
            {
            }
        }

        [TestMethod]
        public void TestOneString()
        {
            var testInput = new[] {"a"};
            var result = Joiner.On(", ").Join(testInput);
            Assert.AreEqual("a", result);
        }


        [TestMethod]
        public void TestOneInt()
        {
            var testInput = new[] {1};
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
            var testInput = new[] {"a", "b"};
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
            var testInput = new[] {null, "b"};
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
            var testInput = new[] {"a", null};
            var result = Joiner.On(", ").SkipNulls().Join(testInput);
            Assert.AreEqual("a", result);
        }


        [TestMethod]
        public void TestNullAndStringSkipped()
        {
            var testInput = new[] {null, "b"};
            var result = Joiner.On(", ").SkipNulls().Join(testInput);
            Assert.AreEqual("b", result);
        }


        [TestMethod]
        public void TestNullInTheMiddleSkipped()
        {
            var testInput = new[] {"a", null, "b"};
            var result = Joiner.On(", ").SkipNulls().Join(testInput);
            Assert.AreEqual("a, b", result);
        }


        [TestMethod]
        public void TestMultipleNullsInFrontSkipped()
        {
            var testInput = new[] {null, null, "b"};
            var result = Joiner.On(", ").SkipNulls().Join(testInput);
            Assert.AreEqual("b", result);
        }

        private static void CheckNoOutput(Joiner joiner, IEnumerable<string> set)
        {
            Assert.AreEqual("", joiner.Join(set));
            Assert.AreEqual("", joiner.Join(set.GetEnumerator()));
        }

        private static void CheckResult(Joiner joiner, IEnumerable<string> parts, string expected)
        {
            Assert.AreEqual(expected, joiner.Join(parts));
            Assert.AreEqual(expected, joiner.Join(parts.GetEnumerator()));
        }

    }

}
