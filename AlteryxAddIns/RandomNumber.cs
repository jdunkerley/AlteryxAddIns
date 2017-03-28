using OmniBus;
using OmniBus.Framework;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    /// Generate A Random Number
    /// </summary>
    public class RandomNumber : BaseTool<RandomNumberConfig, RandomNumberEngine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : RandomNumberEngine
        {
        }
    }
}