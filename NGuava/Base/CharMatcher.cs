using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NGuava.Base
{
    public abstract class CharMatcher
    {
        #region Constant matcher properties
        
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
        /// Returns a {@code char} matcher that matches only one specified character.
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public static CharMatcher isChar(char match)
        {
            var description = "CharMatcher.is('" + showCharacter(match) + "')";
            //var description = "Aaa";
            return new SingleCharMatcher(match, description);
        }

        /// <summary>
        /// Returns a {@code char} matcher that matches any character present in the given character
        /// sequence.
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
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
        /// Returns a matcher that matches any character matched by either this matcher or {@code other}.
        /// </summary>
        public CharMatcher or(CharMatcher other)
        {
            return new Or(this, other);
        }
        #endregion

        #region Text processing routines
        public virtual int indexIn(string sequence)
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

        public virtual int indexIn(string sequence, int start)
        {
            int length = sequence.Length;
            Preconditions.CheckPositionIndex(start, length);
            for (int i = start; i < length; i++)
            {
                if (matches(sequence[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion

        public virtual CharMatcher precomputed()
        {
            //return Platform.precomputeCharMatcher(this);
            return this;
        }


        private static CharMatcher isEither(char match1, char match2)
        {
            return new EitherCharMatcher(match1, match2);
        }

   

        private static String showCharacter(char c)
        {
            String hex = "0123456789ABCDEF";
            char[] tmp = {'\\', 'u', '\0', '\0', '\0', '\0'};
            for (int i = 0; i < 4; i++)
            {
                tmp[5 - i] = hex[c & 0xF];
                c >>= 4;
            }
            return new string(tmp);
        }

        #region Fast matchers
        /** A matcher for which precomputation will not yield any significant benefit. */
        abstract class FastMatcher : CharMatcher
        {
            public sealed override CharMatcher precomputed()
            {
                return this;
            }
        }

        /** {@link FastMatcher} which overrides {@code toString()} with a custom name. */
        abstract class NamedFastMatcher : FastMatcher
        {
            private readonly string description;

            protected NamedFastMatcher(string description)
            {
                this.description = Preconditions.CheckNotNull(description);
            }

            public override string ToString()
            {
                return description;
            }
        }

        class SingleCharMatcher : NamedFastMatcher
        {
            private readonly char match;

            internal SingleCharMatcher(char match, string description) : base(description)
            {
                this.match = match;
            }

            public override bool matches(char c)
            {
                return c == match;
            }
        }

        class EitherCharMatcher : FastMatcher
        {
            private readonly char match1;
            private readonly char match2;

            internal EitherCharMatcher(char match1, char match2)
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
        #endregion

        #region Static constant implementation classes
        class AnyOfMatcher : CharMatcher
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

        class Or : CharMatcher
        {
            private readonly CharMatcher first;
            private readonly CharMatcher second;

            internal Or(CharMatcher a, CharMatcher b)
            {
                first = Preconditions.CheckNotNull(a);
                second = Preconditions.CheckNotNull(b);
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
       


        class NoneMatcher : FastMatcher
        {

            public static readonly CharMatcher INSTANCE = new NoneMatcher();

            public override bool matches(char c)
            {
                return false;
            }

            public override int indexIn(string sequence)
            {
                return -1;
            }

            public override int indexIn(string sequence, int start)
            {
                return -1;
            }
        }

        


/**
* Determines whether a character is whitespace according to the latest Unicode standard, as
* illustrated
* <a href="http://unicode.org/cldr/utility/list-unicodeset.jsp?a=%5Cp%7Bwhitespace%7D">here</a>.
* This is not the same definition used by other Java APIs. (See a
* <a href="http://spreadsheets.google.com/pub?key=pd8dAQyHbdewRsnE5x5GzKQ">comparison of several
* definitions of "whitespace"</a>.)
*
* <p><b>Note:</b> as the Unicode definition evolves, we will modify this constant to keep it up
* to date.
*/

       
        class WhitespaceMatcher : NamedFastMatcher
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
    }


}