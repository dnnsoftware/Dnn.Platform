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
    public class [CONTROL]Controller
    {
        public void Add[CONTROL]([CONTROL]Info [CONTROL])
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<[CONTROL]Info>();
                rep.Insert([CONTROL]);
            }
        }

        public void Delete[CONTROL]([CONTROL]Info [CONTROL])
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<[CONTROL]Info>();
                rep.Delete([CONTROL]);
            }
        }

        public IEnumerable<[CONTROL]Info> Get[CONTROL]s(int moduleId)
        {
            IEnumerable<[CONTROL]Info> [CONTROL]s;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<[CONTROL]Info>();
                [CONTROL]s = rep.Get(moduleId);
            }
            return [CONTROL]s;
        }

        public [CONTROL]Info Get[CONTROL](int [CONTROL]Id, int moduleId)
        {
            [CONTROL]Info [CONTROL];

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<[CONTROL]Info>();
                [CONTROL] = rep.GetById([CONTROL]Id, moduleId);
            }
            return [CONTROL];
        }

        public void Update[CONTROL]([CONTROL]Info [CONTROL])
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<[CONTROL]Info>();
                rep.Update([CONTROL]);
            }
        }
    }
}