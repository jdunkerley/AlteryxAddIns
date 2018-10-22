using System;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.Factories;
using OmniBus.Framework.Interfaces;
using OmniBus.Framework.Serialisation;

namespace OmniBus
{
    /// <summary>
    ///     Engine Class For Creating Date Time Input Values
    /// </summary>
    public class DateTimeInputEngine : BaseEngine<DateTimeInputConfig>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DateTimeInputEngine" /> class.
        ///     Constructor for Alteryx Engine
        /// </summary>
        public DateTimeInputEngine()
            : base(new OutputHelperFactory())
        {
        }

        /// <summary>
        ///     Gets or sets the output.
        /// </summary>
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
            if (this.Output == null)
            {
                this.Engine.OutputMessage(this.NToolId, MessageStatus.STATUS_Error, "Output is not set.");
                return false;
            }

            var field = new FieldDescription(
                this.ConfigObject.OutputFieldName,
                this.ConfigObject.OutputType,
                19,
                source: $"DateTImeInput: {this.ConfigObject.DateToReturn}");

            var recordInfo = FieldDescription.CreateRecordInfo(field);

            this.Output.Init(recordInfo);
            if (nRecordLimit == 0)
            {
                this.Output.Close(true);
                return true;
            }

            var dateOutput = ValueToReturn(this.ConfigObject.DateToReturn);

            var recordOut = this.Output.Record;
            this.Output[this.ConfigObject.OutputFieldName]?.SetFromString(
                recordOut,
                dateOutput.ToString(
                    this.ConfigObject.OutputType == FieldType.E_FT_Time ? "HH:mm:ss" : "yyyy-MM-dd HH:mm:ss"));
            this.Output.Push(recordOut);
            this.Output.UpdateProgress(1.0);

            this.Output.Close(true);
            return true;
        }

        /// <summary>Create a Serialiser</summary>
        /// <returns><see cref="T:OmniBus.Framework.Serialisation.ISerialiser`1" /> to de-serialise object</returns>
        protected override ISerialiser<DateTimeInputConfig> Serialiser() => new Serialiser<DateTimeInputConfig>();

        private static DateTime ValueToReturn(DateTimeInputValueToReturn dateToReturn)
        {
            DateTime today;

            switch (dateToReturn)
            {
                case DateTimeInputValueToReturn.Now:
                    return DateTime.Now;
                case DateTimeInputValueToReturn.Today:
                    return DateTime.Today;
                case DateTimeInputValueToReturn.Yesterday:
                    return DateTime.Today.AddDays(-1);
                case DateTimeInputValueToReturn.StartOfYear:
                    return new DateTime(DateTime.Today.Year, 1, 1);
                case DateTimeInputValueToReturn.PreviousYearEnd:
                    return new DateTime(DateTime.Today.Year - 1, 12, 31);
                case DateTimeInputValueToReturn.StartOfQuarter:
                    today = DateTime.Today;
                    return new DateTime(today.Year, (today.Month - 1) / 3 * 3 + 1, 1);
                case DateTimeInputValueToReturn.PreviousQuarterEnd:
                    today = DateTime.Today;
                    return new DateTime(today.Year, (today.Month - 1) / 3 * 3 + 1, 1).AddDays(-1);
                case DateTimeInputValueToReturn.StartOfMonth:
                    today = DateTime.Today;
                    return today.AddDays(1 - today.Day);
                case DateTimeInputValueToReturn.PreviousMonthEnd:
                    today = DateTime.Today;
                    return today.AddDays(-today.Day);
                case DateTimeInputValueToReturn.StartOfWeek:
                    today = DateTime.Today;
                    return today.AddDays(-(int)today.DayOfWeek);
                default:
                    throw new ArgumentOutOfRangeException(nameof(dateToReturn), dateToReturn, null);
            }
        }
    }
}