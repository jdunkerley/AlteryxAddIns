using AlteryxGuiToolkit.Plugins;

using OmniBus;
using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.ConfigWindows;
using OmniBus.Framework.Serialisation;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    /// .Net Based Date Time Parser
    /// Supports automatic format and specific format as well as cultures
    /// </summary>
    [PlugInGroup("Parse", "OmniBus DateTime")]
    public class DateTimeParser : BaseTool<DateTimeParserConfig, DateTimeParserEngine>, IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : DateTimeParserEngine
        {
        }

        /// <summary>GUI Designer</summary>
        /// <returns>The configuration object to render in the properties window.</returns>
        public override IPluginConfiguration GetConfigurationGui()
            => new PropertyGridGui<DateTimeParserConfig> { SerialiserFactory = () => new Serialiser<DateTimeParserConfig>() };
    }
}