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


        public static Joiner On(char separator)
        {
            return new Joiner(separator.ToString());
        }


        private Joiner(string separator)
        {
            this.separator = CheckNotNull(separator);
        }

        public virtual StringBuilder AppendTo<T>(StringBuilder stringBuilder, IEnumerable<T> parts)
        {
            return AppendTo(stringBuilder, parts.GetEnumerator());
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


        /// <summary>
        /// Returns a { @code MapJoiner } using the given key-value separator, and the same configuration as
        /// this {@code Joiner} otherwise.
        /// </summary>
        /// <param name="keyValueSeparator"></param>
        /// <returns></returns>
        public MapJoiner WithKeyValueSeparator(string keyValueSeparator)
        {
            return new MapJoiner(this, keyValueSeparator);
        }

        private class UseForNullJoiner : Joiner
        {
            private readonly string nullText;

            internal UseForNullJoiner(Joiner prototype, string nullText) : base(prototype.separator)
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
            internal SkipNullsJoiner(Joiner prototype) : base(prototype.separator)
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

        public class MapJoiner
        {
            private readonly Joiner joiner;
            private readonly string keyValueSeparator;

            internal MapJoiner(Joiner joiner, string keyValueSeparator)
            {
                this.joiner = joiner; // only "this" is ever passed, so don't checkNotNull
                this.keyValueSeparator = CheckNotNull(keyValueSeparator);
            }

            public string Join<TKey, TValue>(IDictionary<TKey, TValue> map)
            {
                return Join(map.GetEnumerator());
            }

            public string Join<TKey, TValue>(IEnumerator<KeyValuePair<TKey, TValue>> mapEnumerator)
            {
                return AppendTo(new StringBuilder(), mapEnumerator).ToString();
            }


           /// <summary>
           /// Appends the string representation of each entry of {@code map}, using the previously
           /// configured separator and key-value separator, to {@code appendable}.
           /// </summary>
           /// <typeparam name="TKey"></typeparam>
           /// <typeparam name="TValue"></typeparam>
           /// <param name="stringBuilder"></param>
           /// <param name="map"></param>
           /// <returns></returns>
            public StringBuilder AppendTo<TKey, TValue>(StringBuilder stringBuilder, IDictionary<TKey, TValue> map)
            {
                return AppendTo(stringBuilder, map.GetEnumerator());
            }

            public StringBuilder AppendTo<TKey, TValue>(StringBuilder stringBuilder,
                IEnumerator<KeyValuePair<TKey, TValue>> parts)
            {
                CheckNotNull(stringBuilder);
                if (parts.MoveNext())
                {
                    var entry = parts.Current;
                    stringBuilder.Append(joiner.ToString(entry.Key));
                    stringBuilder.Append(keyValueSeparator);
                    stringBuilder.Append(joiner.ToString(entry.Value));
                    while (parts.MoveNext())
                    {
                        stringBuilder.Append(joiner.separator);
                        var e = parts.Current;
                        stringBuilder.Append(joiner.ToString(e.Key));
                        stringBuilder.Append(keyValueSeparator);
                        stringBuilder.Append(joiner.ToString(e.Value));
                    }
                }
                return stringBuilder;
            }

            public MapJoiner UseForNull(string nullText)
            {
                return new MapJoiner(joiner.UseForNull(nullText), keyValueSeparator);
            }
        }
    }
}