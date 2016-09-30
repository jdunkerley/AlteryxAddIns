using System.Windows.Forms;

namespace JDunkerley.AlteryxAddIns.Roslyn
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.CodeAnalysis;

    using Syncfusion.Windows.Forms.Edit.Enums;

    public partial class RoslynEditor : UserControl
    {
        private readonly Syncfusion.Windows.Forms.Edit.EditControl _textBox;

        private string _code;

        public RoslynEditor()
        {
            this.InitializeComponent();

            // Set up text box
            this._textBox = new Syncfusion.Windows.Forms.Edit.EditControl { Dock = DockStyle.Fill };
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
            get
            {
                return this._code;
            }
            set
            {
                this._code = value;
                this._textBox.Text = value;
            }
        }


        /// <summary>
        /// Event When Code Changed
        /// </summary>
        public event EventHandler CodeChanged = delegate { };

        /// <summary>
        /// Sets the Error Messages
        /// </summary>
        public IEnumerable<Diagnostic> Messages
        {
            set
            {
                this.dgvErrors.DataSource = value.Select(d => new
                                                                  {
                                                                      d.Severity,
                                                                      Message = d.GetMessage()
                                                                  });
            }
        }
    }
}
