﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

//
// Licensed to the Apache Software Foundation (ASF) under one or more
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership.
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#if !NETCF
using System;
using System.Collections;

using log4net.Core;

namespace log4net.Util
{
    /// <summary>
    /// Delegate type used for LogicalThreadContextStack's callbacks.
    /// </summary>
#if NET_2_0 || MONO_2_0
    public delegate void TwoArgAction<T1, T2>(T1 t1, T2 t2);
#else
	public delegate void TwoArgAction(string t1, LogicalThreadContextStack t2);
#endif

    /// <summary>
    /// Implementation of Stack for the <see cref="log4net.LogicalThreadContext"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementation of Stack for the <see cref="log4net.LogicalThreadContext"/>.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    public sealed class LogicalThreadContextStack : IFixingRequired
    {
        /// <summary>
        /// The stack store.
        /// </summary>
        private Stack m_stack = new Stack();

        /// <summary>
        /// The name of this <see cref="log4net.Util.LogicalThreadContextStack"/> within the
        /// <see cref="log4net.Util.LogicalThreadContextProperties"/>.
        /// </summary>
        private string m_propertyKey;

        /// <summary>
        /// The callback used to let the <see cref="log4net.Util.LogicalThreadContextStacks"/> register a
        /// new instance of a <see cref="log4net.Util.LogicalThreadContextStack"/>.
        /// </summary>
#if NET_2_0 || MONO_2_0
        private TwoArgAction<string, LogicalThreadContextStack> m_registerNew;
#else
		private TwoArgAction m_registerNew;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalThreadContextStack"/> class.
        /// Internal constructor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="LogicalThreadContextStack" /> class.
        /// </para>
        /// </remarks>
#if NET_2_0 || MONO_2_0
        internal LogicalThreadContextStack(string propertyKey, TwoArgAction<string, LogicalThreadContextStack> registerNew)
#else
		internal LogicalThreadContextStack(string propertyKey, TwoArgAction registerNew)
#endif
        {
            this.m_propertyKey = propertyKey;
            this.m_registerNew = registerNew;
        }

        /// <summary>
        /// Gets the number of messages in the stack.
        /// </summary>
        /// <value>
        /// The current number of messages in the stack.
        /// </value>
        /// <remarks>
        /// <para>
        /// The current number of messages in the stack. That is
        /// the number of times <see cref="Push"/> has been called
        /// minus the number of times <see cref="Pop"/> has been called.
        /// </para>
        /// </remarks>
        public int Count
        {
            get { return this.m_stack.Count; }
        }

        /// <summary>
        /// Clears all the contextual information held in this stack.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Clears all the contextual information held in this stack.
        /// Only call this if you think that this thread is being reused after
        /// a previous call execution which may not have completed correctly.
        /// You do not need to use this method if you always guarantee to call
        /// the <see cref="IDisposable.Dispose"/> method of the <see cref="IDisposable"/>
        /// returned from <see cref="Push"/> even in exceptional circumstances,
        /// for example by using the <c>using(log4net.LogicalThreadContext.Stacks["NDC"].Push("Stack_Message"))</c>
        /// syntax.
        /// </para>
        /// </remarks>
        public void Clear()
        {
            this.m_registerNew(this.m_propertyKey, new LogicalThreadContextStack(this.m_propertyKey, this.m_registerNew));
        }

        /// <summary>
        /// Removes the top context from this stack.
        /// </summary>
        /// <returns>The message in the context that was removed from the top of this stack.</returns>
        /// <remarks>
        /// <para>
        /// Remove the top context from this stack, and return
        /// it to the caller. If this stack is empty then an
        /// empty string (not <see langword="null"/>) is returned.
        /// </para>
        /// </remarks>
        public string Pop()
        {
            // copy current stack
            Stack stack = new Stack(new Stack(this.m_stack));
            string result = string.Empty;
            if (stack.Count > 0)
            {
                result = ((StackFrame)stack.Pop()).Message;
            }

            LogicalThreadContextStack ltcs = new LogicalThreadContextStack(this.m_propertyKey, this.m_registerNew);
            ltcs.m_stack = stack;
            this.m_registerNew(this.m_propertyKey, ltcs);
            return result;
        }

        /// <summary>
        /// Pushes a new context message into this stack.
        /// </summary>
        /// <param name="message">The new context message.</param>
        /// <returns>
        /// An <see cref="IDisposable"/> that can be used to clean up the context stack.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Pushes a new context onto this stack. An <see cref="IDisposable"/>
        /// is returned that can be used to clean up this stack. This
        /// can be easily combined with the <c>using</c> keyword to scope the
        /// context.
        /// </para>
        /// </remarks>
        /// <example>Simple example of using the <c>Push</c> method with the <c>using</c> keyword.
        /// <code lang="C#">
        /// using(log4net.LogicalThreadContext.Stacks["NDC"].Push("Stack_Message"))
        /// {
        ///         log.Warn("This should have an ThreadContext Stack message");
        ///     }
        /// </code>
        /// </example>
        public IDisposable Push(string message)
        {
            // do modifications on a copy
            Stack stack = new Stack(new Stack(this.m_stack));
            stack.Push(new StackFrame(message, (stack.Count > 0) ? (StackFrame)stack.Peek() : null));

            LogicalThreadContextStack contextStack = new LogicalThreadContextStack(this.m_propertyKey, this.m_registerNew);
            contextStack.m_stack = stack;
            this.m_registerNew(this.m_propertyKey, contextStack);
            return new AutoPopStackFrame(contextStack, stack.Count - 1);
        }

        /// <summary>
        /// Gets the current context information for this stack.
        /// </summary>
        /// <returns>The current context information.</returns>
        internal string GetFullMessage()
        {
            Stack stack = this.m_stack;
            if (stack.Count > 0)
            {
                return ((StackFrame)stack.Peek()).FullMessage;
            }

            return null;
        }

        /// <summary>
        /// Gets or sets and sets the internal stack used by this <see cref="LogicalThreadContextStack"/>.
        /// </summary>
        /// <value>The internal storage stack.</value>
        /// <remarks>
        /// <para>
        /// This property is provided only to support backward compatability
        /// of the <see cref="NDC"/>. Tytpically the internal stack should not
        /// be modified.
        /// </para>
        /// </remarks>
        internal Stack InternalStack
        {
            get { return this.m_stack; }
            set { this.m_stack = value; }
        }

        /// <summary>
        /// Gets the current context information for this stack.
        /// </summary>
        /// <returns>Gets the current context information.</returns>
        /// <remarks>
        /// <para>
        /// Gets the current context information for this stack.
        /// </para>
        /// </remarks>
        public override string ToString()
        {
            return this.GetFullMessage();
        }

        /// <summary>
        /// Get a portable version of this object.
        /// </summary>
        /// <returns>the portable instance of this object.</returns>
        /// <remarks>
        /// <para>
        /// Get a cross thread portable version of this object.
        /// </para>
        /// </remarks>
        object IFixingRequired.GetFixedObject()
        {
            return this.GetFullMessage();
        }

        /// <summary>
        /// Inner class used to represent a single context frame in the stack.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Inner class used to represent a single context frame in the stack.
        /// </para>
        /// </remarks>
        private sealed class StackFrame
        {
            private readonly string m_message;
            private readonly StackFrame m_parent;
            private string m_fullMessage = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="StackFrame"/> class.
            /// Constructor.
            /// </summary>
            /// <param name="message">The message for this context.</param>
            /// <param name="parent">The parent context in the chain.</param>
            /// <remarks>
            /// <para>
            /// Initializes a new instance of the <see cref="StackFrame" /> class
            /// with the specified message and parent context.
            /// </para>
            /// </remarks>
            internal StackFrame(string message, StackFrame parent)
            {
                this.m_message = message;
                this.m_parent = parent;

                if (parent == null)
                {
                    this.m_fullMessage = message;
                }
            }

            /// <summary>
            /// Gets get the message.
            /// </summary>
            /// <value>The message.</value>
            /// <remarks>
            /// <para>
            /// Get the message.
            /// </para>
            /// </remarks>
            internal string Message
            {
                get { return this.m_message; }
            }

            /// <summary>
            /// Gets the full text of the context down to the root level.
            /// </summary>
            /// <value>
            /// The full text of the context down to the root level.
            /// </value>
            /// <remarks>
            /// <para>
            /// Gets the full text of the context down to the root level.
            /// </para>
            /// </remarks>
            internal string FullMessage
            {
                get
                {
                    if (this.m_fullMessage == null && this.m_parent != null)
                    {
                        this.m_fullMessage = string.Concat(this.m_parent.FullMessage, " ", this.m_message);
                    }

                    return this.m_fullMessage;
                }
            }
        }

        /// <summary>
        /// Struct returned from the <see cref="LogicalThreadContextStack.Push"/> method.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This struct implements the <see cref="IDisposable"/> and is designed to be used
        /// with the <see langword="using"/> pattern to remove the stack frame at the end of the scope.
        /// </para>
        /// </remarks>
        private struct AutoPopStackFrame : IDisposable
        {
            /// <summary>
            /// The depth to trim the stack to when this instance is disposed.
            /// </summary>
            private int m_frameDepth;

            /// <summary>
            /// The outer LogicalThreadContextStack.
            /// </summary>
            private LogicalThreadContextStack m_logicalThreadContextStack;

            /// <summary>
            /// Initializes a new instance of the <see cref="AutoPopStackFrame"/> struct.
            /// Constructor.
            /// </summary>
            /// <param name="logicalThreadContextStack">The internal stack used by the ThreadContextStack.</param>
            /// <param name="frameDepth">The depth to return the stack to when this object is disposed.</param>
            /// <remarks>
            /// <para>
            /// Initializes a new instance of the <see cref="AutoPopStackFrame" /> class with
            /// the specified stack and return depth.
            /// </para>
            /// </remarks>
            internal AutoPopStackFrame(LogicalThreadContextStack logicalThreadContextStack, int frameDepth)
            {
                this.m_frameDepth = frameDepth;
                this.m_logicalThreadContextStack = logicalThreadContextStack;
            }

            /// <summary>
            /// Returns the stack to the correct depth.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Returns the stack to the correct depth.
            /// </para>
            /// </remarks>
            public void Dispose()
            {
                if (this.m_frameDepth >= 0 && this.m_logicalThreadContextStack.m_stack != null)
                {
                    Stack stack = new Stack(new Stack(this.m_logicalThreadContextStack.m_stack));
                    while (stack.Count > this.m_frameDepth)
                    {
                        stack.Pop();
                    }

                    LogicalThreadContextStack ltcs = new LogicalThreadContextStack(this.m_logicalThreadContextStack.m_propertyKey, this.m_logicalThreadContextStack.m_registerNew);
                    ltcs.m_stack = stack;
                    this.m_logicalThreadContextStack.m_registerNew(
                        this.m_logicalThreadContextStack.m_propertyKey,
                        ltcs);
                }
            }
        }
    }
}
#endif
