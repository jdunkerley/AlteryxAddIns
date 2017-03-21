namespace OmniBus.Framework.Interfaces
{
    /// <summary>
    ///     Interface to decouple the construction of <see cref="IOutputHelper" /> objects from
    ///     <see cref="BaseEngine{TConfig}" />.
    /// </summary>
    public interface IOutputHelperFactory
    {
        /// <summary>
        ///     Creates a new instance of an <see cref="IOutputHelper" />
        /// </summary>
        /// <param name="hostEngine">The host engine.</param>
        /// <param name="connectionName">Name of the outgoing connection.</param>
        /// <returns>A configured instance of an <see cref="IOutputHelper" /></returns>
        IOutputHelper CreateOutputHelper(IBaseEngine hostEngine, string connectionName);
    }
}