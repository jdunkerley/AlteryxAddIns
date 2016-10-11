namespace JDunkerley.AlteryxAddIns.Framework
{
    using Attributes;
    using Interfaces;

    /// <summary>
    /// Base streaming input tool. Has a single output.
    /// </summary>
    /// <typeparam name="TConfig">Configuration object for reading XML into.</typeparam>
    public abstract class BaseStreamEngine<TConfig> : BaseEngine<TConfig>
        where TConfig : new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseStreamEngine{TConfig}"/> class.
        /// </summary>
        /// <param name="recordCopierFactory">Factory to create copiers</param>
        /// <param name="outputHelperFactory">Factory to create output helpers</param>
        protected BaseStreamEngine(IRecordCopierFactory recordCopierFactory, IOutputHelperFactory outputHelperFactory)
            : base(recordCopierFactory, outputHelperFactory)
        {
        }

        /// <summary>
        /// Gets or sets the output.
        /// </summary>
        [CharLabel('O')]
        public IOutputHelper Output { get; set; }
    }
}