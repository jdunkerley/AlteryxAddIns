namespace JDunkerley.AlteryxAddIns.Framework.Attributes
{
    using System;

    /// <summary>
    /// Specify Which Group A PlugIn Should Appear In and Display Name
    /// (Used by DefaultSettings Generator)
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
        /// Group Name To Use
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// Description (Display Name) for the PlugIn
        /// </summary>
        public string Description {  get; }
    }
}