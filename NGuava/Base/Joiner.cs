using System;
using System.Collections.Generic;
using System.Text;

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
            this.separator = Preconditions.CheckNotNull(separator);
        }


        public StringBuilder AppendTo<T>(StringBuilder stringBuilder, IEnumerator<T> parts)
        {
            Preconditions.CheckNotNull(stringBuilder);
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
            Preconditions.CheckNotNull(part);
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
            Preconditions.CheckNotNull(nullText);
            return new UseForNullJoiner(this, nullText);
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
        }
    }
}