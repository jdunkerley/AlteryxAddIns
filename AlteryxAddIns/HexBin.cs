using AlteryxGuiToolkit.Plugins;

using OmniBus;
using OmniBus.Framework;
using OmniBus.Framework.ConfigWindows;
using OmniBus.Framework.Serialisation;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    /// Replicate the Tableau HexBin functions
    /// </summary>
    public class HexBin : BaseTool<HexBinConfig, HexBinEngine>, IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : HexBinEngine
        {
        }

        /// <summary>GUI Designer</summary>
        /// <returns>The configuration object to render in the properties window.</returns>
        public override IPluginConfiguration GetConfigurationGui()
            => new PropertyGridGui<HexBinConfig> { SerialiserFactory = () => new Serialiser<HexBinConfig>() };

    }
}