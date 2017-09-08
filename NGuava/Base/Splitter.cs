using System.Collections;

namespace NGuava.Base
{
    public class Splitter
    {
        private readonly CharMatcher trimmer;
        private readonly bool omitEmptyStrings;
        private readonly Strategy strategy;
        private readonly int limit;

        private interface Strategy
        {
            IEnumerator iterator(Splitter splitter, string toSplit);
        }

        private abstract class SplittingIterator : AbstractIterator<string>
        {
            readonly string toSplit;
            readonly CharMatcher trimmer;
            readonly bool omitEmptyStrings;

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

            protected SplittingIterator(Splitter splitter, string toSplit)
            {
                this.trimmer = splitter.trimmer;
                this.omitEmptyStrings = splitter.omitEmptyStrings;
                this.limit = splitter.limit;
                this.toSplit = toSplit;
            }

            protected override string computeNext()
            {
                /*
                 * The returned string will be from the end of the last match to the
                 * beginning of the next one. nextStart is the start position of the
                 * returned substring, while offset is the place to start looking for a
                 * separator.
                 */
                int nextStart = offset;
                while (offset != -1)
                {
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

                    return toSplit.Substring(start, end);
                }
                return endOfData();
            }
        }
    }
}