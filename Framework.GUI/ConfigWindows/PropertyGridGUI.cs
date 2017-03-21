using System.Windows.Forms;

namespace OmniBus.Framework.ConfigWindows
{
    /// <summary>
    /// Simple Property Grid Based Configuration Panel
    /// </summary>
    /// <typeparam name="T">Configuration Object</typeparam>
    /// <seealso cref="System.Windows.Forms.UserControl" />
    /// <seealso cref="AlteryxGuiToolkit.Plugins.IPluginConfiguration" />
    public class PropertyGridGui<T> : BaseGui<T>
        where T : new()
    {
        /// <summary>
        /// The _property grid
        /// </summary>
        private readonly PropertyGrid _propertyGrid;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGridGui{T}"/> class.
        /// </summary>
        public PropertyGridGui()
        {
            this._propertyGrid = new PropertyGrid
                                     {
                                         PropertySort = PropertySort.CategorizedAlphabetical,
                                     };
            this.AddControl(this._propertyGrid);
        }

        /// <summary>
        /// Called when the <see cref="BaseGui{T}.Config"/> object is set up
        /// </summary>
        protected override void OnObjectSet()
        {
            this._propertyGrid.SelectedObject = this.Config;
        }
    }
}