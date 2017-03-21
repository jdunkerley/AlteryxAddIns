using OmniBus;
using OmniBus.Framework;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    ///     Pass through the Input flow if no data received on Breaker
    /// </summary>
    public class CircuitBreaker : BaseTool<CircuitBreakerConfig, CircuitBreakerEngine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : CircuitBreakerEngine
        {
        }
    }
}