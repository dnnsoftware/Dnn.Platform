#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Web.UI.WebControls;

using DotNetNuke.Services.Localization;


#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnGridColumn : BoundColumn
    {
        //#region Public Properties

        //public string LocalResourceFile
        //{
        //    get
        //    {
        //        return Utilities.GetLocalResourceFile(Owner.OwnerGrid.Parent);
        //    }
        //}

        //#endregion

        //#region Public Methods

        //public override GridColumn Clone()
        //{
        //    var dnnGridColumn = new DnnGridColumn();
        //    dnnGridColumn.CopyBaseProperties(this);
        //    dnnGridColumn.setHeaderText = HeaderText;
        //    return dnnGridColumn;
        //}

        //private String _HeaderText;

        //public override string HeaderText
        //{
        //    get
        //    {
        //        if (String.IsNullOrEmpty(base.HeaderText))
        //            base.HeaderText = Localization.GetString(string.Format("{0}.Header", _HeaderText), DotNetNuke.Web.UI.Utilities.GetLocalResourceFile(Owner.OwnerGrid.Parent));
        //        return base.HeaderText;
        //    }
        //    set
        //    {
        //        _HeaderText = value;
        //        base.HeaderText = "";
        //    }
        //}

        //public String setHeaderText
        //{
        //    set
        //    {
        //        base.HeaderText = value;
        //    }
        //}


        //#endregion
    }
}
