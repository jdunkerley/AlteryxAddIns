using OmniBus.Framework;

namespace JDunkerley.AlteryxAddIns
{
    /// <summary>
    /// Allow sorting on a field with specified culture
    /// </summary>
    public class SortWithCulture :
        BaseTool<SortWithCultureConfig, SortWithCultureEngine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : SortWithCultureEngine
        {
        }
    }
}
