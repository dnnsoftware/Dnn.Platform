namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    public struct ComplexType
    {
        public ComplexType(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
    }
}