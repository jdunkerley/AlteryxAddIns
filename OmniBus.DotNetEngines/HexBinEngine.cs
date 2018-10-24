using System;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.EventHandlers;
using OmniBus.Framework.Interfaces;
using OmniBus.Framework.Serialisation;

namespace OmniBus
{
    /// <summary>
    ///     Given a set of X and Y co=ordinates compute HexBin X and Y co-ordinates
    /// </summary>
    public class HexBinEngine : BaseEngine<HexBinConfig>
    {
        private Func<RecordData, Tuple<double?, double?>> _inputReader;
        private FieldBase _outputBinXFieldBase;
        private FieldBase _outputBinYFieldBase;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HexBinEngine" /> class.
        ///     Constructor For Alteryx
        /// </summary>
        public HexBinEngine()
        {
            this.Input = new InputProperty(this);
            this.Input.InitCalled += this.OnInit;
            this.Input.ProgressUpdated += (sender, percentage) => this.Output?.UpdateProgress(percentage, true);
            this.Input.RecordPushed += this.OnRecordPushed;
            this.Input.Closed += sender => this.OnClosed();
        }

        /// <summary>
        ///     Gets the input stream.
        /// </summary>
        [CharLabel('I')]
        public IInputProperty Input { get; }

        /// <summary>
        ///     Gets or sets the output stream.
        /// </summary>
        [CharLabel('O')]
        public IOutputHelper Output { get; set; }

        /// <summary>Create a Serialiser</summary>
        /// <returns><see cref="T:OmniBus.Framework.Serialisation.ISerialiser`1" /> to de-serialise object</returns>
        protected override ISerialiser<HexBinConfig> Serialiser() => new Serialiser<HexBinConfig>();

        private void OnInit(IInputProperty sender, SuccessEventArgs args)
        {
            this._inputReader = this.InputPointReader(this.Input.RecordInfo);
            if (this._inputReader == null)
            {
                args.SetFailed();
                return;
            }

            var recordInfo = new RecordInfoBuilder()
                .AddFields(this.Input.RecordInfo)
                .AddFields(new FieldDescription(
                    this.ConfigObject.OutputBinXFieldName,
                    FieldType.E_FT_Double,
                    source: "HexBin: X Co-ordinate of Center"))
                .AddFields(new FieldDescription(
                        this.ConfigObject.OutputBinYFieldName,
                        FieldType.E_FT_Double,
                        source: "HexBin: Y Co-ordinate of Center"))
                .Build();

            this.Output?.Init(recordInfo);

            this._outputBinXFieldBase = this.Output?[this.ConfigObject.OutputBinXFieldName];
            this._outputBinYFieldBase = this.Output?[this.ConfigObject.OutputBinYFieldName];
        }

        private void OnRecordPushed(object sender, RecordData recordData, SuccessEventArgs args)
        {
            var record = this.Output.Record;
            record.Reset();

            this.Input.Copier.Copy(record, recordData);
            var point = this._inputReader(recordData);

            if (!point.Item1.HasValue || double.IsNaN(point.Item1.Value) || !point.Item2.HasValue
                || double.IsNaN(point.Item2.Value))
            {
                this._outputBinXFieldBase.SetNull(record);
                this._outputBinYFieldBase.SetNull(record);
                this.Output.Push(record);
                return;
            }

            var dy = 2 * 0.86602540378443864676372317075294 * this.ConfigObject.Radius; // 2 * Sin(π/3)
            var dx = 1.5 * this.ConfigObject.Radius;

            var px = point.Item1.Value / dx;
            var pi = (int)Math.Round(px);
            var mod2 = (pi & 1) == 1;
            var py = point.Item2.Value / dy - (mod2 ? 0.5 : 0);
            var pj = Math.Round(py);
            var px1 = (px - pi) * dx;

            if (Math.Abs(px1) * 3 > 1)
            {
                var py1 = (py - pj) * dy;
                var pj2 = pj + (py < pj ? -1 : 1) / 2.0;
                var pi2 = pi + (px < pi ? -1 : 1);
                var px2 = (px - pi2) * dx;
                var py2 = (py - pj2) * dy;

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

        private Func<RecordData, Tuple<double?, double?>> InputPointReader(RecordInfo ri)
        {
            var pointXBaseIndex = ri.GetFieldNum(this.ConfigObject.InputPointXFieldName, false);
            var pointYBaseIndex = ri.GetFieldNum(this.ConfigObject.InputPointYFieldName, false);
            if (pointXBaseIndex == -1 || pointYBaseIndex == -1)
            {
                return null;
            }

            return
                data => Tuple.Create(ri[pointXBaseIndex].GetAsDouble(data), ri[pointYBaseIndex].GetAsDouble(data));
        }
    }
}