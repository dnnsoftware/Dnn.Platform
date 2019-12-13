using DotNetNuke.Entities.Users;

namespace Dnn.ExportImport.Components.Interfaces
{
    /// <summary>
    /// Contract to convert all the datetiem properties in the class to User's local time
    /// </summary>
    internal interface IDateTimeConverter
    {
        /// <summary>
        /// Converts the datetime properties to user locale.
        /// </summary>
        /// <param name="userInfo"></param>
        void ConvertToLocal(UserInfo userInfo);
    }
}
