using log4net.Core;
using System;
using System.Collections;

namespace log4net.Util
{
	public sealed class ThreadContextStack : IFixingRequired
	{
		private Stack m_stack = new Stack();

		public int Count
		{
			get
			{
				return this.m_stack.Count;
			}
		}

		internal Stack InternalStack
		{
			get
			{
				return this.m_stack;
			}
			set
			{
				this.m_stack = value;
			}
		}

		internal ThreadContextStack()
		{
		}

		public void Clear()
		{
			this.m_stack.Clear();
		}

		internal string GetFullMessage()
		{
			Stack mStack = this.m_stack;
			if (mStack.Count <= 0)
			{
				return null;
			}
			return ((ThreadContextStack.StackFrame)mStack.Peek()).FullMessage;
		}

		object log4net.Core.IFixingRequired.GetFixedObject()
		{
			return this.GetFullMessage();
		}

		public string Pop()
		{
			Stack mStack = this.m_stack;
			if (mStack.Count <= 0)
			{
				return "";
			}
			return ((ThreadContextStack.StackFrame)mStack.Pop()).Message;
		}

		public IDisposable Push(string message)
		{
			ThreadContextStack.StackFrame stackFrame;
			Stack mStack = this.m_stack;
			Stack stacks = mStack;
			string str = message;
			if (mStack.Count > 0)
			{
				stackFrame = (ThreadContextStack.StackFrame)mStack.Peek();
			}
			else
			{
				stackFrame = null;
			}
			stacks.Push(new ThreadContextStack.StackFrame(str, stackFrame));
			return new ThreadContextStack.AutoPopStackFrame(mStack, mStack.Count - 1);
		}

		public override string ToString()
		{
			return this.GetFullMessage();
		}

		private struct AutoPopStackFrame : IDisposable
		{
			private Stack m_frameStack;

			private int m_frameDepth;

			internal AutoPopStackFrame(Stack frameStack, int frameDepth)
			{
				this.m_frameStack = frameStack;
				this.m_frameDepth = frameDepth;
			}

			public void Dispose()
			{
				if (this.m_frameDepth >= 0 && this.m_frameStack != null)
				{
					while (this.m_frameStack.Count > this.m_frameDepth)
					{
						this.m_frameStack.Pop();
					}
				}
			}
		}

		private sealed class StackFrame
		{
			private readonly string m_message;

			private readonly ThreadContextStack.StackFrame m_parent;

			private string m_fullMessage;

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

			internal string Message
			{
				get
				{
					return this.m_message;
				}
			}

			internal StackFrame(string message, ThreadContextStack.StackFrame parent)
			{
				this.m_message = message;
				this.m_parent = parent;
				if (parent == null)
				{
					this.m_fullMessage = message;
				}
			}
		}
	}
}