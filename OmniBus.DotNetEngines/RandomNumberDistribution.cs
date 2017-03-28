using System.Diagnostics.CodeAnalysis;

namespace OmniBus
{
    /// <summary>
    /// Different support Random Number Distributions
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1514", Justification = "Enum style.")]

    public enum RandomNumberDistribution
    {
        /// <summary>
        /// Simple uniform distribution
        /// </summary>
        Uniform,
        /// <summary>
        /// Triangular distribution
        /// </summary>
        Triangular,
        /// <summary>
        /// Normal distribution
        /// </summary>
        Normal,
        /// <summary>
        /// Log Normal distribution
        /// </summary>
        LogNormal
    }
}