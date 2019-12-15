// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Web.UI;

namespace DotNetNuke.Web.DDRMenu.TemplateEngine
{
	internal interface ITemplateProcessor
	{
		bool LoadDefinition(TemplateDefinition baseDefinition);
		void Render(object source, HtmlTextWriter htmlWriter, TemplateDefinition liveDefinition);
	}
}
