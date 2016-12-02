#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// All Rights Reserved
// 
#endregion
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using DotNetNuke.ComponentModel;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Web.Validators;

using Moq;

namespace DotNetNuke.Tests.PreviewProfileManagement.Mocks
{
    internal class MockHelper
    {
		public static Mock<T> CreateNew<T>() where T : class
		{
			if (ComponentFactory.Container == null)
			{
				ResetContainer();
			}

			//Try and get mock
			var mockComp = ComponentFactory.GetComponent<Mock<T>>();
			var objComp = ComponentFactory.GetComponent<T>();

			if (mockComp == null)
			{
				mockComp = new Mock<T>();
				ComponentFactory.RegisterComponentInstance<Mock<T>>(mockComp);
			}

			if (objComp == null)
			{
				ComponentFactory.RegisterComponentInstance<T>(mockComp.Object);
			}

			return mockComp;
		}

		public static void ResetContainer()
		{
			ComponentFactory.Container = new SimpleContainer();
		}
    }
}