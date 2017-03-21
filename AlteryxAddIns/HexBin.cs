using AlteryxGuiToolkit.Plugins;

using OmniBus;
using OmniBus.Framework;

namespace JDunkerley.AlteryxAddIns
{
    public class HexBin : BaseTool<HexBinConfig, HexBinEngine>, IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : HexBinEngine
        {
        }
    }
}