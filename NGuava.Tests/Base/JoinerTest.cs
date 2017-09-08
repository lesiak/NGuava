using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NGuava.Base;

namespace NGuava.Tests.Base
{
    [TestClass]
    public class JoinerTest
    {
        private static readonly Joiner J = Joiner.On("-");

        private static readonly IList<string> ITERABLE_ = new List<string>();

        private static readonly IList<string> ITERABLE_1 = new List<string> {"1"};
        private static readonly IList<string> ITERABLE_12 = new List<string> {"1", "2"};
        private static readonly IList<string> ITERABLE_123 = new List<string> {"1", "2", "3"};
        private static readonly IList<string> ITERABLE_NULL = new List<string> {null};
        private static readonly IList<string> ITERABLE_NULL_NULL = new List<string> {null, null};
        private static readonly IList<string> ITERABLE_NULL_1 = new List<string> {null, "1"};
        private static readonly IList<string> ITERABLE_1_NULL = new List<string> {"1", null};
        private static readonly IList<string> ITERABLE_1_NULL_2 = new List<string> {"1", null, "2"};
        private static readonly IList<string> ITERABLE_FOUR_NULLS = new List<string> {null, null, null, null};

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
        public void TestOnCharOverride()
        {
            var onChar = Joiner.On('-');
            CheckNoOutput(onChar, ITERABLE_);
            CheckResult(onChar, ITERABLE_1, "1");
            CheckResult(onChar, ITERABLE_12, "1-2");
            CheckResult(onChar, ITERABLE_123, "1-2-3");
        }

        [TestMethod]
        public void TestSkipNulls()
        {
            var skipNulls = J.SkipNulls();
            CheckNoOutput(skipNulls, ITERABLE_);
            CheckNoOutput(skipNulls, ITERABLE_NULL);
            CheckNoOutput(skipNulls, ITERABLE_NULL_NULL);
            CheckNoOutput(skipNulls, ITERABLE_FOUR_NULLS);
            CheckResult(skipNulls, ITERABLE_1, "1");
            CheckResult(skipNulls, ITERABLE_12, "1-2");
            CheckResult(skipNulls, ITERABLE_123, "1-2-3");
            CheckResult(skipNulls, ITERABLE_NULL_1, "1");
            CheckResult(skipNulls, ITERABLE_1_NULL, "1");
            CheckResult(skipNulls, ITERABLE_1_NULL_2, "1-2");
        }

        [TestMethod]
        public void TestUseForNull()
        {
            var zeroForNull = J.UseForNull("0");
            CheckNoOutput(zeroForNull, ITERABLE_);
            CheckResult(zeroForNull, ITERABLE_1, "1");
            CheckResult(zeroForNull, ITERABLE_12, "1-2");
            CheckResult(zeroForNull, ITERABLE_123, "1-2-3");
            CheckResult(zeroForNull, ITERABLE_NULL, "0");
            CheckResult(zeroForNull, ITERABLE_NULL_NULL, "0-0");
            CheckResult(zeroForNull, ITERABLE_NULL_1, "0-1");
            CheckResult(zeroForNull, ITERABLE_1_NULL, "1-0");
            CheckResult(zeroForNull, ITERABLE_1_NULL_2, "1-0-2");
            CheckResult(zeroForNull, ITERABLE_FOUR_NULLS, "0-0-0-0");
        }

        private static void CheckNoOutput(Joiner joiner, IList<string> parts)
        {
            Assert.AreEqual("", joiner.Join(parts));
            Assert.AreEqual("", joiner.Join(parts.GetEnumerator()));
        }

        private static void CheckResult(Joiner joiner, IList<string> parts, string expected)
        {
            Assert.AreEqual(expected, joiner.Join(parts));
            Assert.AreEqual(expected, joiner.Join(parts.GetEnumerator()));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Test_UseForNull_SkipNulls()
        {
            var j = Joiner.On("x").UseForNull("y");
            j = j.SkipNulls();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Test_SkipNulls_UseForNull()
        {
            var j = Joiner.On("x").SkipNulls();
            j = j.UseForNull("y");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Test_UseForNull_Twice()
        {
            var j = Joiner.On("x").UseForNull("y");
            j = j.UseForNull("y");
        }

        [TestMethod]
        public void TestMap()
        {
            var j = Joiner.On(";").WithKeyValueSeparator(":");
            Assert.AreEqual("", j.Join(new Dictionary<string, string>()));
           
            var mapWithNulls = new Dictionary<string, string>
            {
                {"a", null},
                {"b", null}
            };

            try
            {
                j.Join(mapWithNulls);
                Assert.Fail();
            }
            catch (NullReferenceException)
            {
            }

            Assert.AreEqual("a:00;b:00", j.UseForNull("00").Join(mapWithNulls));

            var mapInts = new Dictionary<int, int>
            {
                {1, 2},
                {3, 4},
                {5, 6}
            };
            var sb = new StringBuilder();
            j.AppendTo(sb, mapInts);
            Assert.AreEqual("1:2;3:4;5:6", sb.ToString());
        }
    }
}