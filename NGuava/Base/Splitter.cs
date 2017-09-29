using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static NGuava.Base.Preconditions;

namespace NGuava.Base
{
    public class Splitter
    {
        private readonly CharMatcher trimmer;
        private readonly bool omitEmptyStrings;
        private readonly SplittingEnumerableProducer enumerableProducer;
        private readonly int limit;

        private delegate IEnumerable<string> SplittingEnumerableProducer(Splitter splitter, string toSplit);

        private Splitter(SplittingEnumerableProducer enumerableProducer) : this(enumerableProducer, false, CharMatcher.None, int.MaxValue)
        {
        }

        private Splitter(SplittingEnumerableProducer enumerableProducer, bool omitEmptyStrings,
            CharMatcher trimmer, int limit)
        {
            this.enumerableProducer = enumerableProducer;
            this.omitEmptyStrings = omitEmptyStrings;
            this.trimmer = trimmer;
            this.limit = limit;
        }

        /// <summary>
        /// Returns a splitter that uses the given single-character separator. For example,
        /// <code>Splitter.On(',').split("foo,,bar")</code> returns an iterable containing
        /// <c>["foo", "", "bar"]</c>
        /// </summary>
        /// <param name="separator">the character to recognize as a separator</param>
        /// <returns>a splitter, with default settings, that recognizes that separator</returns>
        public static Splitter On(char separator)
        {
            return On(CharMatcher.isChar(separator));
        }

        /// <summary>
        /// Returns a splitter that considers any single character matched by the
        /// given <c>CharMatcher</c> to be a separator.For example, 
        /// <code>Splitter.On(CharMatcher.anyOf(";,")).split("foo,;bar,quux")</code>
        ///returns an iterable containing <c>["foo", "", "bar", "quux"]</c>.
        /// </summary>
        /// <param name="separatorMatcher">a <see cref="CharMatcher"/> that determines 
        /// whether a character is a separator</param>
        /// <returns>a splitter, with default settings, that uses this matcher</returns>
        public static Splitter On(CharMatcher separatorMatcher)
        {
            CheckNotNull(separatorMatcher);
            return new Splitter(ByCharMatcherSplitterStrategy(separatorMatcher));
        }

        /// <summary>
        /// Returns a splitter that uses the given fixed string as a separator. For example,
        /// <c>Splitter.on(", ").split("foo, bar,baz")</c>
        /// returns an iterable containing <c>["foo", "bar,baz"]</c>.
        /// </summary>
        /// <param name="separator">the literal, nonempty string to recognize as a separator</param>
        /// <returns>a splitter, with default settings, that recognizes that separator</returns>
        public static Splitter On(string separator)
        {
            CheckArgument(separator.Length != 0, "The separator may not be the empty string.");
            if (separator.Length == 1)
            {
                return On(separator[0]);
            }
            return new Splitter(ByStringSplitterStrategy(separator));
        }

        /// <summary>
        /// Returns a splitter that considers any subsequence matching <paramref name="separatorPattern"/> to be a separator.
        /// For example, <code>Splitter.On(new Regex("\r?\n")).split(entireFile)</code> splits a string
        /// into lines whether it uses DOS-style or UNIX-style line terminators.
        /// </summary>
        /// <param name="separatorPattern">
        /// the pattern that determines whether a subsequence is a separator. This
        /// pattern may not match the empty string.
        /// </param>
        /// <returns>a splitter, with default settings, that uses this pattern</returns>
        /// <exception cref="ArgumentException">if <paramref name="separatorPattern"/> matches the empty string</exception>
        public static Splitter On(Regex separatorPattern)
        {
            CheckArgument(
                !separatorPattern.IsMatch(""),
                "The pattern may not match the empty string: %s",
                separatorPattern);
            return new Splitter(ByRegexSplitterStrategy(separatorPattern));
        }

        /// <summary>
        /// Returns a splitter that considers any subsequence matching a given pattern (regular expression)
        /// to be a separator. For example, <code>Splitter.OnPattern("\r?\n").split(entireFile)</code> splits a
        /// string into lines whether it uses DOS-style or UNIX-style line terminators. This is equivalent
        /// to <code>Splitter.On(new Regex(pattern))</code>.
        /// </summary>
        /// <param name="separatorPattern">
        /// the pattern that determines whether a subsequence is a separator. This
        /// pattern may not match the empty string.
        /// </param>
        /// <returns>a splitter, with default settings, that uses this pattern</returns>
        /// <exception cref="ArgumentException">if <paramref name="separatorPattern"/> matches the empty string or is a
        /// malformed expression
        /// </exception>
        public static Splitter OnPattern(string separatorPattern)
        {
            return On(new Regex(separatorPattern));
        }

