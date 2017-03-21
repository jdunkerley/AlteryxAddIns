using System;

namespace OmniBus.Framework.Attributes
{
    /// <summary>
    ///     Specifies a numeric value to order incoming or outgoing connections.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class OrderingAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OrderingAttribute" /> class.
        /// </summary>
        /// <param name="order">The order value.</param>
        public OrderingAttribute(int order)
        {
            this.Order = order;
        }

        /// <summary>
        ///     Gets the order value.
        /// </summary>
        public int Order { get; }
    }
}