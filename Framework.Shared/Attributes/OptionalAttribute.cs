using System;

namespace OmniBus.Framework.Attributes
{
    /// <summary>
    ///     Specifies that an incoming connection is an optional input
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class OptionalAttribute : Attribute
    {
    }
}