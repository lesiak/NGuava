using System;

namespace NGuava.Base
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public sealed class NullableAttribute : Attribute
    {
    }
}