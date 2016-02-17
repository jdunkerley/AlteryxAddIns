namespace JDunkerley.Alteryx
{
    using System;

    using AlteryxGuiToolkit.Plugins;
    using AlteryxRecordInfoNet;


    /// <summary>
    /// Simple Date Time Parsing Control
    /// </summary>
    public class DateTimeInputTool : 
        BaseTool<DateTimeInputTool.Config, DateTimeInputTool.Engine>, IPlugin
    {
        public enum DateToReturn
        {
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
            /// Return A DateTime Instead Of A Date
            /// </summary>
            public bool ReturnDateTime { get; set; }

            /// <summary>
            /// Field Name For Output
            /// </summary>
            public string OutputFieldName { get; set; } = "Date";

            /// <summary>
            /// Date To Return
            /// </summary>
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
            /// Called only if you have no Input Connections
            /// </summary>
            /// <param name="nRecordLimit"></param>
            /// <returns></returns>
            public override bool PI_PushAllRecords(long nRecordLimit)
            {
                var config = this.GetConfigObject();
                string outputFieldName = config?.OutputFieldName ?? "Date";

                var recordInfo = new RecordInfo();
                recordInfo.AddField(outputFieldName, config?.ReturnDateTime ?? false ? FieldType.E_FT_DateTime : FieldType.E_FT_Date);

                this.OutputHelper?.Init(recordInfo, "Output", null, this.XmlConfig);
                if (nRecordLimit == 0)
                {
                    this.Engine?.OutputMessage(this.NToolId, MessageStatus.STATUS_Complete, "");
                    this.OutputHelper?.Close();
                    return true;
                }

                var dateOutput = DateTime.Today;
                switch (config?.DateToReturn ?? DateToReturn.Today)
                {
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

                var recordOut = recordInfo.CreateRecord();
                recordInfo.GetFieldByName(outputFieldName, false)?.SetFromString(recordOut, dateOutput.ToString("yyyy-MM-dd HH:mm:ss"));
                this.OutputHelper?.PushRecord(recordOut.GetRecord());
                this.OutputHelper?.UpdateProgress(1.0);
                this.OutputHelper?.OutputRecordCount(true);

                this.Engine?.OutputMessage(this.NToolId, MessageStatus.STATUS_Complete, "");
                this.OutputHelper?.Close();
                return true;
            }
        }
    }
}