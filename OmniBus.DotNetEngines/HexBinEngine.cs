using System;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.EventHandlers;
using OmniBus.Framework.Factories;
using OmniBus.Framework.Interfaces;

namespace OmniBus
{
    /// <summary>
    /// Given a set of X and Y co=ordinates compute HexBin X and Y co-ordinates
    /// </summary>
    public class HexBinEngine : BaseEngine<HexBinConfig>
    {
        private Func<RecordData, Tuple<double?, double?>> _inputReader;
        private FieldBase _outputBinXFieldBase;

        private FieldBase _outputBinYFieldBase;

        /// <summary>
        ///     Constructor For Alteryx
        /// </summary>
        public HexBinEngine()
            : this(new RecordCopierFactory(), new InputPropertyFactory(), new OutputHelperFactory())
        {
        }

        /// <summary>
        ///     Create An Engine for unit testing.
        /// </summary>
        /// <param name="recordCopierFactory">Factory to create copiers</param>
        /// <param name="inputPropertyFactory">Factory to create input properties</param>
        /// <param name="outputHelperFactory">Factory to create output helpers</param>
        internal HexBinEngine(
            IRecordCopierFactory recordCopierFactory,
            IInputPropertyFactory inputPropertyFactory,
            IOutputHelperFactory outputHelperFactory)
            : base(recordCopierFactory, outputHelperFactory)
        {
            this.Input = inputPropertyFactory.Build(recordCopierFactory, this.ShowDebugMessages);
            this.Input.InitCalled += this.OnInit;
            this.Input.ProgressUpdated += (sender, args) => this.Output.UpdateProgress(args.Progress, true);
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

        private void OnInit(IInputProperty sender, SuccessEventArgs args)
        {
            this._inputReader = this.ConfigObject.InputPointReader(this.Input.RecordInfo);
            if (this._inputReader == null)
            {
                args.Success = false;
                return;
            }

            this.Output?.Init(
                FieldDescription.CreateRecordInfo(
                    this.Input.RecordInfo,
                    new FieldDescription(this.ConfigObject.OutputBinXFieldName, FieldType.E_FT_Double)
                        {
                            Source = nameof(HexBinEngine),
                            Description = "X Co-ordinate of HexBin Centre"
                        },
                    new FieldDescription(this.ConfigObject.OutputBinYFieldName, FieldType.E_FT_Double)
                        {
                            Source = nameof(HexBinEngine),
                            Description = "Y Co-ordinate of HexBin Centre"
                        }));
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
    }
}