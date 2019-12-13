#region Usings

using System;
using System.Collections;

#endregion

namespace DotNetNuke.Entities.Urls.Config
{
    [Serializable]
    public class RewriterRuleCollection : CollectionBase
    {
        public virtual RewriterRule this[int index]
        {
            get
            {
                return (RewriterRule) List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        public void Add(RewriterRule r)
        {
            InnerList.Add(r);
        }
    }
}
