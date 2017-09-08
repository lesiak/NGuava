using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NGuava.Base
{
    public class Splitter
    {
        private readonly CharMatcher trimmer;
        private readonly bool omitEmptyStrings;
        private readonly ISplittingStrategy strategy;
        private readonly int limit;

        private interface ISplittingStrategy
        {
            IEnumerable<string> MakeEnumerable(Splitter splitter, string toSplit);
        }
        
        private Splitter(ISplittingStrategy strategy) : this(strategy, false, CharMatcher.NONE, int.MaxValue) {
            
        }

        private Splitter(ISplittingStrategy strategy, bool omitEmptyStrings,
            CharMatcher trimmer, int limit) {
            this.strategy = strategy;
            this.omitEmptyStrings = omitEmptyStrings;
            this.trimmer = trimmer;
            this.limit = limit;
        }

        /**
   * Returns a splitter that uses the given single-character separator. For
   * example, {@code Splitter.On(',').split("foo,,bar")} returns an iterable
   * containing {@code ["foo", "", "bar"]}.
   *
   * @param separator the character to recognize as a separator
   * @return a splitter, with default settings, that recognizes that separator
   */
        public static Splitter On(char separator) {
            return On(CharMatcher.isChar(separator));
        }
        
        
        /**
   * Returns a splitter that considers any single character matched by the
   * given {@code CharMatcher} to be a separator. For example, {@code
   * Splitter.On(CharMatcher.anyOf(";,")).split("foo,;bar,quux")} returns an
   * iterable containing {@code ["foo", "", "bar", "quux"]}.
   *
   * @param separatorMatcher a {@link CharMatcher} that determines whether a
   *     character is a separator
   * @return a splitter, with default settings, that uses this matcher
   */
        public static Splitter On(CharMatcher separatorMatcher) {
            Preconditions.CheckNotNull(separatorMatcher);

            return new Splitter(new CharMatcherSplitterStrategy(separatorMatcher));
        }

        public Splitter OmitEmptyStrings()
        {
            return new Splitter(strategy, true, trimmer, limit);
        }

        public IEnumerable<string> split(string sequence) {
            Preconditions.CheckNotNull(sequence);
            return MakesSlittingEnumerable(sequence); ;
        }
        
        private IEnumerable<string> MakesSlittingEnumerable(string sequence) {
            return strategy.MakeEnumerable(this, sequence);
        }
        

        class CharMatcherSplitterStrategy : ISplittingStrategy
        {
            private readonly CharMatcher separatorMatcher;

            public CharMatcherSplitterStrategy(CharMatcher separatorMatcher)
            {
                this.separatorMatcher = separatorMatcher;
            }

            public IEnumerable<string> MakeEnumerable(Splitter splitter, string toSplit)
            {
                return new CharMatcherSplittingEnumerable(splitter, toSplit, separatorMatcher);
            }

            class CharMatcherSplittingEnumerable : SplittingEnumerable
            { 
                private readonly CharMatcher separatorMatcher;

                public CharMatcherSplittingEnumerable(Splitter splitter, 
                    string toSplit, 
                    CharMatcher separatorMatcher) : base(splitter, toSplit)
                {
                    this.separatorMatcher = separatorMatcher;
                }

                internal override int separatorStart(int start) {
                    return separatorMatcher.indexIn(toSplit, start);
                }

                internal override int separatorEnd(int separatorPosition) {
                    return separatorPosition + 1;
                }
            }
        }


        private abstract class SplittingEnumerable : IEnumerable<string>
        {
            internal readonly string toSplit;
            private readonly CharMatcher trimmer;
            private readonly bool omitEmptyStrings;

            /**
             * Returns the first index in {@code toSplit} at or after {@code start}
             * that contains the separator.
             */
            internal abstract int separatorStart(int start);

            /**
             * Returns the first index in {@code toSplit} after {@code
             * separatorPosition} that does not contain a separator. This method is only
             * invoked after a call to {@code separatorStart}.
             */
            internal abstract int separatorEnd(int separatorPosition);

            int offset = 0;
            int limit;

            protected SplittingEnumerable(Splitter splitter, string toSplit)
            {
                this.trimmer = splitter.trimmer;
                this.omitEmptyStrings = splitter.omitEmptyStrings;
                this.limit = splitter.limit;
                this.toSplit = toSplit;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<string> GetEnumerator()
            {
                /*
                 * The returned string will be from the end of the last match to the
                 * beginning of the next one. nextStart is the start position of the
                 * returned substring, while offset is the place to start looking for a
                 * separator.
                 */

                while (offset != -1)
                {
                    int nextStart = offset;
                    int start = nextStart;
                    int end;

                    int separatorPosition = separatorStart(offset);
                    if (separatorPosition == -1)
                    {
                        end = toSplit.Length;
                        offset = -1;
                    }
                    else
                    {
                        end = separatorPosition;
                        offset = separatorEnd(separatorPosition);
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
                        if (offset >= toSplit.Length)
                        {
                            offset = -1;
                        }
                        continue;
                    }

                    while (start < end && trimmer.matches(toSplit[start]))
                    {
                        start++;
                    }
                    while (end > start && trimmer.matches(toSplit[end - 1]))
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
                        end = toSplit.Length;
                        offset = -1;
                        // Since we may have changed the end, we need to trim it again.
                        while (end > start && trimmer.matches(toSplit[end - 1]))
                        {
                            end--;
                        }
                    }
                    else
                    {
                        limit--;
                    }

                    yield return toSplit.Substring(start, end - start);
                }
            }
        }
    }
}