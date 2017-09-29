using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

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
        /// <c>Splitter.on(',').split("foo,,bar")</c> returns an iterable containing
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
        /// <c>Splitter.On(CharMatcher.anyOf(";,")).split("foo,;bar,quux")</c>
        ///returns an iterable containing <c>["foo", "", "bar", "quux"]</c>.
        /// </summary>
        /// <param name="separatorMatcher">a <see cref="CharMatcher"/> that determines 
        /// whether a character is a separator</param>
        /// <returns>a splitter, with default settings, that uses this matcher</returns>
        public static Splitter On(CharMatcher separatorMatcher)
        {
            Preconditions.CheckNotNull(separatorMatcher);
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
            Preconditions.CheckArgument(separator.Length != 0, "The separator may not be the empty string.");
            if (separator.Length == 1)
            {
                return On(separator[0]);
            }
            return new Splitter(ByStringSplitterStrategy(separator));
        }


        public static Splitter On(Regex separatorPattern)
        {
            Preconditions.CheckArgument(
                !separatorPattern.IsMatch(""),
                "The pattern may not match the empty string: %s",
                separatorPattern);
            return new Splitter(ByRegexSplitterStrategy(separatorPattern));
        }

        public static Splitter OnPattern(string separatorPattern)
        {
            return On(new Regex(separatorPattern));
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
            Preconditions.CheckNotNull(trimmer);
            return new Splitter(enumerableProducer, omitEmptyStrings, trimmer, limit);
        }

        public IEnumerable<string> split(string sequence)
        {
            Preconditions.CheckNotNull(sequence);
            return MakeSplittingEnumerable(sequence);
        }

        private IEnumerable<string> MakeSplittingEnumerable(string sequence)
        {
            return enumerableProducer.Invoke(this, sequence);
        }

        public List<string> SplitToList(string sequence)
        {
            Preconditions.CheckNotNull(sequence);
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