namespace OmniBus.Framework
{
    /// <summary>
    ///     Connection State Enumeration
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        ///     Alteryx Has Called To Get XML Sort Setting
        /// </summary>
        Added,

        /// <summary>
        ///     Alteryx Has Called Initialize
        /// </summary>
        InitCalled,

        /// <summary>
        ///     Alteryx Has Called Close
        /// </summary>
        Closed
    }
}