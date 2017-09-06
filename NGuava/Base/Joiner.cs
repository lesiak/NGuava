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

         private Joiner(string separator) {
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

        string ToString(object part)
        {
            Preconditions.CheckNotNull(part);
            return (part is string) ? (string)part : part.ToString();
        }

        public string Join<T>(IEnumerable<T> parts)
        {
            return Join(parts.GetEnumerator());
        }

        public string Join<T>(IEnumerator<T> parts)
        {
            return AppendTo(new StringBuilder(), parts).ToString();
        }

    }
}