        /// <summary>
        /// Returns a splitter that divides strings into pieces of the given length. For example,
        /// <code>Splitter.fixedLength(2).split("abcde")</code> returns an iterable containing
        /// <c>["ab", "cd", "e"]</c>. The last piece can be smaller than <paramref name="length"/> but will never be
        /// empty.
        /// 
        /// <p><b>Exception:</b> for consistency with separator-based splitters, <c>split("")</c>
        /// does not yield an empty iterable, but an iterable containing <c>""</c>. This is the
        /// only case in which <c>Iterables.size(split(input))</c> does not equal
        /// <c>IntMath.divide(input.length(), length, CEILING)</c>. To avoid this behavior, use
        /// <see cref="OmitEmptyStrings"/>.</p>
        /// </summary>
        /// <param name="length">length the desired length of pieces after splitting, a positive integer</param>
        /// <returns>a splitter, with default settings, that can split into fixed sized pieces</returns>
        /// <exception cref="ArgumentException">if <paramref name="length"/> is zero or negative</exception>
        public static Splitter FixedLength(int length) {
            CheckArgument(length > 0, "The length may not be less than 1");

            return new Splitter(ByFixedLengthSplitterStrategy(length));
        }

        public Splitter OmitEmptyStrings()
        {
            return new Splitter(enumerableProducer, true, trimmer, limit);
        }

        public Splitter TrimResults()
        {
            return TrimResults(CharMatcher.Whitespace);
        }

        public Splitter TrimResults(CharMatcher trimmer)
        {
            CheckNotNull(trimmer);
            return new Splitter(enumerableProducer, omitEmptyStrings, trimmer, limit);
        }

        public IEnumerable<string> split(string sequence)
        {
            CheckNotNull(sequence);
            return MakeSplittingEnumerable(sequence);
        }

        private IEnumerable<string> MakeSplittingEnumerable(string sequence)
        {
            return enumerableProducer.Invoke(this, sequence);
        }

        public List<string> SplitToList(string sequence)
        {
            CheckNotNull(sequence);
            var enumerable = MakeSplittingEnumerable(sequence);
            var result = new List<string>(enumerable);
            return result;
        }

        private static SplittingEnumerableProducer ByCharMatcherSplitterStrategy(CharMatcher separatorMatcher)
        {
            return (splitter, toSplit) => new ByCharMatcherSplittingEnumerable(splitter, toSplit, separatorMatcher);
        }

        private static SplittingEnumerableProducer ByStringSplitterStrategy(string separatorMatcher)
        {
            return (splitter, toSplit) => new ByStringSplittingEnumerable(splitter, toSplit, separatorMatcher);
        }

        private static SplittingEnumerableProducer ByRegexSplitterStrategy(Regex separatorMatcher)
        {
            return (splitter, toSplit) => new ByRegexSplittingEnumerable(splitter, toSplit, separatorMatcher);
        }
        
        private static SplittingEnumerableProducer ByFixedLengthSplitterStrategy(int length)
        {
            return (splitter, toSplit) => new ByFixedLenghtSplittingEnumerable(splitter, toSplit, length);
        }


        private class ByCharMatcherSplittingEnumerable : SplittingEnumerable
        {
            private readonly CharMatcher separatorMatcher;

            public ByCharMatcherSplittingEnumerable(Splitter splitter,
                string toSplit,
                CharMatcher separatorMatcher) : base(splitter, toSplit)
            {
                this.separatorMatcher = separatorMatcher;
            }

            internal override int SeparatorStart(int start)
            {
                return separatorMatcher.IndexIn(ToSplit, start);
            }

            internal override int SeparatorEnd(int separatorPosition)
            {
                return separatorPosition + 1;
            }
        }


        private class ByStringSplittingEnumerable : SplittingEnumerable
        {
            private readonly string separator;

            public ByStringSplittingEnumerable(Splitter splitter,
                string toSplit,
                string separator) : base(splitter, toSplit)
            {
                this.separator = separator;
            }

            internal override int SeparatorStart(int start)
            {
                var separatorLength = separator.Length;

                for (int p = start, last = ToSplit.Length - separatorLength; p <= last; p++)
                {
                    var advanceFirstSeparatorNotMatched = false;
                    for (var i = 0; i < separatorLength; i++)
                    {
                        if (ToSplit[i + p] != separator[i])
                        {
                            advanceFirstSeparatorNotMatched = true;
                            break;
                        }
                    }
                    if (advanceFirstSeparatorNotMatched)
                    {
                        continue;
                    }
                    return p;
                }
                return -1;
            }

