using System;
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
    }
}