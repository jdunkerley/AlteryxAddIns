using AlteryxGuiToolkit.Plugins;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;

namespace OmniBus.XmlTools
{
    /// <summary>Tell Alteryx About The Config And Engine</summary>
    [PlugInGroup("Parse", "XML Parser")]
    public class XmlParse : BaseTool<XmlParseConfig, XmlParseEngine>, IPlugin
    {
    }
}