            internal override int SeparatorEnd(int separatorPosition)
            {
                return separatorPosition + separator.Length;
            }
        }


        private class ByRegexSplittingEnumerable : SplittingEnumerable
        {
            private readonly Regex separatorPattern;
            private Match currentMatch;

            public ByRegexSplittingEnumerable(Splitter splitter,
                string toSplit,
                Regex separatorPattern) : base(splitter, toSplit)
            {
                this.separatorPattern = separatorPattern;
            }

            internal override int SeparatorStart(int start)
            {
                var matches = separatorPattern.Matches(ToSplit, start);
                foreach (Match match in matches)
                {
                    //if there is a match, return first index
                    currentMatch = match;
                    return currentMatch.Index;
                }
                return -1;
            }

            internal override int SeparatorEnd(int separatorPosition)
            {
                return currentMatch.Index + currentMatch.Length;
            }
        }
        
        
        private class ByFixedLenghtSplittingEnumerable : SplittingEnumerable
        {
            private readonly int length;

            public ByFixedLenghtSplittingEnumerable(Splitter splitter,
                string toSplit,
                int length) : base(splitter, toSplit)
            {
                this.length = length;
            }

            
            internal override int SeparatorStart(int start) {
                var nextChunkStart = start + length;
                return nextChunkStart < ToSplit.Length ? nextChunkStart : -1;
            }

            
            internal override int SeparatorEnd(int separatorPosition) {
                return separatorPosition;
            }
        }


        private abstract class SplittingEnumerable : IEnumerable<string>
        {
            internal readonly string ToSplit;
            private readonly CharMatcher trimmer;
            private readonly bool omitEmptyStrings;
            private readonly int spitterLimit;

            /**
             * Returns the first index in {@code toSplit} at or after {@code start}
             * that contains the separator.
             */
            internal abstract int SeparatorStart(int start);

            /**
             * Returns the first index in {@code toSplit} after {@code
             * separatorPosition} that does not contain a separator. This method is only
             * invoked after a call to {@code SeparatorStart}.
             */
            internal abstract int SeparatorEnd(int separatorPosition);

            protected SplittingEnumerable(Splitter splitter, string toSplit)
            {
                this.trimmer = splitter.trimmer;
                this.omitEmptyStrings = splitter.omitEmptyStrings;
                this.spitterLimit = splitter.limit;
                this.ToSplit = toSplit;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<string> GetEnumerator()
            {
                int offset = 0;
                int limit = spitterLimit;
                int nextStart = offset;
                /*
                 * The returned string will be from the end of the last match to the
                 * beginning of the next one. nextStart is the start position of the
                 * returned substring, while offset is the place to start looking for a
                 * separator.
                 */

                while (offset != -1)
                {
                    int start = nextStart;
                    int end;

                    int separatorPosition = SeparatorStart(offset);
                    if (separatorPosition == -1)
                    {
                        end = ToSplit.Length;
                        offset = -1;
                    }
                    else
                    {
                        end = separatorPosition;
                        offset = SeparatorEnd(separatorPosition);
                    }
                    if (offset == nextStart)
                    {
                        /*
                         * This occurs when some pattern has an empty match, even if it
                         * doesn't match the empty string -- for example, if it requires
                         * lookahead or the like. The offset must be increased to look for
                         * separators beyond this point, without changing the start position
                         * of the next returned substring -- so nextStart stays the same.
                         */
                        offset++;
                        if (offset > ToSplit.Length)
                        {
                            offset = -1;
                        }
                        continue;
                    }

                    while (start < end && trimmer.matches(ToSplit[start]))
                    {
                        start++;
                    }
                    while (end > start && trimmer.matches(ToSplit[end - 1]))
                    {
                        end--;
                    }

                    if (omitEmptyStrings && start == end)
                    {
                        // Don't include the (unused) separator in next split string.
                        nextStart = offset;
                        continue;
                    }

                    if (limit == 1)
                    {
                        // The limit has been reached, return the rest of the string as the
                        // final item.  This is tested after empty string removal so that
                        // empty strings do not count towards the limit.
                        end = ToSplit.Length;
                        offset = -1;
                        // Since we may have changed the end, we need to trim it again.
                        while (end > start && trimmer.matches(ToSplit[end - 1]))
                        {
                            end--;
                        }
                    }
                    else
                    {
                        limit--;
                    }

                    yield return ToSplit.Substring(start, end - start);
                    nextStart = offset;
                }
            }

            public override string ToString()
            {
                return Joiner.On(", ")
                    .AppendTo(new StringBuilder().Append('['), this)
                    .Append(']')
                    .ToString();
            }
        }
    }
}