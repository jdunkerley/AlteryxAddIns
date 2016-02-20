namespace JDunkerley.Alteryx
{
    using JDunkerley.Alteryx.Framework;

    public class QuandlDownloadTool :
        BaseTool<QuandlDownloadTool.Config, QuandlDownloadTool.Engine>
    {
        public class Config
        {
            /// <summary>
            /// Gets or sets the name of the data base.
            /// </summary>
            public string DataBaseName { get; set; }

            /// <summary>
            /// Gets or sets the name of the data base.
            /// </summary>
            public string DataSetName { get; set; }

            /// <summary>
            /// Default Annotation
            /// </summary>
            public override string ToString() => $"{this.DataBaseName}/{this.DataSetName}";
        }

        public class Engine : BaseEngine<Config>
        {
            public override bool PI_PushAllRecords(long nRecordLimit)
            {
                return true;
            }
        }
    }
}