using FluentAssertions;
using FluentAssertions.Collections;

namespace NGuava.Tests.Base
{
    internal static class StringCollectionAssertionsExtension
    {
        internal static AndConstraint<StringCollectionAssertions> ContainExactlyInOrder(
            this StringCollectionAssertions scAssertions,
            params string[] expectations)
        {
            return scAssertions.BeEquivalentTo(expectations, options => options.WithStrictOrdering());
        }
    }
}