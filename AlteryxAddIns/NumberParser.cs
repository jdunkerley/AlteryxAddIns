using OmniBus;
using OmniBus.Framework;
using OmniBus.Framework.Attributes;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    /// .Net Based Number Parser
    /// Supports automatic format and specific format as well as cultures
    /// </summary>
    [PlugInGroup("Parse", "Omnibus Number")]
    public class NumberParser : BaseTool<NumberParserConfig, NumberParserEngine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : NumberParserEngine
        {
        }

    }
}