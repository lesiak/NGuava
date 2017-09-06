using System;

namespace NGuava.Base
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class VisibleForTestingAttribute : Attribute
    {
    }
}