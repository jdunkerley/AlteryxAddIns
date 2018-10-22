using System.ComponentModel;

namespace OmniBus.Reporting
{
    public class TableReportConfig
    {
        [TypeConverter()]
        public string FileName { get; set; } = "test.pdf";
    }
}
