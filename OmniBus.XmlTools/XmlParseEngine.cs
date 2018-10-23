using System.Collections.Generic;
using System.Xml;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.EventHandlers;
using OmniBus.Framework.Interfaces;

namespace OmniBus.XmlTools
{
    /// <summary>Read An Xml File Into Alteryx.</summary>
    public class XmlParseEngine : BaseEngine<XmlParseConfig>
    {
        private IRecordCopier _copier;
        private FieldBase _inputField;
        private FieldBase _xpathField;
        private FieldBase _innerTextField;
        private FieldBase _innerXmlField;
        private long _recordCount;

        /// <summary>Gets the input stream.</summary>
        [CharLabel('I')]
        public IInputProperty Input { get; }


        /// <summary>Gets or sets the output stream.</summary>
        [CharLabel('O')]
        public IOutputHelper Output { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlParseEngine"/> class.
        ///     Constructor For Alteryx
        /// </summary>
        public XmlParseEngine()
        {
            this.Input = new InputProperty(this);
            this.Input.InitCalled += this.OnInit;
            this.Input.ProgressUpdated += (sender, percentage) => this.Output.UpdateProgress(percentage, true);
            this.Input.RecordPushed += this.OnRecordPushed;
            this.Input.Closed += sender => this.Output?.Close(true);
        }

        private void OnInit(IInputProperty sender, SuccessEventArgs args)
        {
            // Get Input Field
            this._inputField = this.Input.RecordInfo.GetFieldByName(this.ConfigObject.InputFieldName, false);
            if (this._inputField == null)
            {
                args.Success = false;
                return;
            }

            // Create Output Format
            var recordInfo = new RecordInfoBuilder()
                .AddFields(this.Input.RecordInfo)
                .RemoveFields()
                .AddFields(
                    new FieldDescription(nameof(XmlUtils.NodeData.XPath), FieldType.E_FT_V_WString),
                    new FieldDescription(nameof(XmlUtils.NodeData.InnerText), FieldType.E_FT_V_WString),
                    new FieldDescription(nameof(XmlUtils.NodeData.InnerXml), FieldType.E_FT_V_WString)
                ).Build();
            this.Output?.Init(recordInfo);

            // Create the Copier
            this._copier = new RecordCopierBuilder(this.Input.RecordInfo, this.Output?.RecordInfo)
                .SkipFields(
                    this._inputField.GetFieldName(),
                    nameof(XmlUtils.NodeData.XPath),
                    nameof(XmlUtils.NodeData.InnerText),
                    nameof(XmlUtils.NodeData.InnerXml)
                ).Build();

            this._xpathField = this.Output?[nameof(XmlUtils.NodeData.XPath)];
            this._innerTextField = this.Output?[nameof(XmlUtils.NodeData.InnerText)];
            this._innerXmlField = this.Output?[nameof(XmlUtils.NodeData.InnerXml)];

            args.Success = true;
        }

        private void OnRecordPushed(IInputProperty sender, RecordPushedEventArgs args)
        {
            var xml = this._inputField.GetAsString(args.RecordData);
            var nodes = this.ReadNodes(xml);
            var record = this.Output.Record;

            if (nodes == null)
            {
                record.Reset();
                this._copier.Copy(record, args.RecordData);

                this._xpathField.SetNull(record);
                this._innerTextField.SetNull(record);
                this._innerXmlField.SetNull(record);
                this.Output?.Push(record);
                this._recordCount++;
                if (this._recordCount % 100 == 0)
                {
                    this.Output.PushCountAndSize();
                }
            }
            else
            {
                foreach (var nodeData in nodes)
                {
                    record.Reset();
                    this._copier.Copy(record, args.RecordData);

                    if (nodeData?.XPath == null) this._xpathField.SetNull(record); else this._xpathField.SetFromString(record, nodeData.XPath);
                    if (nodeData?.InnerText == null) this._innerTextField.SetNull(record); else this._innerTextField.SetFromString(record, nodeData.InnerText);
                    if (nodeData?.InnerXml == null) this._innerXmlField.SetNull(record); else this._innerXmlField.SetFromString(record, nodeData.InnerXml);
                    this.Output?.Push(record);

                    this._recordCount++;
                    if (this._recordCount % 100 == 0)
                    {
                        this.Output.PushCountAndSize();
                    }
                }
            }
        }

        private IEnumerable<XmlUtils.NodeData> ReadNodes(string xml)
        {
            var document = new XmlDocument();

            try
            {
                document.LoadXml(xml);
            }
            catch (XmlException ex)
            {
                this.Message($"Error reading XML: {ex.Message}");
                return null;
            }

            return XmlUtils.ReadNodes(document.DocumentElement);
        }
    }
}
