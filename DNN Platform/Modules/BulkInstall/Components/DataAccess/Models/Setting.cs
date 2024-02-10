using DotNetNuke.ComponentModel.DataAnnotations;

namespace DotNetNuke.BulkInstall.Components.DataAccess.Models
{
    [TableName("Cantarus_PolyDeploy_Settings")]
    [PrimaryKey("SettingID")]
    public class Setting
    {
        public int SettingId { get; set; }
        public string Group { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
