using OmniBus;
using OmniBus.Framework;
using OmniBus.Framework.Attributes;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    /// .Net Based Date Time Parser
    /// Supports automatic format and specific format as well as cultures
    /// </summary>
    [PlugInGroup("Parse", "OmniBus DateTime")]
    public class DateTimeParser : BaseTool<DateTimeParserConfig, DateTimeParserEngine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : DateTimeParserEngine
        {
        }
    }
}