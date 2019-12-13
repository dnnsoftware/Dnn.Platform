#region Usings

using System.Collections;

#endregion

namespace DotNetNuke.Entities.Modules.Communications
{
    public class ModuleListeners : CollectionBase
    {
        public IModuleListener this[int index]
        {
            get
            {
                return (IModuleListener) List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        public int Add(IModuleListener item)
        {
            return List.Add(item);
        }
    }
}
