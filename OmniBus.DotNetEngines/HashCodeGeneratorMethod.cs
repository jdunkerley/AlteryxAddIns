using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentNaming

namespace OmniBus
{
    /// <summary>
    ///     Enumeration for selecting hash code algorithm
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1514", Justification = "Enum style.")]
    public enum HashCodeGeneratorMethod
    {
        /// <summary>
        ///     https://en.wikipedia.org/wiki/MD5
        /// </summary>
        MD5,
        /// <summary>
        ///     https://en.wikipedia.org/wiki/RIPEMD
        /// </summary>
        RIPEMD160,
        /// <summary>
        ///     https://en.wikipedia.org/wiki/SHA-1
        /// </summary>
        SHA1,
        /// <summary>
        ///     https://en.wikipedia.org/wiki/SHA-256
        /// </summary>
        SHA256,
        /// <summary>
        ///     https://en.wikipedia.org/wiki/SHA-384
        /// </summary>
        SHA384,
        /// <summary>
        ///     https://en.wikipedia.org/wiki/SHA-512
        /// </summary>
        SHA512
    }
}