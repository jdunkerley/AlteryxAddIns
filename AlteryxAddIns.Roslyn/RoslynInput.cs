using OmniBus.Framework;

namespace JDunkerley.AlteryxAddIns.Roslyn
{
    /// <summary>
    /// 
    /// </summary>
    public class RoslynInput :
        BaseTool<RoslynInputConfig, RoslynInputEngine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public override AlteryxGuiToolkit.Plugins.IPluginConfiguration GetConfigurationGui()
            => new RoslynInputGui(RoslynInputEngine.GetCodeFromLambda);

        /// <summary>
        /// Place Holder for Old Entry Point
        /// </summary>
        public class Engine : RoslynInputEngine
        {
        }
    }
}
