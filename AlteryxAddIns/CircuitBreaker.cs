using AlteryxGuiToolkit.Plugins;

using OmniBus;
using OmniBus.Framework;
using OmniBus.Framework.ConfigWindows;
using OmniBus.Framework.Serialisation;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    ///     Pass through the Input flow if no data received on Breaker
    /// </summary>
    public class CircuitBreaker : BaseTool<CircuitBreakerConfig, CircuitBreakerEngine>, IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : CircuitBreakerEngine
        {
        }

        /// <summary>GUI Designer</summary>
        /// <returns>The configuration object to render in the properties window.</returns>
        public override IPluginConfiguration GetConfigurationGui()
            => new PropertyGridGui<CircuitBreakerConfig> { SerialiserFactory = () => new Serialiser<CircuitBreakerConfig>() };
    }
}