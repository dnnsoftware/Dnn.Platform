#region Usings

using System.Data;

#endregion

namespace DotNetNuke.Entities.Modules
{
    public interface IHydratable
    {
        int KeyID { get; set; }

        void Fill(IDataReader dr);
    }
}
