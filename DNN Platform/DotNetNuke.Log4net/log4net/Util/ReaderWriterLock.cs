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

#if (!NETCF)
#define HAS_READERWRITERLOCK
#endif

using System;

namespace log4net.Util
{
	/// <summary>
	/// Defines a lock that supports single writers and multiple readers
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>ReaderWriterLock</c> is used to synchronize access to a resource. 
	/// At any given time, it allows either concurrent read access for 
	/// multiple threads, or write access for a single thread. In a 
	/// situation where a resource is changed infrequently, a 
	/// <c>ReaderWriterLock</c> provides better throughput than a simple 
	/// one-at-a-time lock, such as <see cref="System.Threading.Monitor"/>.
	/// </para>
	/// <para>
	/// If a platform does not support a <c>System.Threading.ReaderWriterLock</c> 
	/// implementation then all readers and writers are serialized. Therefore 
	/// the caller must not rely on multiple simultaneous readers.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public sealed class ReaderWriterLock
	{
		#region Instance Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="ReaderWriterLock" /> class.
		/// </para>
		/// </remarks>
		public ReaderWriterLock()
		{
#if HAS_READERWRITERLOCK
			m_lock = new System.Threading.ReaderWriterLock();
#endif
		}

		#endregion Private Instance Constructors
  
		#region Public Methods

		/// <summary>
		/// Acquires a reader lock
		/// </summary>
		/// <remarks>
		/// <para>
		/// <see cref="AcquireReaderLock"/> blocks if a different thread has the writer 
		/// lock, or if at least one thread is waiting for the writer lock.
		/// </para>
		/// </remarks>
		public void AcquireReaderLock()
		{
#if HAS_READERWRITERLOCK
			m_lock.AcquireReaderLock(-1);
#else
			System.Threading.Monitor.Enter(this);
#endif
		}

		/// <summary>
		/// Decrements the lock count
		/// </summary>
		/// <remarks>
		/// <para>
		/// <see cref="ReleaseReaderLock"/> decrements the lock count. When the count 
		/// reaches zero, the lock is released.
		/// </para>
		/// </remarks>
		public void ReleaseReaderLock()
		{
#if HAS_READERWRITERLOCK
			m_lock.ReleaseReaderLock();
#else
			System.Threading.Monitor.Exit(this);
#endif
		}

		/// <summary>
		/// Acquires the writer lock
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method blocks if another thread has a reader lock or writer lock.
		/// </para>
		/// </remarks>
		public void AcquireWriterLock()
		{
#if HAS_READERWRITERLOCK
			m_lock.AcquireWriterLock(-1);
#else
			System.Threading.Monitor.Enter(this);
#endif
		}

		/// <summary>
		/// Decrements the lock count on the writer lock
		/// </summary>
		/// <remarks>
		/// <para>
		/// ReleaseWriterLock decrements the writer lock count. 
		/// When the count reaches zero, the writer lock is released.
		/// </para>
		/// </remarks>
		public void ReleaseWriterLock()
		{
#if HAS_READERWRITERLOCK
			m_lock.ReleaseWriterLock();
#else
			System.Threading.Monitor.Exit(this);
#endif
		}

		#endregion Public Methods

		#region Private Members

#if HAS_READERWRITERLOCK
		private System.Threading.ReaderWriterLock m_lock;
#endif

		#endregion
	}
}
