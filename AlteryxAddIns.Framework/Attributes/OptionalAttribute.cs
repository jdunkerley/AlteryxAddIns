namespace JDunkerley.AlteryxAddIns.Framework.Attributes
{
    using System;

    /// <summary>
    /// Tag an Incoming Connection As An Optional Input
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class OptionalAttribute : Attribute
    {
    }
}