using OmniBus;
using OmniBus.Framework;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    ///     Take a value and format as a string
    /// </summary>
    public class StringFormatter : BaseTool<StringFormatterConfig, StringFormatterEngine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : StringFormatterEngine
        {           
        }
    }
}