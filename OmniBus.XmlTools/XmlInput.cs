using AlteryxGuiToolkit.Plugins;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.ConfigWindows;
using OmniBus.Framework.Serialisation;

namespace OmniBus.XmlTools
{
    /// <summary>Tell Alteryx About The Config And Engine</summary>
    [PlugInGroup("Parse", "XML Parser")]
    public class XmlInput : BaseTool<XmlInputConfig, XmlInputEngine>, IPlugin
    {

        /// <summary>GUI Designer</summary>
        /// <returns>The configuration object to render in the properties window.</returns>
        public override IPluginConfiguration GetConfigurationGui()
            => new PropertyGridGui<XmlParseConfig> { SerialiserFactory = () => new Serialiser<XmlParseConfig>() };

    }
}
