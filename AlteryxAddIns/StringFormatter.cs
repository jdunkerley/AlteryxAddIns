using AlteryxGuiToolkit.Plugins;

using OmniBus;
using OmniBus.Framework;
using OmniBus.Framework.ConfigWindows;
using OmniBus.Framework.Serialisation;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    ///     Take a value and format as a string
    /// </summary>
    public class StringFormatter : BaseTool<StringFormatterConfig, StringFormatterEngine>, IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : StringFormatterEngine
        {
        }

        /// <summary>GUI Designer</summary>
        /// <returns>The configuration object to render in the properties window.</returns>
        public override IPluginConfiguration GetConfigurationGui()
            => new PropertyGridGui<StringFormatterConfig> { SerialiserFactory = () => new Serialiser<StringFormatterConfig>() };

    }
}