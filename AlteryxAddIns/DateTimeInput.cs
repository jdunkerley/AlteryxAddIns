namespace JDunkerley.AlteryxAddIns
{
    using System;
    using System.ComponentModel;

    using Framework;
    using Framework.Attributes;
    using Framework.ConfigWindows;

    /// <summary>
    /// Simple Date Time Input Control
    /// </summary>
    public class DateTimeInput :
        BaseTool<DateTimeInput.Config, DateTimeInput.Engine>, AlteryxGuiToolkit.Plugins.IPlugin
    {
        public enum DateToReturn
        {
            Now,
            Today,
            Yesterday,
            StartOfWeek,
            StartOfMonth,
            StartOfYear,
            PreviousMonthEnd,
            PreviousYearEnd
        }

        public class Config
        {
            /// <summary>
            /// Gets or sets the type of the output.
            /// </summary>
            [Category("Output")]
            [Description("Alteryx Type for the Output Field")]
            [TypeConverter(typeof(FixedListTypeConverter<OutputType>))]
            [FieldList(OutputType.Date, OutputType.DateTime, OutputType.Time, OutputType.String)]
            public OutputType OutputType { get; set; } = OutputType.DateTime;

            /// <summary>
            /// Gets or sets the name of the output field.
            /// </summary>
            [Category("Output")]
            [Description("Field Name To Use For Output Field")]
            public string OutputFieldName { get; set; } = "Date";

            /// <summary>
            /// Date To Return
            /// </summary>
            [Description("Value to Return")]
            public DateToReturn DateToReturn { get; set; } = DateToReturn.Today;

            /// <summary>
            /// ToString used for annotation
            /// </summary>
            /// <returns></returns>
            public override string ToString() => $"{this.OutputFieldName}={this.DateToReturn}";
        }

        public class Engine : BaseEngine<Config>
        {
            /// <summary>
            /// Constructor for Alteryx Engine
            /// </summary>
            public Engine()
                : base(null, null)
            {
            }

            /// <summary>
            /// Gets or sets the output.
            /// </summary>
            [CharLabel('O')]
            public OutputHelper Output { get; set; }

            /// <summary>
            /// Called only if you have no Input Connections
            /// </summary>
            /// <param name="nRecordLimit"></param>
            /// <returns></returns>
            public override bool PI_PushAllRecords(long nRecordLimit)
            {
                if (this.Output == null)
                {
                    this.Engine.OutputMessage(
                        this.NToolId,
                        AlteryxRecordInfoNet.MessageStatus.STATUS_Error,
                        "Output is not set.");
                    return false;
                }

                var fieldDescription = this.ConfigObject.OutputType.OutputDescription(this.ConfigObject.OutputFieldName, 19);
                if (fieldDescription == null)
                {
                    return false;
                }
                fieldDescription.Source = nameof(DateTimeInput);
                fieldDescription.Description = $"{this.ConfigObject.DateToReturn}";

                var recordInfo = Utilities.CreateRecordInfo(fieldDescription);

                this.Output.Init(recordInfo);
                if (nRecordLimit == 0)
                {
                    this.Output.Close(true);
                    return true;
                }

                var dateOutput = DateTime.Today;
                switch (this.ConfigObject.DateToReturn)
                {
                    case DateToReturn.Now:
                        dateOutput = DateTime.Now;
                        break;
                    case DateToReturn.Yesterday:
                        dateOutput = dateOutput.AddDays(-1);
                        break;
                    case DateToReturn.StartOfWeek:
                        dateOutput = dateOutput.AddDays(-(int)dateOutput.DayOfWeek);
                        break;
                    case DateToReturn.StartOfMonth:
                        dateOutput = dateOutput.AddDays(1 - dateOutput.Day);
                        break;
                    case DateToReturn.StartOfYear:
                        dateOutput = new DateTime(dateOutput.Year, 1, 1);
                        break;
                    case DateToReturn.PreviousMonthEnd:
                        dateOutput = dateOutput.AddDays(-dateOutput.Day);
                        break;
                    case DateToReturn.PreviousYearEnd:
                        dateOutput = new DateTime(dateOutput.Year - 1, 12, 31);
                        break;
                }

                var recordOut = this.Output.CreateRecord();
                this.Output[this.ConfigObject.OutputFieldName]?
                    .SetFromString(recordOut, dateOutput.ToString(this.ConfigObject.OutputType == OutputType.Time ? "HH:mm:ss" : "yyyy-MM-dd HH:mm:ss"));
                this.Output.Push(recordOut);
                this.Output.UpdateProgress(1.0);
                this.Output.Close(true);
                return true;
            }
        }
    }
}