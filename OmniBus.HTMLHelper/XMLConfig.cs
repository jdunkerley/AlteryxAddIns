using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace OmniBus.HTMLHelper
{
    internal class XMLConfig
    {
        public XMLConfig(Type engineType, IEngineScanner scanner)
        {
            this.ConfigDocument = CreateConfigXml(engineType, scanner);
        }

        public XmlDocument ConfigDocument { get; }

        private static XmlDocument CreateConfigXml(Type engineType, IEngineScanner scanner)
        {
            var xmlDoc = new XmlDocument();
            var docElement = xmlDoc.CreateElement("AlteryxJavaScriptPlugin");
            xmlDoc.AppendChild(docElement);

            // Engine Settings
            var engineElement = xmlDoc.CreateElement("EngineSettings");
            docElement.AppendChild(engineElement);
            CreateAttributes(
                engineElement,
                new Dictionary<string, string>
                    {
                        { "SDKVersion", "10.1" },
                        { "EngineDll", engineType.Assembly.Location },
                        { "EngineDllEntryPoint", $".Net:{engineType.FullName}" }
                    });

            // Gui Settings
            var guiSettings = xmlDoc.CreateElement("GuiSettings");
            docElement.AppendChild(guiSettings);
            CreateAttributes(
                guiSettings,
                new Dictionary<string, string>
                    {
                        { "SDKVersion", "10.1" },
                        { "Html", $"{engineType.FullName}.html" },
                        { "Icon", $"{engineType.FullName}.png" }
                    }); // Help

            // Input Connections
            string inputXml = string.Join("", scanner.InputConnections(engineType).Select(c => c.MakeXml()));
            if (!string.IsNullOrWhiteSpace(inputXml))
            {
                var inputConnections = xmlDoc.CreateElement("InputConnections");
                inputConnections.InnerXml = inputXml;
                guiSettings.AppendChild(inputConnections);
            }

            // Output Connections
            string outputXml = string.Join("", scanner.OutputConnections(engineType).Select(c => c.MakeXml()));
            if (!string.IsNullOrWhiteSpace(outputXml))
            {
                var inputConnections = xmlDoc.CreateElement("OutputConnections");
                inputConnections.InnerXml = inputXml;
                guiSettings.AppendChild(inputConnections);
            }

            // Properties

            return xmlDoc;
        }

        private static void CreateAttributes(XmlElement parent, IEnumerable<KeyValuePair<string, string>> attributes)
        {
            if (parent.OwnerDocument == null)
            {
                return;
                ;
            }

            foreach (var keyValuePair in attributes)
            {
                var attribute = parent.OwnerDocument.CreateAttribute(keyValuePair.Key);
                attribute.InnerText = keyValuePair.Value;
                parent.Attributes.Append(attribute);
            }
        }
    }
}
