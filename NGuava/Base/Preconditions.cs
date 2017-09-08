using System;
using System.Text;

namespace NGuava.Base
{
    public static class Preconditions
    {
        public static void CheckArgument(bool expression)
        {
            if (!expression)
                throw new ArgumentException("Invalid argument.", "expression");
        }

        public static void CheckArgument(bool expression, [Nullable] object errorMessage)
        {
            if (!expression)
                throw new ArgumentException(StringHelper.ValueOf(errorMessage), "expression");
        }

        public static void CheckArgument(bool expression,
            [Nullable] string errorMessageTemplate,
            [Nullable] params object[] errorMessageArgs)
        {
            if (!expression)
                throw new ArgumentException(Format(errorMessageTemplate, errorMessageArgs), "expression");
        }
        
        public static void CheckState(bool expression)
        {
            if (!expression) {
                throw new InvalidOperationException();
            }
        }

        public static T CheckNotNull<T>(T reference) where T : class
        {
            if (reference == null)
                throw new NullReferenceException();

            return reference;
        }

        public static T CheckNotNull<T>(T reference, [Nullable] object errorMessage) where T : class
        {
            if (reference == null)
                throw new NullReferenceException(StringHelper.ValueOf(errorMessage));

            return reference;
        }

        public static T CheckNotNull<T>(T reference,
            [Nullable] string errorMessageTemplate,
            [Nullable] params object[] errorMessageArgs) where T : class
        {
            if (reference == null)
                throw new NullReferenceException(Format(errorMessageTemplate, errorMessageArgs));

            return reference;
        }

        public static int CheckElementIndex(int index, int size)
        {
            return CheckElementIndex(index, size, "index");
        }

        public static int CheckElementIndex(int index, int size, [Nullable] string desc)
        {
            if (index < 0 || index >= size)
                throw new IndexOutOfRangeException(BadElementIndex(index, size, desc));

            return index;
        }

        private static string BadElementIndex(int index, int size, string desc)
        {
            if (index < 0)
                return Format("%s (%s) must not be negative", desc, index);

            if (size < 0)
                throw new ArgumentException("negative size: " + size, "size");

            // index >= size
            return Format("%s (%s) must be less than size (%s)", desc, index, size);
        }

        public static int CheckPositionIndex(int index, int size)
        {
            return CheckPositionIndex(index, size, "index");
        }

        public static int CheckPositionIndex(int index, int size, [Nullable] string desc)
        {
            // Carefully optimized for execution by hotspot (explanatory comment above)
            if (index < 0 || index > size)
            {
                throw new IndexOutOfRangeException(BadPositionIndex(index, size, desc));
            }
            return index;
        }

        private static string BadPositionIndex(int index, int size, string desc)
        {
            if (index < 0)
                return Format("%s (%s) must not be negative", desc, index);

            if (size < 0)
                throw new ArgumentException("negative size: " + size, "size");

            // index > size
            return Format("%s (%s) must not be greater than size (%s)", desc, index, size);
        }

        public static void CheckPositionIndexes(int start, int end, int size)
        {
            // Carefully optimized for execution by hotspot (explanatory comment above)
            if (start < 0 || end < start || end > size)
            {
                throw new IndexOutOfRangeException(BadPositionIndexes(start, end, size));
            }
        }

        private static string BadPositionIndexes(int start, int end, int size)
        {
            if (start < 0 || start > size)
                return BadPositionIndex(start, size, "start index");

            if (end < 0 || end > size)
                return BadPositionIndex(end, size, "end index");

            // end < start
            return Format("end index (%s) must not be less than start index (%s)", end, start);
        }

        [VisibleForTesting]
        internal static string Format(string template,
            [Nullable] params object[] args)
        {
            template = StringHelper.ValueOf(template); // null -> "null"

            // start substituting the arguments into the '%s' placeholders
            var builder = new StringBuilder(template.Length + 16 * args.Length);

            var templateStart = 0;
            var i = 0;

            while (i < args.Length)
            {
                var placeholderStart = template.IndexOf("%s", templateStart, StringComparison.Ordinal);
                if (placeholderStart == -1)
                    break;

                builder.Append(template.Substring(templateStart, placeholderStart));
                builder.Append(args[i++]);
                templateStart = placeholderStart + 2;
            }

            builder.Append(template.Substring(templateStart));

            // if we run out of placeholders, append the extra args in square braces
            if (i < args.Length)
            {
                builder.Append(" [");
                builder.Append(args[i++]);

                while (i < args.Length)
                {
                    builder.Append(", ");
                    builder.Append(args[i++]);
                }

                builder.Append(']');
            }

            return builder.ToString();
        }

        
    }
}