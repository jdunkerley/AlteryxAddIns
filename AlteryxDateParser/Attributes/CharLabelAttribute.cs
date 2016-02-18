namespace JDunkerley.Alteryx.Attributes
{
    using System;

    /// <summary>
    /// order a Connection
    /// </summary>
    /// <seealso cref="System.Attribute" />
    public class CharLabelAttribute : Attribute
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
        /// Gets the order.
        /// </summary>
        public char Label { get; }
    }
}