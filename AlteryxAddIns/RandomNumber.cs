﻿namespace JDunkerley.AlteryxAddins
{
    using System;
    using System.ComponentModel;

    using AlteryxRecordInfoNet;

    using JDunkerley.AlteryxAddIns.Framework;
    using JDunkerley.AlteryxAddIns.Framework.Attributes;
    using JDunkerley.AlteryxAddIns.Framework.ConfigWindows;

    public class RandomNumber :
        BaseTool<RandomNumber.Config, RandomNumber.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public enum Distribution
        {
            Uniform,
            Triangular,
            Normal,
            LogNormal
        }

        public class Config
        {
            /// <summary>
            /// Gets or sets the type of the output.
            /// </summary>
            [Category("Output")]
            [Description("Alteryx Type for the Output Field")]
            [TypeConverter(typeof(FixedListTypeConverter<OutputType>))]
            [FieldList(OutputType.Double, OutputType.Float, OutputType.Int16, OutputType.Int32, OutputType.Int64)]
            public OutputType OutputType { get; set; } = OutputType.Double;

            /// <summary>
            /// Gets or sets the name of the output field.
            /// </summary>
            [Category("Output")]
            [Description("Field Name To Use For Output Field")]
            public string OutputFieldName { get; set; } = "Random";

            /// <summary>
            /// Gets or sets the initial seed.
            /// </summary>
            [Description("Seed for Random Number Generator (0 to use default)")]
            public int Seed { get; set; } = 0;

            /// <summary>
            /// Gets or sets the distribution.
            /// </summary>
            [Description("Distribution For Random Number")]
            public Distribution Distribution { get; set; } = Distribution.Uniform;

            /// <summary>
            /// Gets or sets the minimum boundary.
            /// </summary>
            [Description("Minimum Range Value (for bounded distributions)")]
            public double Minimum { get; set; } = 0;

            /// <summary>
            /// Gets or sets the minimum boundary.
            /// </summary>
            [Description("Maximum Range Value (for bounded distributions)")]
            public double Maximum { get; set; } = 1;

            /// <summary>
            /// Gets or sets the average.
            /// </summary>
            [Description("Average Used For Distributions. (Mean for Normal, Mu for LogNormal)")]
            public double Average { get; set; } = 0;

            /// <summary>
            /// Gets or sets the average.
            /// </summary>
            [Description("Standard Deviation Used For Distributions")]
            public double StandardDeviation { get; set; } = 1;

            /// <summary>
            /// ToString used for annotation
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                switch (this.Distribution)
                {
                    case Distribution.Uniform:
                        return $"{this.OutputFieldName}=Rand[{this.Minimum}, {this.Maximum}]";
                    case Distribution.Triangular:
                        return $"{this.OutputFieldName}=Tri[{this.Minimum}, {this.Average}, {this.Maximum}]";
                    case Distribution.Normal:
                    case Distribution.LogNormal:
                        return $"{this.OutputFieldName}={this.Distribution}[{this.Average}, {this.StandardDeviation}]";
                }
                return "";
            }

            public Func<double> CreateRandomFunc()
            {
                var random = this.Seed == 0 ? new Random() : new Random(this.Seed);

                switch (this.Distribution)
                {
                    case Distribution.Triangular:
                        return () =>
                            {
                                double p = random.NextDouble();
                                return p < (this.Average - this.Minimum) / (this.Maximum - this.Minimum)
                                           ? this.Minimum
                                             + Math.Sqrt(
                                                 p * (this.Maximum - this.Minimum) * (this.Average - this.Minimum))
                                           : this.Maximum
                                             - Math.Sqrt(
                                                 (1 - p) * (this.Maximum - this.Minimum) * (this.Maximum - this.Average));
                            };
                    case Distribution.Normal:
                        {
                            var normal = new MathNet.Numerics.Distributions.Normal(
                                this.Average,
                                this.StandardDeviation,
                                random);
                            return () => normal.Sample();
                        }
                    case Distribution.LogNormal:
                        {
                            var logNormal = new MathNet.Numerics.Distributions.LogNormal(
                                this.Average,
                                this.StandardDeviation,
                                random);
                            return () => logNormal.Sample();
                        }
                    case Distribution.Uniform:
                        return () => random.NextDouble() * (this.Maximum - this.Minimum) + this.Minimum;
                }

                return () => double.NaN;
            }
        }

        public class Engine : BaseEngine<Config>
        {
            private Func<double> _nextValue;

            private RecordInfo _outputRecordInfo;

            private FieldBase _outputFieldBase;

            public Engine()
            {
                this.Input = new InputProperty(
                    initFunc: this.InitFunc,
                    progressAction: d => this.Output.UpdateProgress(d),
                    pushFunc: this.PushFunc,
                    closedAction: () =>
                        {
                            this.Output?.Close();
                            this._nextValue = null;
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
                this._nextValue = this.ConfigObject.CreateRandomFunc();

                var fieldDescription = this.ConfigObject.OutputType.OutputDescription(this.ConfigObject.OutputFieldName, 19);
                if (fieldDescription == null)
                {
                    return false;
                }
                fieldDescription.Source = nameof(RandomNumber);
                fieldDescription.Description = $"Random Number {this.ConfigObject.ToString().Replace($"{this.ConfigObject.OutputFieldName}=", "")}";

                this._outputRecordInfo = Utilities.CreateRecordInfo(info, fieldDescription);
                this._outputFieldBase = this._outputRecordInfo.GetFieldByName(this.ConfigObject.OutputFieldName, false);
                this.Output?.Init(this._outputRecordInfo, nameof(this.Output), null, this.XmlConfig);

                return true;
            }

            private bool PushFunc(RecordData r)
            {
                var record = this._outputRecordInfo.CreateRecord();
                this.Input.Copier.Copy(record, r);

                double val = this._nextValue();
                this._outputFieldBase.SetFromDouble(record, val);

                this.Output?.PushRecord(record.GetRecord());
                return true;
            }
        }
    }
}