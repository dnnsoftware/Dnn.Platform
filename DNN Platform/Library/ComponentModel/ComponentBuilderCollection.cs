#region Usings

using DotNetNuke.Collections.Internal;

#endregion

namespace DotNetNuke.ComponentModel
{
    internal class ComponentBuilderCollection : SharedDictionary<string, IComponentBuilder>
    {
        internal IComponentBuilder DefaultBuilder { get; set; }

        internal void AddBuilder(IComponentBuilder builder, bool setDefault)
        {
            if (!ContainsKey(builder.Name))
            {
                this[builder.Name] = builder;

                if (setDefault && DefaultBuilder == null)
                {
                    DefaultBuilder = builder;
                }
            }
        }
    }
}
