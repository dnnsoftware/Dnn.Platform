
namespace DNNConnect.CKEditorProvider.Objects
{
    /// <summary>
    /// A Editor Host Setting Item
    /// </summary>
    public class EditorHostSetting
    {
        #region Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorHostSetting"/> class.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        public EditorHostSetting(string settingName, string settingValue)
        {
            this.Name = settingName;
            this.Value = settingValue;
        }

        /// <summary>
        /// Gets or sets the setting name.
        /// </summary>
        /// <value>
        /// The setting name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the setting value.
        /// </summary>
        /// <value>
        /// The setting value.
        /// </value>
        public string Value { get; set; }

        #endregion
    }
}