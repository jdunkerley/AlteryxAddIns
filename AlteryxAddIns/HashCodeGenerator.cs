using AlteryxGuiToolkit.Plugins;

using OmniBus;
using OmniBus.Framework;
using OmniBus.Framework.ConfigWindows;
using OmniBus.Framework.Serialisation;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    /// Given a string field compute a hash code for it
    /// </summary>
    public class HashCodeGenerator : BaseTool<HashCodeGeneratorConfig, HashCodeGeneratorEngine>, IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : HashCodeGeneratorEngine
        {
        }

        /// <summary>GUI Designer</summary>
        /// <returns>The configuration object to render in the properties window.</returns>
        public override IPluginConfiguration GetConfigurationGui()
            => new PropertyGridGui<HashCodeGeneratorConfig> { SerialiserFactory = () => new Serialiser<HashCodeGeneratorConfig>() };

    }
}