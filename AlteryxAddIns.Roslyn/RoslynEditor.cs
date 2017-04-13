using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Microsoft.CodeAnalysis;

using Syncfusion.Windows.Forms.Edit;
using Syncfusion.Windows.Forms.Edit.Enums;

namespace JDunkerley.AlteryxAddIns.Roslyn
{
    public partial class RoslynEditor : UserControl
    {
        private readonly EditControl _textBox;

        private string _code;

        public RoslynEditor()
        {
            this.InitializeComponent();

            // Set up text box
            this._textBox = new EditControl { Dock = DockStyle.Fill };
            this._textBox.ApplyConfiguration(KnownLanguages.CSharp);
            this._textBox.TextChanged += (sender, args) =>
                {
                    this._code = this._textBox.Text;
                    this.CodeChanged(this, args);
                };
            this.Controls.Add(this._textBox);
        }

        public string Code
        {
            get => this._code;
            set
            {
                this._code = value;
                this._textBox.Text = value;
            }
        }

        /// <summary>
        ///     Event When Code Changed
        /// </summary>
        public event EventHandler CodeChanged = delegate { };

        /// <summary>
        ///     Sets the Error Messages
        /// </summary>
        public void SetMessages(IEnumerable<Diagnostic> value, int startLine, int startCharacter)
        {
            this.dgvErrors.DataSource = value.Select(
                    d =>
                        {
                            var start = d.Location.SourceTree.GetLineSpan(d.Location.SourceSpan);

                            return new DiagnosticMessate
                                       {
                                           Severity = d.Severity,
                                           Message = d.GetMessage(),
                                           StartLine = start.StartLinePosition.Line - startLine,
                                           StartCharacter =
                                               start.StartLinePosition.Character
                                               - (start.StartLinePosition.Line == startLine
                                                      ? startCharacter
                                                      : 0)
                                       };
                        })
                .ToArray();
        }

        private class DiagnosticMessate
        {
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public DiagnosticSeverity Severity { get; set; }

            public string Message { get; set; }

            public int StartLine { get; set; }

            public int StartCharacter { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
        }
    }
}