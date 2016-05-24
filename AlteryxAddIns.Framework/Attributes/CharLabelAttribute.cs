namespace JDunkerley.AlteryxAddIns.Framework.Attributes
{
    using System;

    /// <summary>
    /// Label for an Incoming or Outgoing Connection
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CharLabelAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CharLabelAttribute"/> class.
        /// </summary>
        /// <param name="label">The order.</param>
        public CharLabelAttribute(char label)
        {
            this.Label = label;
        }

        /// <summary>
        /// Character Label for the connection
        /// </summary>
        public char Label { get; }
    }
}