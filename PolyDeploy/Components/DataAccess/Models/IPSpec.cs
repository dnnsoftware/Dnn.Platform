using DotNetNuke.ComponentModel.DataAnnotations;

namespace Cantarus.Modules.PolyDeploy.Components.DataAccess.Models
{
    [TableName("Cantarus_PolyDeploy_IPSpecs")]
    [PrimaryKey("IPSpecID")]
    public class IPSpec
    {
        public int IPSpecId { get; set; }
        public string Address { get; set; }

        public IPSpec () { }

        public IPSpec (string address)
        {
            Address = address;
        }
    }
}
