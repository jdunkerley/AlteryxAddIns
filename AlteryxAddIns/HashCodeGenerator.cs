using AlteryxGuiToolkit.Plugins;

using OmniBus;
using OmniBus.Framework;

namespace JDunkerley.AlteryxAddIns
{
    public class HashCodeGenerator : BaseTool<HashCodeGeneratorConfig, HashCodeGeneratorEngine>, IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : HashCodeGeneratorEngine
        {
        }
    }
}