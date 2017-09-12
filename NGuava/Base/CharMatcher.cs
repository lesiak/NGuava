using System;
using System.Runtime.InteropServices;
using System.Text;
using static NGuava.Base.Preconditions;

namespace NGuava.Base
{
    public abstract class CharMatcher
    {
        #region Constant matcher properties

        public static CharMatcher Any => AnyMatcher.INSTANCE;

        /// <summary>
        /// Matches no characters.
        /// </summary>
        /// <returns></returns>
        public static CharMatcher None => NoneMatcher.INSTANCE;

        /// <summary>
        /// Determines whether a character is whitespace according to the latest Unicode standard, as
        /// illustrated
        /// <a href = "http://unicode.org/cldr/utility/list-unicodeset.jsp?a=%5Cp%7Bwhitespace%7D" > here </ a >.
        /// This is not the same definition used by other Java APIs. (See a
        /// <a href = "https://goo.gl/Y6SLWx" > comparison of several definitions of
        /// "whitespace"</a>.)
        ///
        /// <p><b>Note:</b> as the Unicode definition evolves, we will modify this matcher to keep it up to
        /// date.</p>
        /// </summary>
        public static CharMatcher Whitespace => WhitespaceMatcher.INSTANCE;

        #endregion

        #region Static factories

        /// <summary>
        /// Returns a <c>char</c> matcher that matches only one specified character.
        /// </summary>
        public static CharMatcher isChar(char match)
        {
            var description = "CharMatcher.is('" + showCharacter(match) + "')";
            //var description = "Aaa";
            return new IsMatcher(match, description);
        }

        /// <summary>
        /// Returns a <c>char</c> matcher that matches any character present in the given character
        /// sequence.
        /// </summary>
        public static CharMatcher anyOf(string sequence)
        {
            switch (sequence.Length)
            {
                case 0:
                    return None;
                case 1:
                    return isChar(sequence[0]);
                case 2:
                    return isEither(sequence[0], sequence[1]);
                default:
                    // TODO(user): is it potentially worth just going ahead and building a precomputed matcher?
                    return new AnyOfMatcher(sequence);
            }
        }

        #endregion

        #region Abstract methods

        public abstract bool matches(char c);

        #endregion

        #region Non-static factories

        /// <summary>
        /// Returns a matcher that matches any character matched by both this matcher and <c>other</c>.
        /// </summary>
        public virtual CharMatcher And(CharMatcher other)
        {
            return new AndMatcher(this, other);
        }

        /// <summary>
        /// Returns a matcher that matches any character matched by either this matcher or <c>other</c>.
        /// </summary>
        public virtual CharMatcher Or(CharMatcher other)
        {
            return new OrMatcher(this, other);
        }

        #endregion

        #region Text processing routines

        /// <summary>
        /// Returns <c>true</c> if a character sequence contains at least one matching character.
        /// Equivalent to <c>!MatchesNoneOf(sequence)</c>
        /// <para>The default implementation iterates over the sequence, invoking <see cref="matches"/> for each
        /// character, until this returns <c>true</c> or the end is reached.</para>
        /// </summary>
        /// <param name="sequence">the character sequence to examine, possibly empty</param>
        /// <returns><c>true</c> if this matcher matches at least one character in the sequence</returns>
        public virtual bool MatchesAnyOf(string sequence)
        {
            return !MatchesNoneOf(sequence);
        }

