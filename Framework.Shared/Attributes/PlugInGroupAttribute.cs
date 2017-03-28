using System;

namespace OmniBus.Framework.Attributes
{
    /// <summary>
    ///     Specifies which group a PlugIn tool should appear in and display name.
    ///     (Used to write to DefaultSettings.xml)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class PlugInGroupAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PlugInGroupAttribute" /> class.
        /// </summary>
        /// <param name="catergoryName">Target Category Name</param>
        /// <param name="toolName">Tool Name to Dispaly</param>
        /// <param name="description">Description for the PlugIn</param>
        public PlugInGroupAttribute(string catergoryName, string toolName = null, string description = null)
        {
            this.CatergoryName = catergoryName;
            this.ToolName = toolName;
            this.Description = description;
        }

        /// <summary>
        ///     Gets the group name to use for the <see cref="AlteryxRecordInfoNet.INetPlugin" />.
        /// </summary>
        public string CatergoryName { get; }

        /// <summary>
        ///     Gets the display name to use for the <see cref="AlteryxRecordInfoNet.INetPlugin" />
        /// </summary>
        public string ToolName { get; }

        /// <summary>
        ///     Gets the description (Display Name) for the <see cref="AlteryxRecordInfoNet.INetPlugin" />.
        /// </summary>
        public string Description { get; }
    }
}