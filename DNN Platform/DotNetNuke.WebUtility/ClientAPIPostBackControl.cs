#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2015
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

using System;
using System.Collections;
using System.Diagnostics;
using System.Web.UI;

namespace DotNetNuke.UI.Utilities
{

	/// -----------------------------------------------------------------------------
	/// Project	 : DotNetNuke
	/// Class	 : ClientAPIPostBackControl
	/// 
	/// -----------------------------------------------------------------------------
	/// <summary>
	/// Control used to register post-back events
	/// </summary>
	/// <remarks>
	/// In order for a post-back event to be trapped we need to associate a control to 
	/// handle the event.
	/// </remarks>
	/// <history>
	/// 	[Jon Henning]	9/15/2004	Created
	/// </history>
	/// -----------------------------------------------------------------------------
	public class ClientAPIPostBackControl : Control, IPostBackEventHandler
	{
		public delegate void PostBackEvent(ClientAPIPostBackEventArgs Args);

		private Hashtable m_oEventHandlers = new Hashtable();
		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Property to access individual post back event handlers based off of event name
		/// </summary>
		/// <param name="strEventName">Event Name</param>
		/// <returns>PostBackEvent</returns>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	9/15/2004	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public PostBackEvent EventHandlers(string strEventName) {
			if (m_oEventHandlers.Contains(strEventName)) {
				return (PostBackEvent)m_oEventHandlers[strEventName];
			} else {
				return null;
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Adds a postback event handler to the control
		/// </summary>
		/// <param name="strEventName">Event Name</param>
		/// <param name="objDelegate">Delegate for Function of type PostBackEvent</param>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	9/15/2004	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public void AddEventHandler(string strEventName, PostBackEvent objDelegate)
		{
			if (m_oEventHandlers.Contains(strEventName) == false) {
				m_oEventHandlers.Add(strEventName, objDelegate);
			} else {
				m_oEventHandlers[strEventName] = (ClientAPIPostBackControl.PostBackEvent)System.Delegate.Combine((ClientAPIPostBackControl.PostBackEvent)m_oEventHandlers[strEventName], objDelegate);
			}
		}

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="objPage">Page</param>
		/// <param name="strEventName">Event Name</param>
		/// <param name="objDelegate">Delegate for Function of type PostBackEvent</param>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	9/15/2004	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public ClientAPIPostBackControl(Page objPage, string strEventName, PostBackEvent objDelegate)
		{
			ClientAPI.GetPostBackClientEvent(objPage, this, "");
			AddEventHandler(strEventName, objDelegate);
		}

		public ClientAPIPostBackControl()
		{
		}
		/// -----------------------------------------------------------------------------
		/// <summary>
		/// Function implementing IPostBackEventHandler which allows the ASP.NET page to invoke
		/// the control's events
		/// </summary>
		/// <param name="strEventArgument"></param>
		/// <remarks>
		/// </remarks>
		/// <history>
		/// 	[Jon Henning]	9/15/2004	Created
		/// </history>
		/// -----------------------------------------------------------------------------
		public void RaisePostBackEvent(string strEventArgument)
		{
			ClientAPIPostBackEventArgs objArg = new ClientAPIPostBackEventArgs(strEventArgument);
			if ((EventHandlers(objArg.EventName) != null)) {
				EventHandlers(objArg.EventName)(objArg);
			}
		}
	}

}
