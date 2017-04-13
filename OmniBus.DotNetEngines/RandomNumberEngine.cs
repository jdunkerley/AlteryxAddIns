using System;

using AlteryxRecordInfoNet;

using MathNet.Numerics.Distributions;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.EventHandlers;
using OmniBus.Framework.Factories;
using OmniBus.Framework.Interfaces;
using OmniBus.Framework.Serialisation;

namespace OmniBus
{
    /// <summary>
    /// Generate a random number with given distribution and characteristics
    /// </summary>
    public class RandomNumberEngine : BaseEngine<RandomNumberConfig>
    {
        private Func<double> _nextValue;

        private FieldBase _outputFieldBase;

        static RandomNumberEngine()
        {
            DefaultRandom = new Lazy<Random>(() => new Random());
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RandomNumberEngine" /> class.
        ///     Constructor For Alteryx
        /// </summary>
        public RandomNumberEngine()
            : this(new RecordCopierFactory(), new InputPropertyFactory(), new OutputHelperFactory())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RandomNumberEngine" /> class.
        ///     Create An Engine for unit testing.
        /// </summary>
        /// <param name="recordCopierFactory">Factory to create copiers</param>
        /// <param name="inputPropertyFactory">Factory to create input properties</param>
        /// <param name="outputHelperFactory">Factory to create output helpers</param>
        internal RandomNumberEngine(
            IRecordCopierFactory recordCopierFactory,
            IInputPropertyFactory inputPropertyFactory,
            IOutputHelperFactory outputHelperFactory)
            : base(recordCopierFactory, outputHelperFactory)
        {
            this.Input = inputPropertyFactory.Build(recordCopierFactory, this.ShowDebugMessages);
            this.Input.InitCalled += this.OnInit;
            this.Input.ProgressUpdated += (sender, args) => this.Output?.UpdateProgress(args.Progress, true);
            this.Input.RecordPushed += this.OnRecordPushed;
            this.Input.Closed += sender => this.Output?.Close(true);
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

        private static Lazy<Random> DefaultRandom { get; }

        /// <summary>Create a Serialiser</summary>
        /// <returns><see cref="T:OmniBus.Framework.Serialisation.ISerialiser`1" /> to de-serialise object</returns>
        protected override ISerialiser<RandomNumberConfig> Serialiser() => new Serialiser<RandomNumberConfig>();

        private void OnInit(IInputProperty sender, SuccessEventArgs args)
        {
            this._nextValue = this.CreateRandomFunc();

            var fieldDescription = new FieldDescription(
                this.ConfigObject.OutputFieldName,
                this.ConfigObject.OutputType,
                source: $"RandomNumber: {this.ConfigObject.ToString().Replace($"{this.ConfigObject.OutputFieldName}=", string.Empty)}");

            this.Output?.Init(FieldDescription.CreateRecordInfo(this.Input.RecordInfo, fieldDescription));
            this._outputFieldBase = this.Output?[this.ConfigObject.OutputFieldName];

            args.Success = true;
        }

        private void OnRecordPushed(IInputProperty sender, RecordPushedEventArgs args)
        {
            var record = this.Output.Record;
            record.Reset();

            this.Input.Copier.Copy(record, args.RecordData);

            var val = this._nextValue();
            this._outputFieldBase.SetFromDouble(record, val);

            this.Output.Push(record);
            args.Success = true;
        }

        private Func<double> CreateRandomFunc()
        {
            var random = this.ConfigObject.Seed == 0 ? DefaultRandom.Value : new Random(this.ConfigObject.Seed);

            switch (this.ConfigObject.RandomNumberDistribution)
            {
                case RandomNumberDistribution.Triangular:
                    return () => this.TriangularNumber(random.NextDouble());
                case RandomNumberDistribution.Normal:
                    var normal = new Normal(this.ConfigObject.Average, this.ConfigObject.StandardDeviation, random);
                    return () => normal.Sample();
                case RandomNumberDistribution.LogNormal:
                    var logNormal = new LogNormal(
                        this.ConfigObject.Average,
                        this.ConfigObject.StandardDeviation,
                        random);
                    return () => logNormal.Sample();
                case RandomNumberDistribution.Uniform:
                    return () => random.NextDouble() * (this.ConfigObject.Maximum - this.ConfigObject.Minimum)
                                 + this.ConfigObject.Minimum;
            }

            return () => double.NaN;
        }

        private double TriangularNumber(double p)
        {
            var config = this.ConfigObject;
            return p < (config.Average - config.Minimum) / (config.Maximum - config.Minimum)
                       ? config.Minimum + Math.Sqrt(
                             p * (config.Maximum - config.Minimum) * (config.Average - config.Minimum))
                       : config.Maximum - Math.Sqrt(
                             (1 - p) * (config.Maximum - config.Minimum) * (config.Maximum - config.Average));
        }
    }
}