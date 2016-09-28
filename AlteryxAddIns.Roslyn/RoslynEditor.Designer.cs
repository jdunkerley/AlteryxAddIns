namespace JDunkerley.AlteryxAddIns.Roslyn
{
    partial class RoslynEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Splitter splitter;
            this.dgvErrors = new System.Windows.Forms.DataGridView();
            splitter = new System.Windows.Forms.Splitter();
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrors)).BeginInit();
            this.SuspendLayout();
            // 
            // splitter
            // 
            splitter.Dock = System.Windows.Forms.DockStyle.Bottom;
            splitter.Location = new System.Drawing.Point(0, 348);
            splitter.Name = "splitter";
            splitter.Size = new System.Drawing.Size(517, 3);
            splitter.TabIndex = 1;
            splitter.TabStop = false;
            // 
            // dgvErrors
            // 
            this.dgvErrors.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvErrors.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvErrors.Location = new System.Drawing.Point(0, 351);
            this.dgvErrors.Name = "dgvErrors";
            this.dgvErrors.Size = new System.Drawing.Size(517, 150);
            this.dgvErrors.TabIndex = 2;
            // 
            // RoslynEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(splitter);
            this.Controls.Add(this.dgvErrors);
            this.Name = "RoslynEditor";
            this.Size = new System.Drawing.Size(517, 501);
            ((System.ComponentModel.ISupportInitialize)(this.dgvErrors)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvErrors;
    }
}
