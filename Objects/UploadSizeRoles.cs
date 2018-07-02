
namespace DNNConnect.CKEditorProvider.Objects
{
    /// <summary>
    /// Upload Size for a Role
    /// </summary>
    public class UploadSizeRoles
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating Role ID
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Gets or sets a value for Role Name
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating max upload file size
        /// </summary>
        public int UploadFileLimit { get; set; }

        #endregion
    }
}