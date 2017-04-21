using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.Interfaces;

namespace OmniBus.XmlTools
{
    /// <summary>Read An Xml File Into Alteryx.</summary>
    public class XmlInputEngine : BaseEngine<XmlInputConfig>
    {
        /// <summary>Gets or sets the output stream.</summary>
        [CharLabel('O')]
        public IOutputHelper Output { get; set; }

        /// <summary>
        ///     The PI_PushAllRecords function pointed to by this property will be called by the Alteryx Engine when the plugin
        ///     should provide all of it's data to the downstream tools.
        ///     This is only pertinent to tools which have no upstream (input) connections (such as the Input tool).
        /// </summary>
        /// <param name="nRecordLimit">
        ///     The nRecordLimit parameter will be &lt; 0 to indicate that there is no limit, 0 to indicate
        ///     that the tool is being configured and no records should be sent, or &gt; 0 to indicate that only the requested
        ///     number of records should be sent.
        /// </param>
        /// <returns>Return true to indicate you successfully handled the request.</returns>
        public override bool PI_PushAllRecords(long nRecordLimit)
        {
            this.DebugMessage($"{nameof(nRecordLimit)} Called with {nameof(nRecordLimit)} = {nRecordLimit}");
            this.Output.Init(FieldDescription.CreateRecordInfo(
                new FieldDescription("XPath", FieldType.E_FT_V_WString),
                new FieldDescription("InnerText", FieldType.E_FT_V_WString),
                new FieldDescription("InnerXml", FieldType.E_FT_V_WString)));

            if (nRecordLimit != 0)
            {
                var nodes = this.ReadNodes();
                if (nodes == null)
                {
                    return false;
                }

                long recordCount = 0;
                foreach (var data in nodes)
                {
                    this.Output.PushData(data);
                    recordCount++;

                    if (recordCount % 100 == 0)
                    {
                        this.Output.PushCountAndSize();
                    }

                    if (nRecordLimit == recordCount)
                    {
                        break;
                    }
                }
            }

            this.Output.PushCountAndSize();
            this.Output.Close(true);
            return true;
        }

        /// <summary>
        /// Read All The Xml Nodes From A Document
        /// </summary>
        /// <returns>List of nodes</returns>
        public IEnumerable<object[]> ReadNodes()
        {
            if (string.IsNullOrWhiteSpace(this.ConfigObject.FileName))
            {
                this.Message("You need to specify a filename.", MessageStatus.STATUS_Error);
                return null;
            }

            var fileName = System.IO.Path.IsPathRooted(this.ConfigObject.FileName)
                               ? this.ConfigObject.FileName
                               : System.IO.Path.Combine(
                                   this.Engine.GetInitVar(nameof(InitVar.DefaultDir)),
                                   this.ConfigObject.FileName);

            if (!System.IO.File.Exists(fileName))
            {
                this.Message($"File {this.ConfigObject.FileName} could not be read.", MessageStatus.STATUS_Error);
                return null;
            }

            var document = new XmlDocument();

            try
            {
                document.Load(fileName);
            }
            catch (XmlException ex)
            {
                this.Message($"Error reading XML: {ex.Message}");
                return null;
            }

            return this.ReadNodes(document.DocumentElement);
        }

        /// <summary>
        /// Read All The Xml Nodes From A Node
        /// Recursive scan from input node
        /// </summary>
        /// <param name="node">Xml node to scan</param>
        /// <param name="path">Current path to node</param>
        /// <returns>List of nodes</returns>
        public IEnumerable<object[]> ReadNodes(XmlNode node, string path = "")
        {
            if (node == null)
            {
                return Enumerable.Empty<object[]>();
            }

            path = path + "/" + (node is XmlAttribute ? "@" : "") + node.Name;

            var nodes = node.ChildNodes.Cast<XmlNode>().ToArray();
            var txtNodes = nodes.Where(x => x.Name == "#text").ToArray();

            var txt = nodes.Length == 0 ? node.InnerText : null;
            if (txtNodes.Length == 1)
            {
                txt = txtNodes[0].InnerText;
                nodes = nodes.Where(n => n.Name != "#text").ToArray();
            }

            IEnumerable<object[]> result = new[] { new object[] { path, txt, node.InnerXml } };

            if (node.Attributes != null)
            {
                result = result.Concat(node.Attributes.Cast<XmlNode>().SelectMany(n => this.ReadNodes(n, path)));
            }

            return result.Concat(nodes.SelectMany(n => this.ReadNodes(n, path)));
        }


#if DEBUG
        /// <summary>Tells Alteryx whether to show debug error messages or not.</summary>
        /// <returns>A value indicating whether to show debug error messages or not.</returns>
        public override bool ShowDebugMessages() => true;
#endif
    }
}
