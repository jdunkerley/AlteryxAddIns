namespace OmniBus.HTMLHelper
{
    internal class Connection
    {
        public Connection(string name, bool allowMultiple = false, bool optional = false, char label = '\0')
        {
            this.Name = name;
            this.AllowMultiple = allowMultiple;
            this.Optional = optional;
            this.Label = label;
        }

        public string Name { get; }

        public char Label { get; }

        public bool Optional { get; }

        public bool AllowMultiple { get; }

        private string LabelAsString => (this.Label == '\0' ? string.Empty : $"Label=\"{this.Label}\"");

        private static string BoolAsString(bool value) => value ? "True" : "False";

        public string MakeXml() => $"<Connection Name=\"{this.Name}\" Type=\"Connection\" Optional=\"{BoolAsString(this.Optional)}\" AllowMultiple=\"{BoolAsString(this.AllowMultiple)}\" {this.LabelAsString} />";
    }
}