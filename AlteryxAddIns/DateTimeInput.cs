using OmniBus;
using OmniBus.Framework;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    ///     Simple Date Time Input Control
    /// </summary>
    public class DateTimeInput : BaseTool<DateTimeInputConfig, DateTimeInputEngine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : DateTimeInputEngine
        {
        }
    }
}