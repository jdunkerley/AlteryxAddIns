using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;

namespace JDunkerley.AlteryxAddIns.Roslyn
{
    public class RoslynInputConfig
    {
        /// <summary>
        /// Gets or sets the type of the output.
        /// </summary>
        [Category("Output")]
        [Description("Lambda Code for C#")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string LambdaCode { get; set; } = "() => new { A = \"Hello World\" }";
    }
}