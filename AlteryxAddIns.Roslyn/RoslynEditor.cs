using System.Windows.Forms;

namespace JDunkerley.AlteryxAddIns.Roslyn
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.CodeAnalysis;

    public partial class RoslynEditor : UserControl
    {
        private readonly TextBox _textBox;

        public RoslynEditor()
        {
            this.InitializeComponent();

            // Set up text box
            this._textBox = new TextBox { Multiline = true, Dock = DockStyle.Fill };
            this._textBox.TextChanged += (sender, args) => this.CodeChanged(this, args);
            this.Controls.Add(this._textBox);
        }

        public string Code
        {
            get
            {
                return this._textBox.Text;
            }
            set
            {
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
