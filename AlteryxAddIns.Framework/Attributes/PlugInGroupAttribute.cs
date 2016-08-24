namespace JDunkerley.AlteryxAddIns.Framework.Attributes
{
    using System;

    /// <summary>
    /// Specifies which group a PlugIn tool should appear in and display name.
    /// (Used to write to DefaultSettings.xml)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class PlugInGroupAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlugInGroupAttribute"/> class.
        /// </summary>
        /// <param name="groupName">Target Group Name</param>
        /// <param name="description">Description for the PlugIn</param>
        public PlugInGroupAttribute(string groupName, string description = null)
        {
            this.GroupName = groupName;
            this.Description = description;
        }

        /// <summary>
        /// Gets the group name to use for the <see cref="AlteryxRecordInfoNet.INetPlugin"/>.
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// Gets the description (Display Name) for the <see cref="AlteryxRecordInfoNet.INetPlugin"/>.
        /// </summary>
        public string Description { get; }
    }
}