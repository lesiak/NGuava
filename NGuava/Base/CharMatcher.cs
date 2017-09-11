using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NGuava.Base
{
    public abstract class CharMatcher
    {
        // State
        readonly string description;

        // Constructors

        /**
         * Sets the {@code toString()} from the given description.
         */
        CharMatcher(string description)
        {
            this.description = description;
        }

        protected CharMatcher()
        {
            description = base.ToString();
        }

       

        public abstract bool matches(char c);


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

        public virtual int indexIn(string sequence, int start) {
            int length = sequence.Length;
            Preconditions.CheckPositionIndex(start, length);
            for (int i = start; i < length; i++) {
                if (matches(sequence[i])) {
                    return i;
                }
            }
            return -1;
        }
        
        public virtual CharMatcher precomputed()
        {
            //return Platform.precomputeCharMatcher(this);
            return this;
        }
        
        /**
   * Returns a {@code char} matcher that matches only one specified character.
   */
        public static CharMatcher isChar(char match) {
            //String description = "CharMatcher.is('" + showCharacter(match) + "')";
            var description = "Aaa";
            return new SingleCharMatcher(match, description);
        }


        class SingleCharMatcher : FastMatcher
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

        public static readonly CharMatcher NONE = new NoneMatcher();

        class NoneMatcher : FastMatcher
        {

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

         abstract class FastMatcher : CharMatcher
        {
            protected FastMatcher()
            {
            }

            protected FastMatcher(string description) : base(description)
            {
            }


            public sealed override CharMatcher precomputed()
            {
                return this;
            }
        }

        public static readonly CharMatcher Whitespace = new WhiteSpaceMatcher();

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

         class WhiteSpaceMatcher : FastMatcher
        {

            private static readonly String TABLE = "\u0009\u3000\n\u0009\u0009\u0009\u202F\u0009"
            + "\u0009\u2001\u2006\u0009\u0009\u0009\u0009\u0009"
            + "\u180E\u0009\u2029\u0009\u0009\u0009\u2000\u2005"
            + "\u200A\u0009\u0009\u0009\r\u0009\u0009\u2028"
            + "\u1680\u0009\u00A0\u0009\u2004\u2009\u0009\u0009"
            + "\u0009\u000C\u205F\u0009\u0009\u0020\u0009\u0009"
            + "\u2003\u2008\u0009\u0009\u0009\u000B\u0085\u0009"
            + "\u0009\u0009\u0009\u0009\u0009\u2002\u2007\u0009";

            internal WhiteSpaceMatcher() : base("WHITESPACE")
            {
            }

            public override bool matches(char c)
            {
                //return TABLE.charAt((-844444961 * c) >>> 26) == c;
                return TABLE[(int)((uint)(-844444961 * c) >> 26)] == c;
            }
        }

    }


}