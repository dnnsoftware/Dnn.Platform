// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Membership;

    /// -----------------------------------------------------------------------------
    /// Project  :  DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Modules
    /// Class    :  UserUserControlBase
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserUserControlBase class defines a custom base class for the User Control.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class UserUserControlBase : UserModuleBase
    {
        public delegate void UserCreatedEventHandler(object sender, UserCreatedEventArgs e);

        public delegate void UserDeletedEventHandler(object sender, UserDeletedEventArgs e);

        public delegate void UserRestoredEventHandler(object sender, UserRestoredEventArgs e);

        public delegate void UserRemovedEventHandler(object sender, UserRemovedEventArgs e);

        public delegate void UserUpdateErrorEventHandler(object sender, UserUpdateErrorArgs e);

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

        protected override bool AddUser => !this.Request.IsAuthenticated || base.AddUser;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the UserCreateCompleted Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnUserCreateCompleted(UserCreatedEventArgs e)
        {
            if (this.UserCreateCompleted != null)
            {
                this.UserCreateCompleted(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the UserCreated Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnUserCreated(UserCreatedEventArgs e)
        {
            if (this.UserCreated != null)
            {
                this.UserCreated(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the UserDeleted Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnUserDeleted(UserDeletedEventArgs e)
        {
            if (this.UserDeleted != null)
            {
                this.UserDeleted(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the UserDeleteError Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnUserDeleteError(UserUpdateErrorArgs e)
        {
            if (this.UserDeleteError != null)
            {
                this.UserDeleteError(this, e);
            }
        }

        public void OnUserRestored(UserRestoredEventArgs e)
        {
            if (this.UserRestored != null)
            {
                this.UserRestored(this, e);
            }
        }

        public void OnUserRestoreError(UserUpdateErrorArgs e)
        {
            if (this.UserRestoreError != null)
            {
                this.UserRestoreError(this, e);
            }
        }

        public void OnUserRemoved(UserRemovedEventArgs e)
        {
            if (this.UserRemoved != null)
            {
                this.UserRemoved(this, e);
            }
        }

        public void OnUserRemoveError(UserUpdateErrorArgs e)
        {
            if (this.UserRemoveError != null)
            {
                this.UserRemoveError(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the UserUpdated Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnUserUpdated(EventArgs e)
        {
            if (this.UserUpdated != null)
            {
                this.UserUpdated(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the UserUpdated Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnUserUpdateCompleted(EventArgs e)
        {
            if (this.UserUpdateCompleted != null)
            {
                this.UserUpdateCompleted(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Raises the UserUpdateError Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void OnUserUpdateError(UserUpdateErrorArgs e)
        {
            if (this.UserUpdateError != null)
            {
                this.UserUpdateError(this, e);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The BaseUserEventArgs class provides a base for User EventArgs classes.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public class BaseUserEventArgs
        {
            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets or sets and sets the Id of the User.
            /// </summary>
            /// -----------------------------------------------------------------------------
            public int UserId { get; set; }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets or sets and sets the Id of the User.
            /// </summary>
            /// -----------------------------------------------------------------------------
            public string UserName { get; set; }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UserCreatedEventArgs class provides a customised EventArgs class for
        /// the UserCreated Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public class UserCreatedEventArgs
        {
            private UserCreateStatus _createStatus = UserCreateStatus.Success;

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Initializes a new instance of the <see cref="UserCreatedEventArgs"/> class.
            /// Constructs a new UserCreatedEventArgs.
            /// </summary>
            /// <param name="newUser">The newly Created User.</param>
            /// -----------------------------------------------------------------------------
            public UserCreatedEventArgs(UserInfo newUser)
            {
                this.NewUser = newUser;
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets or sets and sets the Create Status.
            /// </summary>
            /// -----------------------------------------------------------------------------
            public UserCreateStatus CreateStatus
            {
                get
                {
                    return this._createStatus;
                }

                set
                {
                    this._createStatus = value;
                }
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets or sets and sets the New User.
            /// </summary>
            /// -----------------------------------------------------------------------------
            public UserInfo NewUser { get; set; }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets or sets a value indicating whether gets and sets a flag whether to Notify the new User of the Creation.
            /// </summary>
            /// -----------------------------------------------------------------------------
            public bool Notify { get; set; }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UserDeletedEventArgs class provides a customised EventArgs class for
        /// the UserDeleted Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public class UserDeletedEventArgs : BaseUserEventArgs
        {
            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Initializes a new instance of the <see cref="UserDeletedEventArgs"/> class.
            /// Constructs a new UserDeletedEventArgs.
            /// </summary>
            /// <param name="id">The Id of the User.</param>
            /// <param name="name">The user name of the User.</param>
            /// -----------------------------------------------------------------------------
            public UserDeletedEventArgs(int id, string name)
            {
                this.UserId = id;
                this.UserName = name;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UserRestoredEventArgs class provides a customised EventArgs class for
        /// the UserRestored Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public class UserRestoredEventArgs : BaseUserEventArgs
        {
            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Initializes a new instance of the <see cref="UserRestoredEventArgs"/> class.
            /// Constructs a new UserRestoredEventArgs.
            /// </summary>
            /// <param name="id">The Id of the User.</param>
            /// <param name="name">The user name of the User.</param>
            /// -----------------------------------------------------------------------------
            public UserRestoredEventArgs(int id, string name)
            {
                this.UserId = id;
                this.UserName = name;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UserRemovedEventArgs class provides a customised EventArgs class for
        /// the UserRemoved Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public class UserRemovedEventArgs : BaseUserEventArgs
        {
            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Initializes a new instance of the <see cref="UserRemovedEventArgs"/> class.
            /// Constructs a new UserRemovedEventArgs.
            /// </summary>
            /// <param name="id">The Id of the User.</param>
            /// <param name="name">The user name of the User.</param>
            /// -----------------------------------------------------------------------------
            public UserRemovedEventArgs(int id, string name)
            {
                this.UserId = id;
                this.UserName = name;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UserUpdateErrorArgs class provides a customised EventArgs class for
        /// the UserUpdateError Event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public class UserUpdateErrorArgs : BaseUserEventArgs
        {
            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Initializes a new instance of the <see cref="UserUpdateErrorArgs"/> class.
            /// Constructs a new UserUpdateErrorArgs.
            /// </summary>
            /// <param name="id">The Id of the User.</param>
            /// <param name="name">The user name of the User.</param>
            /// <param name="message">The error message.</param>
            /// -----------------------------------------------------------------------------
            public UserUpdateErrorArgs(int id, string name, string message)
            {
                this.UserId = id;
                this.UserName = name;
                this.Message = message;
            }

            /// -----------------------------------------------------------------------------
            /// <summary>
            /// Gets or sets and sets the error message.
            /// </summary>
            /// -----------------------------------------------------------------------------
            public string Message { get; set; }
        }
    }
}
