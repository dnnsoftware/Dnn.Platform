#region Apache License
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
#endregion

using System;

#if !NETCF_1_0
using System.Collections;
#endif

using log4net.Core;

namespace log4net.Util
{
	/// <summary>
	/// Implementation of Stack for the <see cref="log4net.ThreadContext"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Implementation of Stack for the <see cref="log4net.ThreadContext"/>
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public sealed class ThreadContextStack : IFixingRequired
	{
		#region Private Static Fields

		/// <summary>
		/// The stack store.
		/// </summary>
		private Stack m_stack = new Stack();

		#endregion Private Static Fields

		#region Public Instance Constructors

		/// <summary>
		/// Internal constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="ThreadContextStack" /> class. 
		/// </para>
		/// </remarks>
		internal ThreadContextStack()
		{
		}

		#endregion Public Instance Constructors

		#region Public Properties

		/// <summary>
		/// The number of messages in the stack
		/// </summary>
		/// <value>
		/// The current number of messages in the stack
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
			get { return m_stack.Count; }
		}

		#endregion // Public Properties

		#region Public Methods

		/// <summary>
		/// Clears all the contextual information held in this stack.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Clears all the contextual information held in this stack.
		/// Only call this if you think that this tread is being reused after
		/// a previous call execution which may not have completed correctly.
		/// You do not need to use this method if you always guarantee to call
		/// the <see cref="IDisposable.Dispose"/> method of the <see cref="IDisposable"/>
		/// returned from <see cref="Push"/> even in exceptional circumstances,
		/// for example by using the <c>using(log4net.ThreadContext.Stacks["NDC"].Push("Stack_Message"))</c> 
		/// syntax.
		/// </para>
		/// </remarks>
		public void Clear() 
		{
			m_stack.Clear();
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
			Stack stack = m_stack;
			if (stack.Count > 0)
			{
				return ((StackFrame)(stack.Pop())).Message;
			}
			return "";
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
		/// using(log4net.ThreadContext.Stacks["NDC"].Push("Stack_Message"))
		/// {
		///		log.Warn("This should have an ThreadContext Stack message");
		///	}
		/// </code>
		/// </example>
		public IDisposable Push(string message) 
		{
			Stack stack = m_stack;
			stack.Push(new StackFrame(message, (stack.Count>0) ? (StackFrame)stack.Peek() : null));

			return new AutoPopStackFrame(stack, stack.Count - 1);
		}

		#endregion Public Methods

		#region Internal Methods

		/// <summary>
		/// Gets the current context information for this stack.
		/// </summary>
		/// <returns>The current context information.</returns>
		internal string GetFullMessage() 
		{
			Stack stack = m_stack;
			if (stack.Count > 0)
			{
				return ((StackFrame)(stack.Peek())).FullMessage;
			}
			return null;
		}

		/// <summary>
		/// Gets and sets the internal stack used by this <see cref="ThreadContextStack"/>
		/// </summary>
		/// <value>The internal storage stack</value>
		/// <remarks>
		/// <para>
		/// This property is provided only to support backward compatability 
		/// of the <see cref="NDC"/>. Tytpically the internal stack should not
		/// be modified.
		/// </para>
		/// </remarks>
		internal Stack InternalStack
		{
			get { return m_stack; }
			set { m_stack = value; }
		}
  
		#endregion Internal Methods

		/// <summary>
		/// Gets the current context information for this stack.
		/// </summary>
		/// <returns>Gets the current context information</returns>
		/// <remarks>
		/// <para>
		/// Gets the current context information for this stack.
		/// </para>
		/// </remarks>
		public override string ToString()
		{
			return GetFullMessage();
		}

		/// <summary>
		/// Get a portable version of this object
		/// </summary>
		/// <returns>the portable instance of this object</returns>
		/// <remarks>
		/// <para>
		/// Get a cross thread portable version of this object
		/// </para>
		/// </remarks>
		object IFixingRequired.GetFixedObject()
		{
			return GetFullMessage();
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
			#region Private Instance Fields

			private readonly string m_message;
			private readonly StackFrame m_parent;
			private string m_fullMessage = null;
    
			#endregion

			#region Internal Instance Constructors

			/// <summary>
			/// Constructor
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
				m_message = message;
				m_parent = parent;

				if (parent == null) 
				{
					m_fullMessage = message;
				} 
			}

			#endregion Internal Instance Constructors

			#region Internal Instance Properties

			/// <summary>
			/// Get the message.
			/// </summary>
			/// <value>The message.</value>
			/// <remarks>
			/// <para>
			/// Get the message.
			/// </para>
			/// </remarks>
			internal string Message
			{
				get { return m_message; }
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
					if (m_fullMessage == null && m_parent != null)
					{
						m_fullMessage = string.Concat(m_parent.FullMessage, " ", m_message);
					}
					return m_fullMessage; 
				}
			}

			#endregion Internal Instance Properties
		}

		/// <summary>
		/// Struct returned from the <see cref="ThreadContextStack.Push"/> method.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This struct implements the <see cref="IDisposable"/> and is designed to be used
		/// with the <see langword="using"/> pattern to remove the stack frame at the end of the scope.
		/// </para>
		/// </remarks>
		private struct AutoPopStackFrame : IDisposable
		{
			#region Private Instance Fields

			/// <summary>
			/// The ThreadContextStack internal stack
			/// </summary>
			private Stack m_frameStack;

			/// <summary>
			/// The depth to trim the stack to when this instance is disposed
			/// </summary>
			private int m_frameDepth;

			#endregion Private Instance Fields

			#region Internal Instance Constructors

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="frameStack">The internal stack used by the ThreadContextStack.</param>
			/// <param name="frameDepth">The depth to return the stack to when this object is disposed.</param>
			/// <remarks>
			/// <para>
			/// Initializes a new instance of the <see cref="AutoPopStackFrame" /> class with
			/// the specified stack and return depth.
			/// </para>
			/// </remarks>
			internal AutoPopStackFrame(Stack frameStack, int frameDepth)
			{
				m_frameStack = frameStack;
				m_frameDepth = frameDepth;
			}

			#endregion Internal Instance Constructors

			#region Implementation of IDisposable

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
				if (m_frameDepth >= 0 && m_frameStack != null)
				{
					while(m_frameStack.Count > m_frameDepth)
					{
						m_frameStack.Pop();
					}
				}
			}

			#endregion Implementation of IDisposable
		}

#if NETCF_1_0
		/// <summary>
		/// Subclass of <see cref="System.Collections.Stack"/> to
		/// provide missing methods.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The Compact Framework version of the <see cref="System.Collections.Stack"/>
		/// class is missing the <c>Clear</c> and <c>Clone</c> methods.
		/// This subclass adds implementations of those missing methods.
		/// </para>
		/// </remarks>
		public class Stack : System.Collections.Stack
		{
			/// <summary>
			/// Clears the stack of all elements.
			/// </summary>
			/// <remarks>
			/// <para>
			/// Clears the stack of all elements.
			/// </para>
			/// </remarks>
			public void Clear()
			{
				while(Count > 0)
				{
					Pop();
				}
			}

			/// <summary>
			/// Makes a shallow copy of the stack's elements.
			/// </summary>
			/// <returns>A new stack that has a shallow copy of the stack's elements.</returns>
			/// <remarks>
			/// <para>
			/// Makes a shallow copy of the stack's elements.
			/// </para>
			/// </remarks>
			public Stack Clone()
			{
				Stack res = new Stack();
				object[] items = ToArray();
				foreach(object item in items)
				{
					res.Push(item);
				}
				return res;
			}
		}
#endif
	}
}
