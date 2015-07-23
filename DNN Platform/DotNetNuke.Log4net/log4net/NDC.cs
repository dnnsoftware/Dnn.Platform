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
using System.Collections;

#if NETCF_1_0
using Stack = log4net.Util.ThreadContextStack.Stack;
#endif

namespace log4net
{
	/// <summary>
	/// Implementation of Nested Diagnostic Contexts.
	/// </summary>
	/// <remarks>
	/// <note>
	/// <para>
	/// The NDC is deprecated and has been replaced by the <see cref="ThreadContext.Stacks"/>.
	/// The current NDC implementation forwards to the <c>ThreadContext.Stacks["NDC"]</c>.
	/// </para>
	/// </note>
	/// <para>
	/// A Nested Diagnostic Context, or NDC in short, is an instrument
	/// to distinguish interleaved log output from different sources. Log
	/// output is typically interleaved when a server handles multiple
	/// clients near-simultaneously.
	/// </para>
	/// <para>
	/// Interleaved log output can still be meaningful if each log entry
	/// from different contexts had a distinctive stamp. This is where NDCs
	/// come into play.
	/// </para>
	/// <para>
	/// Note that NDCs are managed on a per thread basis. The NDC class
	/// is made up of static methods that operate on the context of the
	/// calling thread.
	/// </para>
	/// </remarks>
	/// <example>How to push a message into the context
	/// <code lang="C#">
	///	using(NDC.Push("my context message"))
	///	{
	///		... all log calls will have 'my context message' included ...
	///	
	///	} // at the end of the using block the message is automatically removed 
	/// </code>
	/// </example>
	/// <threadsafety static="true" instance="true" />
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	/*[Obsolete("NDC has been replaced by ThreadContext.Stacks")]*/
	public sealed class NDC
	{
		#region Private Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="NDC" /> class. 
		/// </summary>
		/// <remarks>
		/// Uses a private access modifier to prevent instantiation of this class.
		/// </remarks>
		private NDC()
		{
		}

		#endregion Private Instance Constructors

		#region Public Static Properties

		/// <summary>
		/// Gets the current context depth.
		/// </summary>
		/// <value>The current context depth.</value>
		/// <remarks>
		/// <note>
		/// <para>
		/// The NDC is deprecated and has been replaced by the <see cref="ThreadContext.Stacks"/>.
		/// The current NDC implementation forwards to the <c>ThreadContext.Stacks["NDC"]</c>.
		/// </para>
		/// </note>
		/// <para>
		/// The number of context values pushed onto the context stack.
		/// </para>
		/// <para>
		/// Used to record the current depth of the context. This can then 
		/// be restored using the <see cref="SetMaxDepth"/> method.
		/// </para>
		/// </remarks>
		/// <seealso cref="SetMaxDepth"/>
		/*[Obsolete("NDC has been replaced by ThreadContext.Stacks")]*/
		public static int Depth
		{
			get { return ThreadContext.Stacks["NDC"].Count; }
		}

		#endregion Public Static Properties

		#region Public Static Methods

		/// <summary>
		/// Clears all the contextual information held on the current thread.
		/// </summary>
		/// <remarks>
		/// <note>
		/// <para>
		/// The NDC is deprecated and has been replaced by the <see cref="ThreadContext.Stacks"/>.
		/// The current NDC implementation forwards to the <c>ThreadContext.Stacks["NDC"]</c>.
		/// </para>
		/// </note>
		/// <para>
		/// Clears the stack of NDC data held on the current thread.
		/// </para>
		/// </remarks>
		/*[Obsolete("NDC has been replaced by ThreadContext.Stacks")]*/
		public static void Clear() 
		{
			ThreadContext.Stacks["NDC"].Clear();
		}

		/// <summary>
		/// Creates a clone of the stack of context information.
		/// </summary>
		/// <returns>A clone of the context info for this thread.</returns>
		/// <remarks>
		/// <note>
		/// <para>
		/// The NDC is deprecated and has been replaced by the <see cref="ThreadContext.Stacks"/>.
		/// The current NDC implementation forwards to the <c>ThreadContext.Stacks["NDC"]</c>.
		/// </para>
		/// </note>
		/// <para>
		/// The results of this method can be passed to the <see cref="Inherit"/> 
		/// method to allow child threads to inherit the context of their 
		/// parent thread.
		/// </para>
		/// </remarks>
		/*[Obsolete("NDC has been replaced by ThreadContext.Stacks")]*/
		public static Stack CloneStack() 
		{
			return ThreadContext.Stacks["NDC"].InternalStack;
		}

