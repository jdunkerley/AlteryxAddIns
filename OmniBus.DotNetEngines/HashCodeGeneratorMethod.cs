namespace OmniBus
{
    /// <summary>
    /// Enumeration for selecting hash code algorithm
    /// </summary>
    public enum HashCodeGeneratorMethod
    {
        // ReSharper disable InconsistentNaming
        /// <summary>
        /// https://en.wikipedia.org/wiki/MD5
        /// </summary>
        MD5,
        /// <summary>
        /// https://en.wikipedia.org/wiki/RIPEMD
        /// </summary>
        RIPEMD160,
        /// <summary>
        /// https://en.wikipedia.org/wiki/SHA-1
        /// </summary>
        SHA1,
        /// <summary>
        /// https://en.wikipedia.org/wiki/SHA-256
        /// </summary>
        SHA256,
        /// <summary>
        /// https://en.wikipedia.org/wiki/SHA-384
        /// </summary>
        SHA384,
        /// <summary>
        /// https://en.wikipedia.org/wiki/SHA-512
        /// </summary>
        SHA512
        // ReSharper restore InconsistentNaming
    }
}