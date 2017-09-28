using FluentAssertions.Collections;

namespace NGuava.Tests.Base
{
    internal static class StringCollectionAssertionsExtension
    {
        internal static void ContainExactlyInOrder(this StringCollectionAssertions scAssertions, params string[] expectations)
        {
            scAssertions.BeEquivalentTo(expectations, options => options.WithStrictOrdering());
        }
    }
}