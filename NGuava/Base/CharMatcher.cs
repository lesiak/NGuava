using System;
using System.Text;

namespace NGuava.Base
{
    public class CharMatcher
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

        public static CharMatcher NONE { get; private set; }

        public bool matches(char c)
        {
            return true;
        }


        public int indexIn(string sequence)
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

        public int indexIn(string sequence, int start) {
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


        abstract class FastMatcher : CharMatcher
        {
            private FastMatcher()
            {
            }

            private FastMatcher(string description) : base(description)
            {
            }


            public sealed override CharMatcher precomputed()
            {
                return this;
            }


            //public override CharMatcher negate() {
            //  return new NegatedFastMatcher(this);
            // }
        }
    }
}