        /// <summary>
        /// Returns <c>true</c> if a character sequence contains only matching characters.
        /// <para>The default implementation iterates over the sequence, invoking <see cref="matches"/> for each
        /// character, until this returns <c>false</c> or the end is reached.</para>
        /// </summary>
        /// <param name="sequence">the character sequence to examine, possibly empty</param>
        /// <returns><c>true</c> if this matcher matches every character in the sequence, including when
        /// the sequence is empty</returns>
        public virtual bool MatchesAllOf(string sequence)
        {
            for (int i = sequence.Length - 1; i >= 0; i--)
            {
                if (!matches(sequence[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns {@code true} if a character sequence contains no matching characters. Equivalent to
        /// <c>!MatchesAnyOf(sequence)</c>
        /// <para>The default implementation iterates over the sequence, invoking <see cref="matches" /> for each
        /// character, until this returns <c>true</c> or the end is reached.</para>
        /// </summary>
        /// <param name="sequence">the character sequence to examine, possibly empty</param>
        /// <returns><c>true</c> if this matcher matches no characters in the sequence, including when
        /// the sequence is empty</returns>
        public virtual bool MatchesNoneOf(string sequence)
        {
            return IndexIn(sequence) == -1;
        }

        /// <summary>
        /// Returns the index of the first matching character in a character sequence, or <c>-1</c> if no
        /// matching character is present.
        /// <para>>The default implementation iterates over the sequence in forward order calling
        /// <see cref="matches"/> for each character.</para>
        /// </summary>
        /// <param name="sequence">the character sequence to examine from the beginning</param>
        /// <returns>an index, or <c>-1</c> if no character matches</returns>
        public virtual int IndexIn(string sequence)
        {
            int length = sequence.Length;
            for (int i = 0; i < length; i++)
            {
                if (matches(sequence[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        ///  Returns the index of the first matching character in a character sequence, starting from a
        /// given position, or <c>-1</c> if no character matches after that position.
        /// <para>The default implementation iterates over the sequence in forward order, beginning at
        /// <c>start</c>, calling <see cref="matches"/> for each character.</para>
        /// </summary>
        /// <param name="sequence">the character sequence to examine</param>
        /// <param name="start">the first index to examine; must be nonnegative and no greater than <c>sequence.length()</c>
        ///</param>
        /// <returns>the index of the first matching character, guaranteed to be no less than <c>start</c>,
        /// or <c>-1</c> if no character matches</returns>
        /// <exception cref="IndexOutOfRangeException">if start is negative 
        /// or greater than <c>sequence.length()</c></exception>
        public virtual int IndexIn(string sequence, int start)
        {
            int length = sequence.Length;
            CheckPositionIndex(start, length);
            for (int i = start; i < length; i++)
            {
                if (matches(sequence[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        ///  Returns the index of the last matching character in a character sequence, or <c>-1</c> if no
        /// matching character is present.
        /// <para>The default implementation iterates over the sequence in reverse order calling
        /// <see cref="matches"/> for each character.</para>
        /// </summary>
        /// <param name="sequence">the character sequence to examine from the end</param>
        /// <returns>an index, or <c>-1</c> if no character matches</returns>
        public virtual int LastIndexIn(string sequence)
        {
            for (int i = sequence.Length - 1; i >= 0; i--)
            {
                if (matches(sequence[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// eturns the number of matching characters found in a character sequence.
        /// </summary>
        public int CountIn(string sequence)
        {
            int count = 0;
            for (int i = 0; i < sequence.Length; i++)
            {
                if (matches(sequence[i]))
                {
                    count++;
                }
            }
            return count;
        }

        public virtual CharMatcher precomputed()
        {
            //return Platform.precomputeCharMatcher(this);
            return this;
        }

        private static String showCharacter(char c)
        {
            String hex = "0123456789ABCDEF";
            char[] tmp = { '\\', 'u', '\0', '\0', '\0', '\0' };
            for (int i = 0; i < 4; i++)
            {
                tmp[5 - i] = hex[c & 0xF];
                c >>= 4;
            }
            return new string(tmp);
        }

        #endregion

        #region Fast matchers

        /** A matcher for which precomputation will not yield any significant benefit. */
        private abstract class FastMatcher : CharMatcher
        {
            public sealed override CharMatcher precomputed()
            {
                return this;
            }
        }

        /// <summary>
        /// <see cref="CharMatcher.FastMatcher" /> which overrides <c>ToString()</c> with a custom name.
        /// </summary>
        private abstract class NamedFastMatcher : FastMatcher
        {
            private readonly string description;

            protected NamedFastMatcher(string description)
            {
                this.description = CheckNotNull(description);
            }

            public override string ToString()
            {
                return description;
            }
        }

        #endregion

        #region Static constant implementation classes

        /// <summary>
        /// Implementation of <see cref="CharMatcher.Any"/>
        /// </summary>
        private sealed class AnyMatcher : NamedFastMatcher
        {
            internal static readonly AnyMatcher INSTANCE = new AnyMatcher();

            private AnyMatcher() : base("CharMatcher.any()")
            {
            }


            public override bool matches(char c)
            {
                return true;
            }


            public override int IndexIn(string sequence)
            {
                return (sequence.Length == 0) ? -1 : 0;
            }


            public override int IndexIn(string sequence, int start)
            {
                int length = sequence.Length;
                CheckPositionIndex(start, length);
                return (start == length) ? -1 : start;
            }


            public override int LastIndexIn(string sequence)
            {
                return sequence.Length - 1;
            }


            public override bool MatchesAllOf(string sequence)
            {
                CheckNotNull(sequence);
                return true;
            }

            public override bool MatchesNoneOf(string sequence)
            {
                return sequence.Length == 0;
            }


            public override CharMatcher And(CharMatcher other)
            {
                return CheckNotNull(other);
            }


            public override CharMatcher Or(CharMatcher other)
            {
                CheckNotNull(other);
                return this;
            }

            public /*override*/ CharMatcher Negate()
            {
                return None;
            }
        }

        /// <summary>
        /// Implementation of <see cref="CharMatcher.None"/>
        /// </summary>
        private sealed class NoneMatcher : FastMatcher
        {
            internal static readonly CharMatcher INSTANCE = new NoneMatcher();

            public override bool matches(char c)
            {
                return false;
            }

            public override int IndexIn(string sequence)
            {
                return -1;
            }

            public override int IndexIn(string sequence, int start)
            {
                return -1;
            }
        }

        /// <summary>
        /// Implementation of <see cref="CharMatcher.Whitespace"/>
        /// </summary>
        private sealed class WhitespaceMatcher : NamedFastMatcher
        {
            private static readonly string TABLE =
                "\u2002\u3000\r\u0085\u200A\u2005\u2000\u3000"
                + "\u2029\u000B\u3000\u2008\u2003\u205F\u3000\u1680"
                + "\u0009\u0020\u2006\u2001\u202F\u00A0\u000C\u2009"
                + "\u3000\u2004\u3000\u3000\u2028\n\u2007\u3000";

            private static readonly int MULTIPLIER = 1682554634;
            private static readonly int SHIFT = (TABLE.Length - 1).NumberOfLeadingZeros();

            internal static readonly WhitespaceMatcher INSTANCE = new WhitespaceMatcher();

            private WhitespaceMatcher() : base("CharMatcher.whitespace()")
            {
            }

            public override bool matches(char c)
            {
                //return TABLE.charAt((MULTIPLIER * c) >>> SHIFT) == c;
                return TABLE[(int) ((uint) (MULTIPLIER * c) >> SHIFT)] == c;
            }
        }

        #endregion

        #region Non-static factory implementation classes

        /// <summary>
        /// Implementation of <see cref="CharMatcher.And(CharMatcher)"/>.
        /// </summary>
        private sealed class AndMatcher : CharMatcher
        {
            private readonly CharMatcher first;
            private readonly CharMatcher second;

            internal AndMatcher(CharMatcher a, CharMatcher b)
            {
                first = CheckNotNull(a);
                second = CheckNotNull(b);
            }

            public override bool matches(char c)
            {
                return first.matches(c) && second.matches(c);
            }

            public override string ToString()
            {
                return "CharMatcher.and(" + first + ", " + second + ")";
            }
        }

        /// <summary>
        /// Implementation of <see cref="CharMatcher.Or(CharMatcher)"/>.
        /// </summary>
        private sealed class OrMatcher : CharMatcher
        {
            private readonly CharMatcher first;
            private readonly CharMatcher second;

            internal OrMatcher(CharMatcher a, CharMatcher b)
            {
                first = CheckNotNull(a);
                second = CheckNotNull(b);
            }

            public override bool matches(char c)
            {
                return first.matches(c) || second.matches(c);
            }

            public override string ToString()
            {
                return "CharMatcher.or(" + first + ", " + second + ")";
            }
        }

        #endregion

        #region Static factory implementations

        /// <summary>
        /// Implementation of <see cref="CharMatcher.isChar(char)"/>
        /// </summary>
        private sealed class IsMatcher : NamedFastMatcher
        {
            private readonly char match;

            internal IsMatcher(char match, string description) : base(description)
            {
                this.match = match;
            }

            public override bool matches(char c)
            {
                return c == match;
            }
        }

        private static CharMatcher isEither(char match1, char match2)
        {
            return new IsEitherMatcher(match1, match2);
        }

        /// <summary>
        /// Implementation of <see cref="CharMatcher.anyOf(string)"/> for exactly two characters.
        /// </summary>
        private sealed class IsEitherMatcher : FastMatcher
        {
            private readonly char match1;
            private readonly char match2;

            internal IsEitherMatcher(char match1, char match2)
            {
                this.match1 = match1;
                this.match2 = match2;
            }

            public override bool matches(char c)
            {
                return c == match1 || c == match2;
            }

            public override string ToString()
            {
                return "CharMatcher.anyOf(\"" + showCharacter(match1) + showCharacter(match2) + "\")";
            }
        }

        /// <summary>
        /// Implementation of <see cref="CharMatcher.anyOf(string)"/> for three or more characters.
        /// </summary>
        private sealed class AnyOfMatcher : CharMatcher
        {
            private readonly char[] chars;

            internal AnyOfMatcher(string chars)
            {
                this.chars = chars.ToCharArray();
                Array.Sort(this.chars);
            }

            public override bool matches(char c)
            {
                return Array.BinarySearch(chars, c) >= 0;
            }

            public override string ToString()
            {
                StringBuilder description = new StringBuilder("CharMatcher.anyOf(\"");
                foreach (char c in chars)
                {
                    description.Append(showCharacter(c));
                }
                description.Append("\")");
                return description.ToString();
            }
        }

        #endregion
    }
}