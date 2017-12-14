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

using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;

#endregion

namespace DotNetNuke.Entities.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project	 :  DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Modules
    /// Class	 :  UserUserControlBase
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserUserControlBase class defines a custom base class for the User Control.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class UserUserControlBase : UserModuleBase
    {
        #region Delegates
        public delegate void UserCreatedEventHandler(object sender, UserCreatedEventArgs e);
        public delegate void UserDeletedEventHandler(object sender, UserDeletedEventArgs e);
        public delegate void UserRestoredEventHandler(object sender, UserRestoredEventArgs e);
        public delegate void UserRemovedEventHandler(object sender, UserRemovedEventArgs e);
        public delegate void UserUpdateErrorEventHandler(object sender, UserUpdateErrorArgs e);
        #endregion

        #region "Events"
        public event UserCreatedEventHandler UserCreated;
        public event UserCreatedEventHandler UserCreateCompleted;
        public event UserDeletedEventHandler UserDeleted;
        public event UserUpdateErrorEventHandler UserDeleteError;
        public event UserRestoredEventHandler UserRestored;
        public event UserUpdateErrorEventHandler UserRestoreError;
        public event UserRemovedEventHandler UserRemoved;
        public event UserUpdateErrorEventHandler UserRemoveError;
        public event EventHandler UserUpdated;
        public event EventHandler UserUpdateCompleted;
        public event UserUpdateErrorEventHandler UserUpdateError;
        #endregion

        #region "Event Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the UserCreateCompleted Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnUserCreateCompleted(UserCreatedEventArgs e)
        {
            if (UserCreateCompleted != null)
            {
                UserCreateCompleted(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the UserCreated Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnUserCreated(UserCreatedEventArgs e)
        {
            if (UserCreated != null)
            {
                UserCreated(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the UserDeleted Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnUserDeleted(UserDeletedEventArgs e)
        {
            if (UserDeleted != null)
            {
                UserDeleted(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the UserDeleteError Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnUserDeleteError(UserUpdateErrorArgs e)
        {
            if (UserDeleteError != null)
            {
                UserDeleteError(this, e);
            }
        }

        public void OnUserRestored(UserRestoredEventArgs e)
        {
            if (UserRestored != null)
            {
                UserRestored(this, e);
            }
        }

        public void OnUserRestoreError(UserUpdateErrorArgs e)
        {
            if (UserRestoreError != null)
            {
                UserRestoreError(this, e);
            }
        }

        public void OnUserRemoved(UserRemovedEventArgs e)
        {
            if (UserRemoved != null)
            {
                UserRemoved(this, e);
            }
        }

        public void OnUserRemoveError(UserUpdateErrorArgs e)
        {
            if (UserRemoveError != null)
            {
                UserRemoveError(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the UserUpdated Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnUserUpdated(EventArgs e)
        {
            if (UserUpdated != null)
            {
                UserUpdated(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the UserUpdated Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnUserUpdateCompleted(EventArgs e)
        {
            if (UserUpdateCompleted != null)
            {
                UserUpdateCompleted(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the UserUpdateError Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnUserUpdateError(UserUpdateErrorArgs e)
        {
            if (UserUpdateError != null)
            {
                UserUpdateError(this, e);
            }
        }

        #endregion

        #region "Properties"

        protected override bool AddUser => !Request.IsAuthenticated || base.AddUser;

        #endregion

        #region Nested type: BaseUserEventArgs

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The BaseUserEventArgs class provides a base for User EventArgs classes
        /// </summary>
        /// -----------------------------------------------------------------------------
        public class BaseUserEventArgs
        {
            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets and sets the Id of the User
            /// </summary>
            /// -----------------------------------------------------------------------------
            public int UserId { get; set; }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets and sets the Id of the User
            /// </summary>
            /// -----------------------------------------------------------------------------
            public string UserName { get; set; }
        }

        #endregion

        #region Nested type: UserCreatedEventArgs

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UserCreatedEventArgs class provides a customised EventArgs class for
        /// the UserCreated Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        public class UserCreatedEventArgs
        {
            private UserCreateStatus _createStatus = UserCreateStatus.Success;

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Constructs a new UserCreatedEventArgs
            /// </summary>
            /// <param name="newUser">The newly Created User</param>
            /// -----------------------------------------------------------------------------
            public UserCreatedEventArgs(UserInfo newUser)
            {
                NewUser = newUser;
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets and sets the Create Status
            /// </summary>
            /// -----------------------------------------------------------------------------
            public UserCreateStatus CreateStatus
            {
                get
                {
                    return _createStatus;
                }
                set
                {
                    _createStatus = value;
                }
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets and sets the New User
            /// </summary>
            /// -----------------------------------------------------------------------------
            public UserInfo NewUser { get; set; }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets and sets a flag whether to Notify the new User of the Creation
            /// </summary>
            /// -----------------------------------------------------------------------------
            public bool Notify { get; set; }
        }

        #endregion

        #region Nested type: UserDeletedEventArgs

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UserDeletedEventArgs class provides a customised EventArgs class for
        /// the UserDeleted Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        public class UserDeletedEventArgs : BaseUserEventArgs
        {
            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Constructs a new UserDeletedEventArgs
            /// </summary>
            /// <param name="id">The Id of the User</param>
            /// <param name="name">The user name of the User</param>
            /// -----------------------------------------------------------------------------
            public UserDeletedEventArgs(int id, string name)
            {
                UserId = id;
                UserName = name;
            }
        }

        #endregion

        #region Nested type: UserRestoredEventArgs

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// The UserRestoredEventArgs class provides a customised EventArgs class for
		/// the UserRestored Event
		/// </summary>
		/// -----------------------------------------------------------------------------
        public class UserRestoredEventArgs : BaseUserEventArgs
        {
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Constructs a new UserRestoredEventArgs
			/// </summary>
			/// <param name="id">The Id of the User</param>
			/// <param name="name">The user name of the User</param>
			/// -----------------------------------------------------------------------------
            public UserRestoredEventArgs(int id, string name)
            {
                UserId = id;
                UserName = name;
            }
        }

        #endregion

        #region Nested type: UserRemovedEventArgs

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// The UserRemovedEventArgs class provides a customised EventArgs class for
		/// the UserRemoved Event
		/// </summary>
		/// -----------------------------------------------------------------------------

        public class UserRemovedEventArgs : BaseUserEventArgs
        {
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Constructs a new UserRemovedEventArgs
			/// </summary>
			/// <param name="id">The Id of the User</param>
			/// <param name="name">The user name of the User</param>
			/// -----------------------------------------------------------------------------
			public UserRemovedEventArgs(int id, string name)
            {
                UserId = id;
                UserName = name;
            }
        }


        #endregion

        #region Nested type: UserUpdateErrorArgs

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UserUpdateErrorArgs class provides a customised EventArgs class for
        /// the UserUpdateError Event
        /// </summary>
        /// -----------------------------------------------------------------------------
        public class UserUpdateErrorArgs : BaseUserEventArgs
        {
            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Constructs a new UserUpdateErrorArgs
            /// </summary>
            /// <param name="id">The Id of the User</param>
            /// <param name="name">The user name of the User</param>
            /// <param name="message">The error message</param>
            /// -----------------------------------------------------------------------------
            public UserUpdateErrorArgs(int id, string name, string message)
            {
                UserId = id;
                UserName = name;
                Message = message;
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets and sets the error message
            /// </summary>
            /// -----------------------------------------------------------------------------
            public string Message { get; set; }
        }

        #endregion
    }
}
