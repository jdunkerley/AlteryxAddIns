using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using AlteryxGuiToolkit.Plugins;
using AlteryxRecordInfoNet;
using OmniBus.Framework;
using OmniBus.Framework.Attributes;
using OmniBus.Framework.EventHandlers;
using OmniBus.Framework.Interfaces;

namespace OmniBus.XmlTools
{
    [PlugInGroup("Omnibus", "Command")]
    public class Command : BaseTool<Command.Config, Command.Engine>, IPlugin
    {
        public class Config
        {
            public string Command { get; set; } = @"hostname";

            public string FieldName { get; set; } = "Output";

            public int TimeInMS { get; set; } = 1000;

            public override string ToString() => this.Command;
        }

        public class Engine : BaseEngine<Config>
        {
            /// <summary>Gets the input stream.</summary>
            public IInputProperty Input { get; }

            /// <summary>Gets or sets the output stream.</summary>
            public IOutputHelper Output { get; set; }

            private Func<RecordData, string> _commandFunc;

            public Engine()
            {
                this.Input = new InputProperty(this);
                this.Input.InitCalled += OnInputOnInitCalled;
                this.Input.RecordPushed += Input_RecordPushed;
                this.Input.ProgressUpdated += (s, p) => this.Output?.UpdateProgress(p, true);
                this.Input.Closed += (s) => this.Output.Close(true);
            }

            private void OnInputOnInitCalled(IInputProperty i, SuccessEventArgs s)
            {
                this.Output?.Init(new RecordInfoBuilder().AddFields(i.RecordInfo)
                    .ReplaceFields(new FieldDescription(this.ConfigObject.FieldName, FieldType.E_FT_V_WString, FieldDescription.MaxStringLength))
                    .Build());

                var regex = new Regex("{{(.*?)}}");
                var matches = regex.Matches(this.ConfigObject.Command);

                var dict = new Dictionary<string, FieldBase>();
                foreach (Match match in matches)
                {
                    var key = match.Value;
                    var field = i.RecordInfo.GetFieldByName(key.Substring(2, key.Length - 4), false);
                    if (field == null)
                    {
                        s.SetFailed();
                        return;
                    }

                    dict[key] = field;
                }

                this._commandFunc = d =>
                {
                    var cmd = this.ConfigObject.Command;

                    foreach (var kvp in dict)
                    {
                        cmd = cmd.Replace(kvp.Key, kvp.Value.GetAsString(d));
                    }

                    return cmd;
                };
            }

            private void Input_RecordPushed(IInputProperty a, AlteryxRecordInfoNet.RecordData data,
                Framework.EventHandlers.SuccessEventArgs success)
            {
                var cmd = this._commandFunc(data);
                int index = cmd.IndexOf(cmd.StartsWith("\"") ? "\"" : " ", 1, StringComparison.InvariantCultureIgnoreCase);
                var args = index == -1 ? "" : cmd.Substring(index + 1);
                cmd = index == -1 ? cmd : cmd.Substring(0, index + 1).Trim();

                var proc = new System.Diagnostics.Process
                {
                    StartInfo = new ProcessStartInfo(cmd, args)
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                proc.Start();
                proc.WaitForExit(this.ConfigObject.TimeInMS);

                this.Output.Record.Reset();
                this.Input.Copier.Copy(this.Output.Record, data);
                var field = this.Output.RecordInfo.GetFieldByName(this.ConfigObject.FieldName, false);
                field.SetFromString(this.Output.Record, proc.StandardOutput.ReadToEnd().Trim());
                this.Output.Push(this.Output.Record);
            }
        }
    }
}
