using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace OmniBus.XmlTools
{
    internal static class XmlUtils
    {
        internal class NodeData
        {
            public NodeData(params object[] data)
            {
                this.Data = data;
            }

            public object[] Data { get; }

            public string XPath => this.Data[0] as string;

            public string InnerText => this.Data[1] as string;

            public string InnerXml => this.Data[2] as string;
        }

        /// <summary>
        /// Read All The Xml Nodes From A Node
        /// Recursive scan from input node
        /// </summary>
        /// <param name="node">Xml node to scan</param>
        /// <param name="path">Current path to node</param>
        /// <returns>List of nodes</returns>
        public static IEnumerable<NodeData> ReadNodes(XmlNode node, string path = "")
        {
            if (node == null)
            {
                return Enumerable.Empty<NodeData>();
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

            IEnumerable<NodeData> result = new [] { new NodeData(path, txt, node.InnerXml) };

            if (node.Attributes != null)
            {
                result = result.Concat(node.Attributes.Cast<XmlNode>().SelectMany(n => ReadNodes(n, path)));
            }

            return result.Concat(nodes.SelectMany(n => ReadNodes(n, path)));
        }
    }
}
