using System;

using AlteryxRecordInfoNet;

using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.Factories;
using OmniBus.Framework.Interfaces;

namespace OmniBus
{
    /// <summary>
    /// Engine Piece For Create Date Time Input Values
    /// </summary>
    public class DateTimeInputEngine : BaseEngine<DateTimeInputConfig>
    {
        /// <summary>
        ///     Constructor for Alteryx Engine
        /// </summary>
        public DateTimeInputEngine()
            : this(new OutputHelperFactory())
        {
        }

        /// <summary>
        ///     Create An Engine for unit testing.
        /// </summary>
        /// <param name="outputHelperFactory">Factory to create output helpers</param>
        internal DateTimeInputEngine(IOutputHelperFactory outputHelperFactory)
            : base(null, outputHelperFactory)
        {
        }

        /// <summary>
        ///     Gets or sets the output.
        /// </summary>
        [CharLabel('O')]
        public IOutputHelper Output { get; set; }

        /// <summary>
        ///     Called only if you have no Input Connections
        /// </summary>
        /// <param name="nRecordLimit"></param>
        /// <returns></returns>
        public override bool PI_PushAllRecords(long nRecordLimit)
        {
            if (this.Output == null)
            {
                this.Engine.OutputMessage(this.NToolId, MessageStatus.STATUS_Error, "Output is not set.");
                return false;
            }

            var field = new FieldDescription(this.ConfigObject.OutputFieldName, this.ConfigObject.OutputType)
                            {
                                Source = nameof(DateTimeInputEngine),
                                Description = $"{this.ConfigObject.DateToReturn}",
                                Size = 19
                            };

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
                dateOutput.ToString(this.ConfigObject.OutputType == FieldType.E_FT_Time ? "HH:mm:ss" : "yyyy-MM-dd HH:mm:ss"));
            this.Output.Push(recordOut);
            this.Output.UpdateProgress(1.0);

            this.Output.Close(true);
            return true;
        }

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
                    return new DateTime(today.Year, (today.Month + 2) / 3 * 3, 1);
                case DateTimeInputValueToReturn.PreviousQuarterEnd:
                    today = DateTime.Today;
                    return new DateTime(today.Year, (today.Month + 2) / 3 * 3, 1);
                case DateTimeInputValueToReturn.StartOfMonth:
                    today = DateTime.Today;
                    return today.AddDays(1 - today.Day);
                case DateTimeInputValueToReturn.PreviousMonthEnd:
                    today = DateTime.Today;
                    return today.AddDays(-today.Day);
                case DateTimeInputValueToReturn.StartOfWeek:
                    today = DateTime.Today;
                    return today.AddDays(-(int)today.DayOfWeek);
            }

            return DateTime.MinValue;
        }
    }
}