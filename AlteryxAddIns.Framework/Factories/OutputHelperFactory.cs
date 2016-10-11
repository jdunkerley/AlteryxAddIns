namespace JDunkerley.AlteryxAddIns.Framework.Factories
{
    using Interfaces;

    /// <summary>
    /// Factory For Creating <see cref="IOutputHelper"/> objects.
    /// </summary>
    public class OutputHelperFactory : IOutputHelperFactory
    {
        /// <summary>
        /// Creates a new instance of an <see cref="IOutputHelper"/>
        /// </summary>
        /// <param name="hostEngine">The host engine.</param>
        /// <param name="connectionName">Name of the outgoing connection.</param>
        /// <returns>A configured instance of an <see cref="IOutputHelper"/></returns>
        public IOutputHelper CreateOutputHelper(IBaseEngine hostEngine, string connectionName)
            => new OutputHelper(hostEngine, connectionName);
    }
}