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
            letters.Should().ContainExactlyInOrder("a", "b", "c");
        }

        [TestMethod]
        public void TestCharacterSimpleSplitToList()
        {
            const string simple = "a,b,c";
            var letters = COMMA_SPLITTER.SplitToList(simple);
            letters.Should().ContainExactlyInOrder("a", "b", "c");
        }

        [TestMethod]
        public void TestToString()
        {
            COMMA_SPLITTER.split("").ToString().Should().Be("[]");
            COMMA_SPLITTER.split("a,b,c").ToString().Should().Be("[a, b, c]");
            Splitter.On(", ").split("yam, bam, jam, ham").ToString().Should().Be("[yam, bam, jam, ham]");
        }

        [TestMethod]
        public void TestSplitterIsUsableAfterToString()
        {
            var letters = Splitter.On(',').split("a,b,c");
            letters.ToString().Should().Be("[a, b, c]");
            letters.Should().ContainExactlyInOrder("a", "b", "c");
        }

        [TestMethod]
        public void TestCharacterSimpleSplitWithNoDelimiter()
        {
            const string simple = "a,b,c";
            var letters = Splitter.On('.').split(simple);
            letters.Should().ContainExactlyInOrder("a,b,c");
        }

        [TestMethod]
        public void TestCharacterSplitWithDoubleDelimiter()
        {
            const string doubled = "a,,b,c";
            var letters = COMMA_SPLITTER.split(doubled);
            letters.Should().ContainExactlyInOrder("a", "", "b", "c");
        }

        [TestMethod]
        public void TestCharacterSplitWithDoubleDelimiterAndSpace()
        {
            const string doubled = "a,, b,c";
            var letters = COMMA_SPLITTER.split(doubled);
            letters.Should().ContainExactlyInOrder("a", "", " b", "c");
        }

        [TestMethod]
        public void TestCharacterSplitWithTrailingDelimiter()
        {
            const string trailing = "a,b,c,";
            var letters = COMMA_SPLITTER.split(trailing);
            letters.Should().ContainExactlyInOrder("a", "b", "c", "");
        }

        [TestMethod]
        public void TestCharacterSplitWithLeadingDelimiter()
        {
            const string leading = ",a,b,c";
            var letters = COMMA_SPLITTER.split(leading);
            letters.Should().ContainExactlyInOrder("", "a", "b", "c");
        }

        [TestMethod]
        public void TestCharacterSplitWithMultitpleLetters()
        {
            var testCharacteringMotto = Splitter.On('-').split(
                "Testing-rocks-Debugging-sucks");
            testCharacteringMotto.Should().ContainExactlyInOrder("Testing", "rocks", "Debugging", "sucks");
        }

        [TestMethod]
        public void TestCharacterSplitWithMatcherDelimiter()
        {
            var testCharacteringMotto = Splitter
                .On(CharMatcher.Whitespace)
                .split("Testing\nrocks\tDebugging sucks");
            testCharacteringMotto.Should()
                .ContainExactlyInOrder("Testing", "rocks", "Debugging", "sucks");
        }

        [TestMethod]
        public void TestCharacterSplitWithDoubleDelimiterOmitEmptyStrings()
        {
            const string doubled = "a..b.c";
            var letters = Splitter.On('.')
                .OmitEmptyStrings().split(doubled);
            letters.Should().ContainExactlyInOrder("a", "b", "c");
        }

        [TestMethod]
        public void TestCharacterSplitEmptyToken()
        {
            const string emptyToken = "a. .c";
            var letters = Splitter.On('.').TrimResults()
                .split(emptyToken);
            letters.Should().ContainExactlyInOrder("a", "", "c");
        }

        [TestMethod]
        public void TestCharacterSplitEmptyTokenOmitEmptyStrings()
        {
            const string emptyToken = "a. .c";
            var letters = Splitter.On('.')
                .OmitEmptyStrings().TrimResults().split(emptyToken);
            letters.Should().ContainExactlyInOrder("a", "c");
        }

        [TestMethod]
        public void TestCharacterSplitOnEmptyString()
        {
            var nothing = Splitter.On('.').split("");
            nothing.Should().ContainExactlyInOrder("");
        }

        [TestMethod]
        public void TestCharacterSplitOnEmptyStringOmitEmptyStrings()
        {
            Splitter.On('.').OmitEmptyStrings().split("").Should().BeEmpty();
        }

        [TestMethod]
        public void TestCharacterSplitOnOnlyDelimiter()
        {
            var blankblank = Splitter.On('.').split(".");
            blankblank.Should().ContainExactlyInOrder("", "");
        }

        [TestMethod]
        public void TestCharacterSplitOnOnlyDelimitersOmitEmptyStrings()
        {
            var empty = Splitter.On('.').OmitEmptyStrings().split("...");
            empty.Should().BeEmpty();
        }

        [TestMethod]
        public void TestCharacterSplitWithTrim()
        {
            const string jacksons = "arfo(Marlon)aorf, (Michael)orfa, afro(Jackie)orfa, "
                                    + "ofar(Jemaine), aff(Tito)";
            var family = COMMA_SPLITTER
                .TrimResults(CharMatcher.anyOf("afro").Or(CharMatcher.Whitespace))
                .split(jacksons);
            family.Should().ContainExactlyInOrder(
                "(Marlon)",
                "(Michael)",
                "(Jackie)",
                "(Jemaine)",
                "(Tito)");
        }

        [TestMethod]
        public void TestStringSimpleSplit()
        {
            const string simple = "a,b,c";
            var letters = Splitter.On(",").split(simple);
            letters.Should().ContainExactlyInOrder("a", "b", "c");
        }

        [TestMethod]
        public void TestStringSimpleSplitWithNoDelimiter()
        {
            const string simple = "a,b,c";
            var letters = Splitter.On(".").split(simple);
            letters.Should().ContainExactlyInOrder("a,b,c");
        }

        [TestMethod]
        public void TestStringSplitWithDoubleDelimiter()
        {
            const string doubled = "a,,b,c";
            var letters = Splitter.On(",").split(doubled);
            letters.Should().ContainExactlyInOrder("a", "", "b", "c");
        }

        [TestMethod]
        public void TestStringSplitWithDoubleDelimiterAndSpace()
        {
            const string doubled = "a,, b,c";
            var letters = Splitter.On(",").split(doubled);
            letters.Should().ContainExactlyInOrder("a", "", " b", "c");
        }

        [TestMethod]
        public void TestStringSplitWithTrailingDelimiter()
        {
            const string trailing = "a,b,c,";
            var letters = Splitter.On(",").split(trailing);
            letters.Should().ContainExactlyInOrder("a", "b", "c", "");
        }

        [TestMethod]
        public void TestStringSplitWithLeadingDelimiter()
        {
            const string leading = ",a,b,c";
            var letters = Splitter.On(",").split(leading);
            letters.Should().ContainExactlyInOrder("", "a", "b", "c");
        }

        [TestMethod]
        public void TestStringSplitWithMultipleLetters()
        {
            var testStringingMotto = Splitter.On("-").split(
                "Testing-rocks-Debugging-sucks");
            testStringingMotto.Should().ContainExactlyInOrder(
                "Testing",
                "rocks",
                "Debugging",
                "sucks");
        }


        [TestMethod]
        public void TestStringSplitWithDoubleDelimiterOmitEmptyStrings()
        {
            const string doubled = "a..b.c";
            var letters = Splitter.On(".")
                .OmitEmptyStrings().split(doubled);
            letters.Should().ContainExactlyInOrder("a", "b", "c");
        }

        [TestMethod]
        public void TestStringSplitEmptyToken()
        {
            const string emptyToken = "a. .c";
            var letters = Splitter.On(".").TrimResults()
                .split(emptyToken);
            letters.Should().ContainExactlyInOrder("a", "", "c");
        }

        [TestMethod]
        public void TestStringSplitEmptyTokenOmitEmptyStrings()
        {
            const string emptyToken = "a. .c";
            var letters = Splitter.On(".")
                .OmitEmptyStrings().TrimResults().split(emptyToken);
            letters.Should().ContainExactlyInOrder("a", "c");
        }

        [TestMethod]
        public void TestStringSplitWithLongDelimiter()
        {
            const string longDelimiter = "a, b, c";
            var letters = Splitter.On(", ").split(longDelimiter);
            letters.Should().ContainExactlyInOrder("a", "b", "c");
        }

        [TestMethod]
        public void TestStringSplitWithLongLeadingDelimiter()
        {
            const string longDelimiter = ", a, b, c";
            var letters = Splitter.On(", ").split(longDelimiter);
            letters.Should().ContainExactlyInOrder("", "a", "b", "c");
        }

        [TestMethod]
        public void TestStringSplitWithLongTrailingDelimiter()
        {
            const string longDelimiter = "a, b, c, ";
            var letters = Splitter.On(", ").split(longDelimiter);
            letters.Should().ContainExactlyInOrder("a", "b", "c", "");
        }

        [TestMethod]
        public void TestStringSplitWithDelimiterSubstringInValue()
        {
            const string fourCommasAndFourSpaces = ",,,,    ";
            var threeCommasThenThreeSpaces = Splitter.On(", ").split(
                fourCommasAndFourSpaces);
            threeCommasThenThreeSpaces.Should().ContainExactlyInOrder(",,,", "   ");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestStringSplitWithEmptyString()
        {
            Splitter.On("");
        }

        [TestMethod]
        public void TestStringSplitOnEmptyString()
        {
            var notMuch = Splitter.On(".").split("");
            notMuch.Should().ContainExactlyInOrder("");
        }

        [TestMethod]
        public void TestStringSplitOnEmptyStringOmitEmptyString()
        {
            Splitter.On(".").OmitEmptyStrings().split("").Should().BeEmpty();
        }

        [TestMethod]
        public void TestStringSplitOnOnlyDelimiter()
        {
            var blankblank = Splitter.On(".").split(".");
            blankblank.Should().ContainExactlyInOrder("", "");
        }

        [TestMethod]
        public void TestStringSplitOnOnlyDelimitersOmitEmptyStrings()
        {
            var empty = Splitter.On(".").OmitEmptyStrings().split("...");
            empty.Should().BeEmpty();
        }
    }
}