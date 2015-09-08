#region Copyright

// 
// Copyright (c) _YEAR_
// by _OWNER_
// 

#endregion

#region Using Statements

using System.Collections.Generic;
using DotNetNuke.Data;

#endregion

namespace _OWNER_._MODULE_
{
    public class _CONTROL_Controller
    {
        public void Add_CONTROL_(_CONTROL_Info _CONTROL_)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<_CONTROL_Info>();
                rep.Insert(_CONTROL_);
            }
        }

        public void Delete_CONTROL_(_CONTROL_Info _CONTROL_)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<_CONTROL_Info>();
                rep.Delete(_CONTROL_);
            }
        }

        public IEnumerable<_CONTROL_Info> Get_CONTROL_s(int moduleId)
        {
            IEnumerable<_CONTROL_Info> _CONTROL_s;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<_CONTROL_Info>();
                _CONTROL_s = rep.Get(moduleId);
            }
            return _CONTROL_s;
        }

        public _CONTROL_Info Get_CONTROL_(int _CONTROL_Id, int moduleId)
        {
            _CONTROL_Info _CONTROL_;

            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<_CONTROL_Info>();
                _CONTROL_ = rep.GetById(_CONTROL_Id, moduleId);
            }
            return _CONTROL_;
        }

        public void Update_CONTROL_(_CONTROL_Info _CONTROL_)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<_CONTROL_Info>();
                rep.Update(_CONTROL_);
            }
        }
    }
}