namespace JDunkerley.AlteryxAddIns
{
    using System;
    using System.ComponentModel;

    using AlteryxRecordInfoNet;

    using Framework;
    using Framework.Attributes;
    using Framework.ConfigWindows;
    using Framework.Factories;
    using Framework.Interfaces;

    public class HexBin :
        BaseTool<HexBin.Config, HexBin.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public class Config : ConfigWithIncomingConnection
        {
            /// <summary>
            /// Specify the name of the field for the X co-ordinate
            /// </summary>
            [Category("Output")]
            [Description("Field Name To Use For X Co-ordinate Of HexBin Centre")]
            public string OutputBinXFieldName { get; set; } = "HexBinX";

            /// <summary>
            /// Specify the name of the field for the Y co-ordinate
            /// </summary>
            [Category("Output")]
            [Description("Field Name To Use For Y Co-ordinate Of HexBin Centre")]
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
            /// Gets or sets the radius of a hexagon.
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
            private FieldBase _outputBinXFieldBase;

            private FieldBase _outputBinYFieldBase;

            private Func<RecordData, Tuple<double?, double?>> _inputReader;

            /// <summary>
            /// Constructor For Alteryx
            /// </summary>
            public Engine()
                : this(new RecordCopierFactory(), new InputPropertyFactory(), new OutputHelperFactory())
            {
            }

            /// <summary>
            /// Create An Engine for unit testing.
            /// </summary>
            /// <param name="recordCopierFactory">Factory to create copiers</param>
            /// <param name="inputPropertyFactory">Factory to create input properties</param>
            /// <param name="outputHelperFactory">Factory to create output helpers</param>
            internal Engine(IRecordCopierFactory recordCopierFactory, IInputPropertyFactory inputPropertyFactory, IOutputHelperFactory outputHelperFactory)
                : base(recordCopierFactory, outputHelperFactory)
            {
                this.Input = inputPropertyFactory.Build(recordCopierFactory, this.ShowDebugMessages);
                this.Input.InitCalled += this.OnInit;
                this.Input.ProgressUpdated += (sender, args) => this.Output.UpdateProgress(args.Progress, true);
                this.Input.RecordPushed += this.OnRecordPushed;
                this.Input.Closed += (sender, args) => this.OnClosed();
            }

            /// <summary>
            /// Gets the input stream.
            /// </summary>
            [CharLabel('I')]
            public IInputProperty Input { get; }

            /// <summary>
            /// Gets or sets the output stream.
            /// </summary>
            [CharLabel('O')]
            public IOutputHelper Output { get; set; }

            private void OnInit(object sender, SuccessEventArgs args)
            {
                this._inputReader = this.ConfigObject.InputPointReader(this.Input.RecordInfo);
                if (this._inputReader == null)
                {
                    args.Success = false;
                    return;
                }

                this.Output?.Init(FieldDescription.CreateRecordInfo(
                    this.Input.RecordInfo,
                    OutputType.Double.OutputDescription(this.ConfigObject.OutputBinXFieldName, source: nameof(HexBin), description: "X Co-ordinate of HexBin Centre"),
                    OutputType.Double.OutputDescription(this.ConfigObject.OutputBinYFieldName, source: nameof(HexBin), description: "Y Co-ordinate of HexBin Centre")));

                this._outputBinXFieldBase = this.Output?[this.ConfigObject.OutputBinXFieldName];
                this._outputBinYFieldBase = this.Output?[this.ConfigObject.OutputBinYFieldName];

                args.Success = true;
            }

            private void OnRecordPushed(object sender, RecordPushedEventArgs args)
            {
                args.Success = true;

                var record = this.Output.Record;
                record.Reset();

                this.Input.Copier.Copy(record, args.RecordData);
                var point = this._inputReader(args.RecordData);

                if (!point.Item1.HasValue || double.IsNaN(point.Item1.Value) ||
                    !point.Item2.HasValue || double.IsNaN(point.Item2.Value))
                {
                    this._outputBinXFieldBase.SetNull(record);
                    this._outputBinYFieldBase.SetNull(record);
                    this.Output.Push(record);
                    return;
                }

                double dy = 2 * 0.86602540378443864676372317075294 * this.ConfigObject.Radius; // 2 * Sin(π/3)
                double dx = 1.5 * this.ConfigObject.Radius;

                double px = point.Item1.Value / dx;
                int pi = (int)Math.Round(px);
                bool mod2 = (pi & 1) == 1;
                double py = point.Item2.Value / dy - (mod2 ? 0.5 : 0);
                double pj = Math.Round(py);
                double px1 = (px - pi) * dx;

                if (Math.Abs(px1) * 3 > 1)
                {
                    double py1 = (py - pj) * dy;
                    double pj2 = pj + (py < pj ? -1 : 1) / 2.0;
                    int pi2 = pi + (px < pi ? -1 : 1);
                    double px2 = (px - pi2) * dx;
                    double py2 = (py - pj2) * dy;

                    if (px1 * px1 + py1 * py1 > px2 * px2 + py2 * py2)
                    {
                        pj = pj2 + (mod2 ? 1 : -1) / 2.0;
                        pi = pi2;
                        mod2 = (pi & 1) == 1;
                    }
                }

                this._outputBinYFieldBase.SetFromDouble(record, (pj + (mod2 ? 0.5 : 0)) * dy);
                this._outputBinXFieldBase.SetFromDouble(record, pi * dx);

                this.Output.Push(record);
            }

            private void OnClosed()

            {
                this._inputReader = null;
                this._outputBinXFieldBase = null;
                this._outputBinYFieldBase = null;
                this.Output?.Close(true);
            }
        }
    }
}
