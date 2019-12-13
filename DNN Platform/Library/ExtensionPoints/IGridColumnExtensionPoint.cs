using System.Web.UI.WebControls;

namespace DotNetNuke.ExtensionPoints
{
    public interface IGridColumnExtensionPoint : IExtensionPoint
    {
        int ColumnAt { get; }
        string UniqueName { get; }
        string DataField { get; }
        string HeaderText { get; }
        Unit HeaderStyleWidth { get; }
        bool ReadOnly { get; }
        bool Reorderable { get; }
        string SortExpression { get; }
    }
}
