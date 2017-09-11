using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NGuava.Base;

namespace NGuava.Tests.Base
{
    [TestClass]
    public class SplitterTest
    {
        private static readonly Splitter COMMA_SPLITTER = Splitter.On(',');

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void TestSplitNullString()
        {
            COMMA_SPLITTER.split(null);
        }

        [TestMethod]
        public void TestCharacterSimpleSplit()
        {
            const string simple = "a,b,c";
            var letters = COMMA_SPLITTER.split(simple);
            letters.Should().BeEquivalentTo(new List<string> {"a", "b", "c"},
                options => options.WithStrictOrdering());
        }

        [TestMethod]
        [Ignore]
        public void TestCharacterSimpleSplitToList()
        {
            String simple = "a,b,c";
           // List<String> letters = COMMA_SPLITTER.SplitToList(simple);
           // assertThat(letters).containsExactly("a", "b", "c").inOrder();
        }

        [TestMethod]
        [Ignore]
        public void testToString()
        {
            //assertEquals("[]", Splitter.on(',').split("").toString());
            //assertEquals("[a, b, c]", Splitter.on(',').split("a,b,c").toString());
            //assertEquals("[yam, bam, jam, ham]", Splitter.on(", ").split("yam, bam, jam, ham").toString());
        }

        [TestMethod]
        public void TestCharacterSimpleSplitWithNoDelimiter()
        {
            const string simple = "a,b,c";
            var letters = Splitter.On('.').split(simple);
            letters.Should().BeEquivalentTo(new List<string> { "a,b,c" },
                options => options.WithStrictOrdering());
        }

        [TestMethod]
        public void TestCharacterSplitWithDoubleDelimiter()
        {
            const string doubled = "a,,b,c";
            var letters = COMMA_SPLITTER.split(doubled);
            letters.Should().BeEquivalentTo(new List<string> { "a", "", "b", "c" },
                options => options.WithStrictOrdering());
        }

        [TestMethod]
        public void TestCharacterSplitWithDoubleDelimiterAndSpace()
        {
            const string doubled = "a,, b,c";
            var letters = COMMA_SPLITTER.split(doubled);
            letters.Should().BeEquivalentTo(new List<string> { "a", "", " b", "c" },
                options => options.WithStrictOrdering());
        }

        [TestMethod]
        public void TestCharacterSplitWithTrailingDelimiter()
        {
            const string trailing = "a,b,c,";
            var letters = COMMA_SPLITTER.split(trailing);
            letters.Should().BeEquivalentTo(new List<string> { "a", "b", "c", "" },
                options => options.WithStrictOrdering());
        }

        [TestMethod]
        public void TestCharacterSplitWithLeadingDelimiter()
        {
            const string leading = ",a,b,c";
            var letters = COMMA_SPLITTER.split(leading);
            letters.Should().BeEquivalentTo(new List<string> { "", "a", "b", "c" },
                options => options.WithStrictOrdering());
        }

        [TestMethod]
        public void TestCharacterSplitWithMulitpleLetters()
        {
            var testCharacteringMotto = Splitter.On('-').split(
                "Testing-rocks-Debugging-sucks");
            testCharacteringMotto.Should()
                .BeEquivalentTo(new List<string> { "Testing", "rocks", "Debugging", "sucks" },
                    options => options.WithStrictOrdering());
        }

    }
}