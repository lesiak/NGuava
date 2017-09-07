using System;
using System.Collections.Generic;
using System.Text;
using static NGuava.Base.Preconditions;

namespace NGuava.Base
{
    public class Joiner
    {
        private readonly string separator;

        public static Joiner On(string separator)
        {
            return new Joiner(separator);
        }


        private Joiner(string separator)
        {
            this.separator = CheckNotNull(separator);
        }


        public virtual StringBuilder AppendTo<T>(StringBuilder stringBuilder, IEnumerator<T> parts)
        {
            CheckNotNull(stringBuilder);
            if (parts.MoveNext())
            {
                stringBuilder.Append(ToString(parts.Current));
            }

            while (parts.MoveNext())
            {
                stringBuilder.Append(separator);
                stringBuilder.Append(ToString(parts.Current));
            }
            return stringBuilder;
        }


        protected virtual string ToString(object part)
        {
            CheckNotNull(part);
            return (part is string s) ? s : part.ToString();
        }


        public string Join<T>(IEnumerable<T> parts)
        {
            return Join(parts.GetEnumerator());
        }


        public string Join<T>(IEnumerator<T> parts)
        {
            return AppendTo(new StringBuilder(), parts).ToString();
        }


        public virtual Joiner UseForNull(string nullText)
        {
            CheckNotNull(nullText);
            return new UseForNullJoiner(this, nullText);
        }


        public virtual Joiner SkipNulls()
        {
            return new SkipNullsJoiner(this);
        }

        private class UseForNullJoiner : Joiner
        {
            private readonly string nullText;


            public UseForNullJoiner(Joiner prototype, string nullText) : base(prototype.separator)
            {
                this.nullText = nullText;
            }


            protected override string ToString(object part)
            {
                return(part == null) ? nullText : base.ToString(part);
            }


            public override Joiner UseForNull(string newNullText)
            {
                throw new InvalidOperationException("already specified UseForNull");
            }

            public override Joiner SkipNulls()
            {
                throw new InvalidOperationException("already specified useForNull");
            }
        }


        private class SkipNullsJoiner : Joiner
        {

            public SkipNullsJoiner(Joiner prototype) : base(prototype.separator)
            { 
            }


            public override StringBuilder AppendTo<T>(StringBuilder stringBuilder, IEnumerator<T> parts)
            {
                CheckNotNull(stringBuilder, "stringBuilder");
                CheckNotNull(parts, "parts");
                while (parts.MoveNext())
                {
                    var part = parts.Current;
                    if (part != null)
                    {
                        stringBuilder.Append(ToString(part));
                        break;
                    }
                }
                while (parts.MoveNext())
                {
                    var part = parts.Current;
                    if (part != null)
                    {
                        stringBuilder.Append(separator);
                        stringBuilder.Append(ToString(part));
                    }
                }
                return stringBuilder;
            }

            public override Joiner UseForNull(String nullText)
            {
                throw new InvalidOperationException("already specified skipNulls");
            }
        }
    }

}