using AlteryxGuiToolkit.Plugins;

using OmniBus;
using OmniBus.Framework;
using OmniBus.Framework.ConfigWindows;
using OmniBus.Framework.Serialisation;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    ///     Simple Date Time Input Control
    /// </summary>
    public class DateTimeInput : BaseTool<DateTimeInputConfig, DateTimeInputEngine>, IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : DateTimeInputEngine
        {
        }

        /// <summary>GUI Designer</summary>
        /// <returns>The configuration object to render in the properties window.</returns>
        public override IPluginConfiguration GetConfigurationGui()
            => new PropertyGridGui<DateTimeInputConfig> { SerialiserFactory = () => new Serialiser<DateTimeInputConfig>() };
    }
}