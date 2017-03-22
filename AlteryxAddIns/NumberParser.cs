using OmniBus;
using OmniBus.Framework;

namespace JDunkerley.AlteryxAddIns
{
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