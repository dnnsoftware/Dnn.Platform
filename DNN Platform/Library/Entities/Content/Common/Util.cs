#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Content.Taxonomy;

#endregion

namespace DotNetNuke.Entities.Content.Common
{
	/// <summary>
	/// Utility Methods for Content.
	/// </summary>
    public static class Util
    {
		/// <summary>
		/// Gets the data service.
		/// </summary>
		/// <returns>data service instance from ComponentFactory.</returns>
        public static IDataService GetDataService()
        {
            var ds = ComponentFactory.GetComponent<IDataService>();

            if (ds == null)
            {
                ds = new DataService();
                ComponentFactory.RegisterComponentInstance<IDataService>(ds);
            }
            return ds;
        }

		/// <summary>
		/// Gets the content controller.
		/// </summary>
		/// <returns>ContentController from ComponentFactory.</returns>
        public static IContentController GetContentController()
        {
            var ctl = ComponentFactory.GetComponent<IContentController>();

            if (ctl == null)
            {
                ctl = new ContentController();
                ComponentFactory.RegisterComponentInstance<IContentController>(ctl);
            }
            return ctl;
        }

		/// <summary>
		/// Gets the scope type controller.
		/// </summary>
		/// <returns>ScopeTypeController from ComponentFactory.</returns>
        public static IScopeTypeController GetScopeTypeController()
        {
            var ctl = ComponentFactory.GetComponent<IScopeTypeController>();

            if (ctl == null)
            {
                ctl = new ScopeTypeController();
                ComponentFactory.RegisterComponentInstance<IScopeTypeController>(ctl);
            }
            return ctl;
        }

		/// <summary>
		/// Gets the term controller.
		/// </summary>
		/// <returns>TermController from ComponentFactory.</returns>
        public static ITermController GetTermController()
        {
            var ctl = ComponentFactory.GetComponent<ITermController>();

            if (ctl == null)
            {
                ctl = new TermController();
                ComponentFactory.RegisterComponentInstance<ITermController>(ctl);
            }
            return ctl;
        }

		/// <summary>
		/// Gets the vocabulary controller.
		/// </summary>
		/// <returns>VocabularyController from ComponentFactory.</returns>
        public static IVocabularyController GetVocabularyController()
        {
            var ctl = ComponentFactory.GetComponent<IVocabularyController>();

            if (ctl == null)
            {
                ctl = new VocabularyController();
                ComponentFactory.RegisterComponentInstance<IVocabularyController>(ctl);
            }
            return ctl;
        }
    }
}