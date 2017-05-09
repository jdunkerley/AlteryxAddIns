using System.ComponentModel;

using AlteryxRecordInfoNet;

using OmniBus.Framework.Attributes;
using OmniBus.Framework.TypeConverters;

namespace OmniBus
{
    /// <summary>
    /// Configuration class for <see cref="RandomNumberEngine"/>
    /// </summary>
    public class RandomNumberConfig
    {
        /// <summary>
        ///     Gets or sets the type of the output.
        /// </summary>
        [Category("Output")]
        [Description("Alteryx Type for the Output Field")]
        [FieldList(
            FieldType.E_FT_Double,
            FieldType.E_FT_Float,
            FieldType.E_FT_Int16,
            FieldType.E_FT_Int32,
            FieldType.E_FT_Int64)]
        [TypeConverter(typeof(FixedListTypeConverter<FieldType>))]
        public FieldType OutputType { get; set; } = FieldType.E_FT_Double;

        /// <summary>
        ///     Gets or sets the name of the output field.
        /// </summary>
        [Category("Output")]
        [Description("Field Name To Use For Output Field")]
        public string OutputFieldName { get; set; } = "Random";

        /// <summary>
        ///     Gets or sets the initial seed.
        /// </summary>
        [Description("Seed for Random Number Generator (0 to use default)")]
        public int Seed { get; set; } = 0;

        /// <summary>
        ///     Gets or sets the distribution.
        /// </summary>
        [Description("RandomNumberDistribution For Random Number")]
        public RandomNumberDistribution Distribution { get; set; } = RandomNumberDistribution.Uniform;

        /// <summary>
        ///     Gets or sets the minimum boundary.
        /// </summary>
        [Description("Minimum Range Value (for bounded distributions)")]
        public double Minimum { get; set; } = 0;

        /// <summary>
        ///     Gets or sets the minimum boundary.
        /// </summary>
        [Description("Maximum Range Value (for bounded distributions)")]
        public double Maximum { get; set; } = 1;

        /// <summary>
        ///     Gets or sets the average.
        /// </summary>
        [Description("Average Used For Distributions. (Mean for Normal, Mu for LogNormal)")]
        public double Average { get; set; } = 0;

        /// <summary>
        ///     Gets or sets the average.
        /// </summary>
        [Description("Standard Deviation Used For Distributions")]
        public double StandardDeviation { get; set; } = 1;

        /// <summary>
        ///     ToString used for annotation
        /// </summary>
        /// <returns>Default Annotation for Alteryx</returns>
        public override string ToString()
        {
            switch (this.Distribution)
            {
                case RandomNumberDistribution.Uniform: return $"{this.OutputFieldName}=Rand[{this.Minimum}, {this.Maximum}]";
                case RandomNumberDistribution.Triangular:
                    return $"{this.OutputFieldName}=Tri[{this.Minimum}, {this.Average}, {this.Maximum}]";
                case RandomNumberDistribution.Normal:
                case RandomNumberDistribution.LogNormal:
                    return $"{this.OutputFieldName}={this.Distribution}[{this.Average}, {this.StandardDeviation}]";
            }

            return string.Empty;
        }
    }
}