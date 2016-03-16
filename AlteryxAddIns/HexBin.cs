namespace JDunkerley.AlteryxAddins
{
    using System;
    using System.ComponentModel;

    using AlteryxRecordInfoNet;

    using JDunkerley.AlteryxAddIns.Framework;
    using JDunkerley.AlteryxAddIns.Framework.Attributes;
    using JDunkerley.AlteryxAddIns.Framework.ConfigWindows;

    public class HexBin :
        BaseTool<HexBin.Config, HexBin.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {

        public class Config
        {
            /// <summary>
            /// Specify the name of the field for the X co-ordinate
            /// </summary>
            [Category("Output")]
            [Description("Field Name To Use For X Coordingate Of HexBin Center")]
            public string OutputBinXFieldName { get; set; } = "HexBinX";

            /// <summary>
            /// Specify the name of the field for the Y co-ordinate
            /// </summary>
            [Category("Output")]
            [Description("Field Name To Use For Y Coordingate Of HexBin Center")]
            public string OutputBinYFieldName { get; set; } = "HexBinY";

            /// <summary>
            /// Specify the name of the field to hash
            /// </summary>
            [Category("Input")]
            [TypeConverter(typeof(InputFieldTypeConverter))]
            [Description("The Field TO Read For The Input Point X Co-Ordinates")]
            [InputPropertyName(nameof(Engine.Input), typeof(Engine), FieldType.E_FT_Double, FieldType.E_FT_Float, FieldType.E_FT_Int16, FieldType.E_FT_Int32, FieldType.E_FT_Int64)]
            public string InputPointXFieldName { get; set; }

            /// <summary>
            /// Specify the name of the field to hash
            /// </summary>
            [Category("Input")]
            [TypeConverter(typeof(InputFieldTypeConverter))]
            [Description("The Field TO Read For The Input Point Y Co-Ordinates")]
            [InputPropertyName(nameof(Engine.Input), typeof(Engine), FieldType.E_FT_Double, FieldType.E_FT_Float, FieldType.E_FT_Int16, FieldType.E_FT_Int32, FieldType.E_FT_Int64)]
            public string InputPointYFieldName { get; set; }

            /// <summary>
            /// Gets or sets the radius of a heaxgon.
            /// </summary>
            public double Radius { get; set; } = 1;

            /// <summary>
            /// ToString used for annotation
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"HexBin({this.InputPointXFieldName}, {this.InputPointYFieldName})";

            /// <summary>
            /// Create A Point Reader Function
            /// </summary>
            /// <param name="ri"></param>
            /// <returns></returns>
            public Func<RecordData, Tuple<double?, double?>> InputPointReader(RecordInfo ri)
            {
                var pointXBaseIndex = ri.GetFieldNum(this.InputPointXFieldName, false);
                var pointYBaseIndex = ri.GetFieldNum(this.InputPointYFieldName, false);
                if (pointXBaseIndex == -1 || pointYBaseIndex == -1)
                {
                    return null;
                }

                return
                    data => Tuple.Create(ri[pointXBaseIndex].GetAsDouble(data), ri[pointYBaseIndex].GetAsDouble(data));
            }
        }

        public class Engine : BaseEngine<Config>
        {

            private RecordInfo _outputRecordInfo;

            private FieldBase _outputBinXFieldBase;

            private FieldBase _outputBinYFieldBase;

            private Func<RecordData, Tuple<double?, double?>> _inputReader;

            public Engine()
            {
                this.Input = new InputProperty(
                    initFunc: this.InitFunc,
                    progressAction: d =>
                        {
                            this.Output.UpdateProgress(d);
                            this.Engine.OutputToolProgress(this.NToolId, d);
                        },
                    pushFunc: this.PushFunc,
                    closedAction: () =>
                    {
                        this._inputReader = null;
                        this._outputBinXFieldBase = null;
                        this._outputBinYFieldBase = null;
                        this.Output?.Close();
                    });
            }

            /// <summary>
            /// Gets the input stream.
            /// </summary>
            [CharLabel('I')]
            public InputProperty Input { get; }

            /// <summary>
            /// Gets or sets the output stream.
            /// </summary>
            [CharLabel('O')]
            public PluginOutputConnectionHelper Output { get; set; }

            private bool InitFunc(RecordInfo info)
            {
                this._inputReader = this.ConfigObject.InputPointReader(info);
                if (this._inputReader == null)
                {
                    return false;
                }

                this._outputRecordInfo = Utilities.CreateRecordInfo(
                    info,
                    new FieldDescription(this.ConfigObject.OutputBinXFieldName, FieldType.E_FT_Double) { Source = nameof(HexBin), Description = "X Co-ordinate of HexBin Center" },
                    new FieldDescription(this.ConfigObject.OutputBinYFieldName, FieldType.E_FT_Double) { Source = nameof(HexBin), Description = "Y Co-ordinate of HexBin Center" });
                this._outputBinXFieldBase = this._outputRecordInfo.GetFieldByName(this.ConfigObject.OutputBinXFieldName, false);
                this._outputBinYFieldBase = this._outputRecordInfo.GetFieldByName(this.ConfigObject.OutputBinYFieldName, false);
                this.Output?.Init(this._outputRecordInfo, nameof(this.Output), null, this.XmlConfig);

                return true;
            }

            private bool PushFunc(RecordData r)
            {
                var record = this._outputRecordInfo.CreateRecord();
                this.Input.Copier.Copy(record, r);

                var point = this._inputReader(r);

                if (!point.Item1.HasValue || double.IsNaN(point.Item1.Value) ||
                    !point.Item2.HasValue || double.IsNaN(point.Item2.Value))
                {
                    this._outputBinXFieldBase.SetNull(record);
                    this._outputBinYFieldBase.SetNull(record);
                    this.Output?.PushRecord(record.GetRecord());
                    return true;
                }

                double dx = 2 * 0.86602540378443864676372317075294 * this.ConfigObject.Radius; // 2 * Sin(π/3)
                double dy = 1.5 * this.ConfigObject.Radius;

                double py = point.Item1.Value / dy;
                int pj = (int)Math.Round(py);
                bool mod2 = (pj & 1) == 1;
                double px = point.Item2.Value / dx - (mod2 ? 0.5 : 0);
                double pi = Math.Round(px);
                double py1 = py - pj;

                if (Math.Abs(py1) * 3 > 1)
                {
                    double px1 = px - pi;
                    double pi2 = pi + (px < pi ? -1 : 1) / 2.0;
                    int pj2 = pj + (py < pj ? -1 : 1);
                    double px2 = px - pi2;
                    double py2 = py - pj2;

                    if (px1 * px1 * dx * dx + py1 * py1 * dy *dy > px2 * px2 * dx * dx + py2 * py2 * dy * dy)
                    {
                        pi = pi2 + (mod2 ? 1 : -1) / 2.0;
                        pj = pj2;
                        mod2 = (pj & 1) == 1;
                    }
                }

                this._outputBinYFieldBase.SetFromDouble(record, (pi + (mod2 ? 0.5 : 0)) * dx);
                this._outputBinXFieldBase.SetFromDouble(record, pj * dy);

                this.Output?.PushRecord(record.GetRecord());
                return true;
            }
        }
    }
}
