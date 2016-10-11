namespace JDunkerley.AlteryxAddIns
{
    using Framework;

    using Framework.Factories;
    using Framework.Interfaces;

    public class TwitterStream : BaseTool<TwitterStream.Config, TwitterStream.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public class Config
        {

        }

        public class Engine : BaseStreamEngine<Config>
        {
            /// <summary>
            /// Constructor for Alteryx Engine
            /// </summary>
            public Engine()
                : this(new OutputHelperFactory())
            {
            }

            /// <summary>
            /// Create An Engine for unit testing.
            /// </summary>
            /// <param name="outputHelperFactory">Factory to create output helpers</param>
            internal Engine(IOutputHelperFactory outputHelperFactory)
                : base(null, outputHelperFactory)
            {
            }
        }
    }
}
