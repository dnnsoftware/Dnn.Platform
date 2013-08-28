#region Copyright

// 
// Copyright (c) [YEAR]
// by [OWNER]
// 

#endregion

#region Using Statements

using System.Collections.Generic;
using DotNetNuke.Data;

#endregion

namespace [OWNER].[MODULE]
{
    public class [MODULE]Controller
    {
        public void Add[MODULE]([MODULE]Info [MODULE])
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<[MODULE]Info>();
                rep.Insert([MODULE]);
            }
        }

        public void Delete[MODULE]([MODULE]Info [MODULE])
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<[MODULE]Info>();
                rep.Delete([MODULE]);
            }
        }

        public IEnumerable<[MODULE]Info> Get[MODULE]s(int moduleId)
        {
            IEnumerable<[MODULE]Info> [MODULE]s;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<[MODULE]Info>();
                [MODULE]s = rep.Get(moduleId);
            }
            return [MODULE]s;
        }

        public [MODULE]Info Get[MODULE](int [MODULE]Id, int moduleId)
        {
            [MODULE]Info [MODULE];

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<[MODULE]Info>();
                [MODULE] = rep.GetById([MODULE]Id, moduleId);
            }
            return [MODULE];
        }

        public void Update[MODULE]([MODULE]Info [MODULE])
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<[MODULE]Info>();
                rep.Update([MODULE]);
            }
        }
    }
}