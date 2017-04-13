using AlteryxGuiToolkit.Plugins;

using OmniBus;
using OmniBus.Framework;
using OmniBus.Framework.ConfigWindows;
using OmniBus.Framework.Serialisation;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    /// Allow sorting on a field with specified culture
    /// </summary>
    public class SortWithCulture :
        BaseTool<SortWithCultureConfig, SortWithCultureEngine>, IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : SortWithCultureEngine
        {
        }

        /// <summary>GUI Designer</summary>
        /// <returns>The configuration object to render in the properties window.</returns>
        public override IPluginConfiguration GetConfigurationGui()
            => new PropertyGridGui<SortWithCultureConfig> { SerialiserFactory = () => new Serialiser<SortWithCultureConfig>() };

    }
}
