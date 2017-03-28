using OmniBus;
using OmniBus.Framework;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    /// Given a string field compute a hash code for it
    /// </summary>
    public class HashCodeGenerator : BaseTool<HashCodeGeneratorConfig, HashCodeGeneratorEngine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : HashCodeGeneratorEngine
        {
        }
    }
}