namespace JDunkerley.AlteryxAddIns.Roslyn
{
    using System;
    using System.Threading.Tasks;

    using Framework.ConfigWindows;

    /// <summary>
    /// Roslyn Editor based Config Window
    /// </summary>
    public class RoslynEditorGui : BaseGui<RoslynInput.Config>
    {
        private readonly RoslynEditor _editor;

        public RoslynEditorGui(Func<string, string> getCodeBlock)
        {
            this.GetCodeBlock = getCodeBlock;

            this._editor = new RoslynEditor();
            this._editor.CodeChanged += this.EditorOnCodeChanged;
            this.AddControl(this._editor);
        }

        public Func<string, string> GetCodeBlock { get; }

        private async void EditorOnCodeChanged(object sender, EventArgs eventArgs)
        {
            this.Config.LambdaCode = this._editor.Code;
            var lambda = this._editor.Code;

            var code = this.GetCodeBlock(lambda);
            var result = await Task.Run(() => Compiler.Compile(code));

            if (this._editor.Code == lambda)
            {
                this._editor.SetMessages(result.Messages, 10, 13);
            }
        }

        /// <summary>
        /// Called when the <see cref="BaseGui{T}.Config"/> object is set up
        /// </summary>
        protected override void OnObjectSet()
        {
            this._editor.Code = this.Config.LambdaCode;
        }
    }
}