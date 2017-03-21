using System;

namespace OmniBus.Framework.Attributes
{
    /// <summary>
    ///     Specifies a character label for an incoming or outgoing connection
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CharLabelAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CharLabelAttribute" /> class.
        /// </summary>
        /// <param name="label">The character label.</param>
        public CharLabelAttribute(char label)
        {
            this.Label = label;
        }

        /// <summary>
        ///     Gets the character label for the connection.
        /// </summary>
        public char Label { get; }
    }
}