		/// <summary>
		/// Inherits the contextual information from another thread.
		/// </summary>
		/// <param name="stack">The context stack to inherit.</param>
		/// <remarks>
		/// <note>
		/// <para>
		/// The NDC is deprecated and has been replaced by the <see cref="ThreadContext.Stacks"/>.
		/// The current NDC implementation forwards to the <c>ThreadContext.Stacks["NDC"]</c>.
		/// </para>
		/// </note>
		/// <para>
		/// This thread will use the context information from the stack
		/// supplied. This can be used to initialize child threads with
		/// the same contextual information as their parent threads. These
		/// contexts will <b>NOT</b> be shared. Any further contexts that
		/// are pushed onto the stack will not be visible to the other.
		/// Call <see cref="CloneStack"/> to obtain a stack to pass to
		/// this method.
		/// </para>
		/// </remarks>
		/*[Obsolete("NDC has been replaced by ThreadContext.Stacks", true)]*/
		public static void Inherit(Stack stack) 
		{
			ThreadContext.Stacks["NDC"].InternalStack = stack;
		}

		/// <summary>
		/// Removes the top context from the stack.
		/// </summary>
		/// <returns>
		/// The message in the context that was removed from the top 
		/// of the stack.
		/// </returns>
		/// <remarks>
		/// <note>
		/// <para>
		/// The NDC is deprecated and has been replaced by the <see cref="ThreadContext.Stacks"/>.
		/// The current NDC implementation forwards to the <c>ThreadContext.Stacks["NDC"]</c>.
		/// </para>
		/// </note>
		/// <para>
		/// Remove the top context from the stack, and return
		/// it to the caller. If the stack is empty then an
		/// empty string (not <c>null</c>) is returned.
		/// </para>
		/// </remarks>
		/*[Obsolete("NDC has been replaced by ThreadContext.Stacks")]*/
		public static string Pop() 
		{
			return ThreadContext.Stacks["NDC"].Pop();
		}

		/// <summary>
		/// Pushes a new context message.
		/// </summary>
		/// <param name="message">The new context message.</param>
		/// <returns>
		/// An <see cref="IDisposable"/> that can be used to clean up 
		/// the context stack.
		/// </returns>
		/// <remarks>
		/// <note>
		/// <para>
		/// The NDC is deprecated and has been replaced by the <see cref="ThreadContext.Stacks"/>.
		/// The current NDC implementation forwards to the <c>ThreadContext.Stacks["NDC"]</c>.
		/// </para>
		/// </note>
		/// <para>
		/// Pushes a new context onto the context stack. An <see cref="IDisposable"/>
		/// is returned that can be used to clean up the context stack. This
		/// can be easily combined with the <c>using</c> keyword to scope the
		/// context.
		/// </para>
		/// </remarks>
		/// <example>Simple example of using the <c>Push</c> method with the <c>using</c> keyword.
		/// <code lang="C#">
		/// using(log4net.NDC.Push("NDC_Message"))
		/// {
		///		log.Warn("This should have an NDC message");
		///	}
		/// </code>
		/// </example>
		/*[Obsolete("NDC has been replaced by ThreadContext.Stacks")]*/
		public static IDisposable Push(string message) 
		{
			return ThreadContext.Stacks["NDC"].Push(message);
		}

		/// <summary>
		/// Removes the context information for this thread. It is
		/// not required to call this method.
		/// </summary>
		/// <remarks>
		/// <note>
		/// <para>
		/// The NDC is deprecated and has been replaced by the <see cref="ThreadContext.Stacks"/>.
		/// The current NDC implementation forwards to the <c>ThreadContext.Stacks["NDC"]</c>.
		/// </para>
		/// </note>
		/// <para>
		/// This method is not implemented.
		/// </para>
		/// </remarks>
		/*[Obsolete("NDC has been replaced by ThreadContext.Stacks")]*/
		public static void Remove() 
		{
		}

		/// <summary>
		/// Forces the stack depth to be at most <paramref name="maxDepth"/>.
		/// </summary>
		/// <param name="maxDepth">The maximum depth of the stack</param>
		/// <remarks>
		/// <note>
		/// <para>
		/// The NDC is deprecated and has been replaced by the <see cref="ThreadContext.Stacks"/>.
		/// The current NDC implementation forwards to the <c>ThreadContext.Stacks["NDC"]</c>.
		/// </para>
		/// </note>
		/// <para>
		/// Forces the stack depth to be at most <paramref name="maxDepth"/>.
		/// This may truncate the head of the stack. This only affects the 
		/// stack in the current thread. Also it does not prevent it from
		/// growing, it only sets the maximum depth at the time of the
		/// call. This can be used to return to a known context depth.
		/// </para>
		/// </remarks>
		/*[Obsolete("NDC has been replaced by ThreadContext.Stacks")]*/
		public static void SetMaxDepth(int maxDepth) 
		{
			if (maxDepth >= 0)
			{
				log4net.Util.ThreadContextStack stack = ThreadContext.Stacks["NDC"];

				if (maxDepth == 0)
				{
					stack.Clear();
				}
				else
				{
					while(stack.Count > maxDepth)
					{
						stack.Pop();
					}
				}
			}
		}

		#endregion Public Static Methods
	}
}
