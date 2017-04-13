using AlteryxGuiToolkit.Plugins;

using OmniBus;
using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.ConfigWindows;
using OmniBus.Framework.Serialisation;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    /// .Net Based Number Parser
    /// Supports automatic format and specific format as well as cultures
    /// </summary>
    [PlugInGroup("Parse", "Omnibus Number")]
    public class NumberParser : BaseTool<NumberParserConfig, NumberParserEngine>, IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : NumberParserEngine
        {
        }

        /// <summary>GUI Designer</summary>
        /// <returns>The configuration object to render in the properties window.</returns>
        public override IPluginConfiguration GetConfigurationGui()
            => new PropertyGridGui<NumberParserConfig> { SerialiserFactory = () => new Serialiser<NumberParserConfig>() };
    }
}