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

            public string ErrorFieldName { get; set; } = "Error";

            public string ExitCodeFieldName { get; set; } = "ExitCode";

            public int TimeOutMilliSeconds { get; set; } = 1000;

            public override string ToString() => this.Command;
        }

        public class Engine : BaseEngine<Config>
        {
            /// <summary>Gets the input stream.</summary>
            public IInputProperty Input { get; }

            /// <summary>Gets or sets the output stream.</summary>
            public IOutputHelper Output { get; set; }

            private Func<RecordData, string> _commandFunc;
            private Action<Record, (string Output, string Error, int ExitCode)> _setResult;

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
                var recordInfo = new RecordInfoBuilder().AddFields(i.RecordInfo)
                    .ReplaceFields(new FieldDescription(this.ConfigObject.FieldName, FieldType.E_FT_V_WString))
                    .ReplaceFields(new FieldDescription(this.ConfigObject.ErrorFieldName, FieldType.E_FT_V_WString))
                    .ReplaceFields(new FieldDescription(this.ConfigObject.ExitCodeFieldName, FieldType.E_FT_Int32))
                    .Build();
                this.Output?.Init(recordInfo);

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

                var output = recordInfo.GetFieldByName(this.ConfigObject.FieldName, true);
                var error = recordInfo.GetFieldByName(this.ConfigObject.ErrorFieldName, true);
                var exitcode = recordInfo.GetFieldByName(this.ConfigObject.ExitCodeFieldName, true);
                this._setResult = (record, t) =>
                {
                    if (t.Output == null) output.SetNull(record);
                    else output.SetFromString(record, t.Output);
                    if (t.Error == null) error.SetNull(record);
                    else error.SetFromString(record, t.Error);
                    exitcode.SetFromInt32(record, t.ExitCode);
                };
            }

            private void Input_RecordPushed(IInputProperty a, AlteryxRecordInfoNet.RecordData data,
                Framework.EventHandlers.SuccessEventArgs success)
            {
                var cmd = this._commandFunc(data);
                int index = cmd.IndexOf(cmd.StartsWith("\"") ? "\"" : " ", 1, StringComparison.InvariantCultureIgnoreCase);
                var args = index == -1 ? "" : cmd.Substring(index + 1);
                cmd = index == -1 ? cmd : cmd.Substring(0, index + 1).Trim();

                var response = ExecuteProcess(cmd, args, this.ConfigObject.TimeOutMilliSeconds);

                this.Output.Record.Reset();
                this.Input.Copier.Copy(this.Output.Record, data);
                this._setResult(this.Output.Record, response);
                this.Output.Push(this.Output.Record);
            }

            private static (string Output, string Error, int ExitCode) ExecuteProcess(string cmd, string args, int timeoutMilliSeconds)
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo(cmd, args)
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                var outputStream = new System.Text.StringBuilder();
                proc.OutputDataReceived += (s, e) =>
                {
                    lock (outputStream)
                    {
                        outputStream.AppendLine(e.Data);
                    }
                };

                var errorStream = new System.Text.StringBuilder();
                proc.ErrorDataReceived += (s, e) =>
                {
                    lock (errorStream)
                    {
                        errorStream.AppendLine(e.Data);
                    }
                };

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                if (proc.WaitForExit(timeoutMilliSeconds))
                {
                    int exit = proc.ExitCode;
                    proc.Close();
                    return (Output: outputStream.ToString(), Error: errorStream.ToString(), ExitCode: exit);
                }

                return (null, null, -1);
            }
        }
    }
}
