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

namespace log4net.Core
{
	/// <summary>
	/// A strongly-typed collection of <see cref="Level"/> objects.
	/// </summary>
	/// <author>Nicko Cadell</author>
	public class LevelCollection : ICollection, IList, IEnumerable, ICloneable
	{
		#region Interfaces

		/// <summary>
		/// Supports type-safe iteration over a <see cref="LevelCollection"/>.
		/// </summary>
		public interface ILevelCollectionEnumerator
		{
			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			Level Current { get; }

			/// <summary>
			/// Advances the enumerator to the next element in the collection.
			/// </summary>
			/// <returns>
			/// <c>true</c> if the enumerator was successfully advanced to the next element; 
			/// <c>false</c> if the enumerator has passed the end of the collection.
			/// </returns>
			/// <exception cref="InvalidOperationException">
			/// The collection was modified after the enumerator was created.
			/// </exception>
			bool MoveNext();

			/// <summary>
			/// Sets the enumerator to its initial position, before the first element in the collection.
			/// </summary>
			void Reset();
		}

		#endregion

		private const int DEFAULT_CAPACITY = 16;

		#region Implementation (data)

		private Level[] m_array;
		private int m_count = 0;
		private int m_version = 0;

		#endregion
	
		#region Static Wrappers

		/// <summary>
		/// Creates a read-only wrapper for a <c>LevelCollection</c> instance.
		/// </summary>
		/// <param name="list">list to create a readonly wrapper arround</param>
		/// <returns>
		/// A <c>LevelCollection</c> wrapper that is read-only.
		/// </returns>
		public static LevelCollection ReadOnly(LevelCollection list)
		{
			if(list==null) throw new ArgumentNullException("list");

			return new ReadOnlyLevelCollection(list);
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <c>LevelCollection</c> class
		/// that is empty and has the default initial capacity.
		/// </summary>
		public LevelCollection()
		{
			m_array = new Level[DEFAULT_CAPACITY];
		}
		
		/// <summary>
		/// Initializes a new instance of the <c>LevelCollection</c> class
		/// that has the specified initial capacity.
		/// </summary>
		/// <param name="capacity">
		/// The number of elements that the new <c>LevelCollection</c> is initially capable of storing.
		/// </param>
		public LevelCollection(int capacity)
		{
			m_array = new Level[capacity];
		}

		/// <summary>
		/// Initializes a new instance of the <c>LevelCollection</c> class
		/// that contains elements copied from the specified <c>LevelCollection</c>.
		/// </summary>
		/// <param name="c">The <c>LevelCollection</c> whose elements are copied to the new collection.</param>
		public LevelCollection(LevelCollection c)
		{
			m_array = new Level[c.Count];
			AddRange(c);
		}

		/// <summary>
		/// Initializes a new instance of the <c>LevelCollection</c> class
		/// that contains elements copied from the specified <see cref="Level"/> array.
		/// </summary>
		/// <param name="a">The <see cref="Level"/> array whose elements are copied to the new list.</param>
		public LevelCollection(Level[] a)
		{
			m_array = new Level[a.Length];
			AddRange(a);
		}

		/// <summary>
		/// Initializes a new instance of the <c>LevelCollection</c> class
		/// that contains elements copied from the specified <see cref="Level"/> collection.
		/// </summary>
		/// <param name="col">The <see cref="Level"/> collection whose elements are copied to the new list.</param>
		public LevelCollection(ICollection col)
		{
			m_array = new Level[col.Count];
			AddRange(col);
		}
		
		/// <summary>
		/// Type visible only to our subclasses
		/// Used to access protected constructor
		/// </summary>
		protected internal enum Tag 
		{
			/// <summary>
			/// A value
			/// </summary>
			Default
		}

		/// <summary>
		/// Allow subclasses to avoid our default constructors
		/// </summary>
		/// <param name="tag"></param>
		protected internal LevelCollection(Tag tag)
		{
			m_array = null;
		}
		#endregion
		
		#region Operations (type-safe ICollection)

		/// <summary>
		/// Gets the number of elements actually contained in the <c>LevelCollection</c>.
		/// </summary>
		public virtual int Count
		{
			get { return m_count; }
		}

		/// <summary>
		/// Copies the entire <c>LevelCollection</c> to a one-dimensional
		/// <see cref="Level"/> array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Level"/> array to copy to.</param>
		public virtual void CopyTo(Level[] array)
		{
			this.CopyTo(array, 0);
		}

		/// <summary>
		/// Copies the entire <c>LevelCollection</c> to a one-dimensional
		/// <see cref="Level"/> array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="Level"/> array to copy to.</param>
		/// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		public virtual void CopyTo(Level[] array, int start)
		{
			if (m_count > array.GetUpperBound(0) + 1 - start)
			{
				throw new System.ArgumentException("Destination array was not long enough.");
			}
			
			Array.Copy(m_array, 0, array, start, m_count); 
		}

		/// <summary>
		/// Gets a value indicating whether access to the collection is synchronized (thread-safe).
		/// </summary>
		/// <value>true if access to the ICollection is synchronized (thread-safe); otherwise, false.</value>
		public virtual bool IsSynchronized
		{
			get { return m_array.IsSynchronized; }
		}

		/// <summary>
		/// Gets an object that can be used to synchronize access to the collection.
		/// </summary>
		public virtual object SyncRoot
		{
			get { return m_array.SyncRoot; }
		}

		#endregion
		
		#region Operations (type-safe IList)

		/// <summary>
		/// Gets or sets the <see cref="Level"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="LevelCollection.Count"/>.</para>
		/// </exception>
		public virtual Level this[int index]
		{
			get
			{
				ValidateIndex(index); // throws
				return m_array[index]; 
			}
			set
			{
				ValidateIndex(index); // throws
				++m_version; 
				m_array[index] = value; 
			}
		}

		/// <summary>
		/// Adds a <see cref="Level"/> to the end of the <c>LevelCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Level"/> to be added to the end of the <c>LevelCollection</c>.</param>
		/// <returns>The index at which the value has been added.</returns>
		public virtual int Add(Level item)
		{
			if (m_count == m_array.Length)
			{
				EnsureCapacity(m_count + 1);
			}

			m_array[m_count] = item;
			m_version++;

			return m_count++;
		}
		
		/// <summary>
		/// Removes all elements from the <c>LevelCollection</c>.
		/// </summary>
		public virtual void Clear()
		{
			++m_version;
			m_array = new Level[DEFAULT_CAPACITY];
			m_count = 0;
		}
		
		/// <summary>
		/// Creates a shallow copy of the <see cref="LevelCollection"/>.
		/// </summary>
		/// <returns>A new <see cref="LevelCollection"/> with a shallow copy of the collection data.</returns>
		public virtual object Clone()
		{
			LevelCollection newCol = new LevelCollection(m_count);
			Array.Copy(m_array, 0, newCol.m_array, 0, m_count);
			newCol.m_count = m_count;
			newCol.m_version = m_version;

			return newCol;
		}

		/// <summary>
		/// Determines whether a given <see cref="Level"/> is in the <c>LevelCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Level"/> to check for.</param>
		/// <returns><c>true</c> if <paramref name="item"/> is found in the <c>LevelCollection</c>; otherwise, <c>false</c>.</returns>
		public virtual bool Contains(Level item)
		{
			for (int i=0; i != m_count; ++i)
			{
				if (m_array[i].Equals(item))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns the zero-based index of the first occurrence of a <see cref="Level"/>
		/// in the <c>LevelCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Level"/> to locate in the <c>LevelCollection</c>.</param>
		/// <returns>
		/// The zero-based index of the first occurrence of <paramref name="item"/> 
		/// in the entire <c>LevelCollection</c>, if found; otherwise, -1.
		///	</returns>
		public virtual int IndexOf(Level item)
		{
			for (int i=0; i != m_count; ++i)
			{
				if (m_array[i].Equals(item))
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Inserts an element into the <c>LevelCollection</c> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The <see cref="Level"/> to insert.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="LevelCollection.Count"/>.</para>
		/// </exception>
		public virtual void Insert(int index, Level item)
		{
			ValidateIndex(index, true); // throws
			
			if (m_count == m_array.Length)
			{
				EnsureCapacity(m_count + 1);
			}

			if (index < m_count)
			{
				Array.Copy(m_array, index, m_array, index + 1, m_count - index);
			}

			m_array[index] = item;
			m_count++;
			m_version++;
		}

		/// <summary>
		/// Removes the first occurrence of a specific <see cref="Level"/> from the <c>LevelCollection</c>.
		/// </summary>
		/// <param name="item">The <see cref="Level"/> to remove from the <c>LevelCollection</c>.</param>
		/// <exception cref="ArgumentException">
		/// The specified <see cref="Level"/> was not found in the <c>LevelCollection</c>.
		/// </exception>
		public virtual void Remove(Level item)
		{		   
			int i = IndexOf(item);
			if (i < 0)
			{
				throw new System.ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			}
			
			++m_version;
			RemoveAt(i);
		}

		/// <summary>
		/// Removes the element at the specified index of the <c>LevelCollection</c>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="index"/> is less than zero</para>
		/// <para>-or-</para>
		/// <para><paramref name="index"/> is equal to or greater than <see cref="LevelCollection.Count"/>.</para>
		/// </exception>
		public virtual void RemoveAt(int index)
		{
			ValidateIndex(index); // throws
			
			m_count--;

			if (index < m_count)
			{
				Array.Copy(m_array, index + 1, m_array, index, m_count - index);
			}
			
			// We can't set the deleted entry equal to null, because it might be a value type.
			// Instead, we'll create an empty single-element array of the right type and copy it 
			// over the entry we want to erase.
			Level[] temp = new Level[1];
			Array.Copy(temp, 0, m_array, m_count, 1);
			m_version++;
		}

		/// <summary>
		/// Gets a value indicating whether the collection has a fixed size.
		/// </summary>
		/// <value>true if the collection has a fixed size; otherwise, false. The default is false</value>
		public virtual bool IsFixedSize
		{
			get { return false; }
		}

		/// <summary>
		/// Gets a value indicating whether the IList is read-only.
		/// </summary>
		/// <value>true if the collection is read-only; otherwise, false. The default is false</value>
		public virtual bool IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region Operations (type-safe IEnumerable)
		
		/// <summary>
		/// Returns an enumerator that can iterate through the <c>LevelCollection</c>.
		/// </summary>
		/// <returns>An <see cref="Enumerator"/> for the entire <c>LevelCollection</c>.</returns>
		public virtual ILevelCollectionEnumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		#endregion

		#region Public helpers (just to mimic some nice features of ArrayList)
		
		/// <summary>
		/// Gets or sets the number of elements the <c>LevelCollection</c> can contain.
		/// </summary>
		public virtual int Capacity
		{
			get 
			{ 
				return m_array.Length; 
			}
			set
			{
				if (value < m_count)
				{
					value = m_count;
				}

				if (value != m_array.Length)
				{
					if (value > 0)
					{
						Level[] temp = new Level[value];
						Array.Copy(m_array, 0, temp, 0, m_count);
						m_array = temp;
					}
					else
					{
						m_array = new Level[DEFAULT_CAPACITY];
					}
				}
			}
		}

		/// <summary>
		/// Adds the elements of another <c>LevelCollection</c> to the current <c>LevelCollection</c>.
		/// </summary>
		/// <param name="x">The <c>LevelCollection</c> whose elements should be added to the end of the current <c>LevelCollection</c>.</param>
		/// <returns>The new <see cref="LevelCollection.Count"/> of the <c>LevelCollection</c>.</returns>
		public virtual int AddRange(LevelCollection x)
		{
			if (m_count + x.Count >= m_array.Length)
			{
				EnsureCapacity(m_count + x.Count);
			}
			
			Array.Copy(x.m_array, 0, m_array, m_count, x.Count);
			m_count += x.Count;
			m_version++;

			return m_count;
		}

		/// <summary>
		/// Adds the elements of a <see cref="Level"/> array to the current <c>LevelCollection</c>.
		/// </summary>
		/// <param name="x">The <see cref="Level"/> array whose elements should be added to the end of the <c>LevelCollection</c>.</param>
		/// <returns>The new <see cref="LevelCollection.Count"/> of the <c>LevelCollection</c>.</returns>
		public virtual int AddRange(Level[] x)
		{
			if (m_count + x.Length >= m_array.Length)
			{
				EnsureCapacity(m_count + x.Length);
			}

			Array.Copy(x, 0, m_array, m_count, x.Length);
			m_count += x.Length;
			m_version++;

			return m_count;
		}

		/// <summary>
		/// Adds the elements of a <see cref="Level"/> collection to the current <c>LevelCollection</c>.
		/// </summary>
		/// <param name="col">The <see cref="Level"/> collection whose elements should be added to the end of the <c>LevelCollection</c>.</param>
		/// <returns>The new <see cref="LevelCollection.Count"/> of the <c>LevelCollection</c>.</returns>
		public virtual int AddRange(ICollection col)
		{
			if (m_count + col.Count >= m_array.Length)
			{
				EnsureCapacity(m_count + col.Count);
			}

			foreach(object item in col)
			{
				Add((Level)item);
			}

			return m_count;
		}
		
		/// <summary>
		/// Sets the capacity to the actual number of elements.
		/// </summary>
		public virtual void TrimToSize()
		{
			this.Capacity = m_count;
		}

		#endregion

		#region Implementation (helpers)

		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="i"/> is less than zero</para>
		/// <para>-or-</para>
		/// <para><paramref name="i"/> is equal to or greater than <see cref="LevelCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i)
		{
			ValidateIndex(i, false);
		}

		/// <exception cref="ArgumentOutOfRangeException">
		/// <para><paramref name="i"/> is less than zero</para>
		/// <para>-or-</para>
		/// <para><paramref name="i"/> is equal to or greater than <see cref="LevelCollection.Count"/>.</para>
		/// </exception>
		private void ValidateIndex(int i, bool allowEqualEnd)
		{
			int max = (allowEqualEnd) ? (m_count) : (m_count-1);
			if (i < 0 || i > max)
			{
				throw log4net.Util.SystemInfo.CreateArgumentOutOfRangeException("i", (object)i, "Index was out of range. Must be non-negative and less than the size of the collection. [" + (object)i + "] Specified argument was out of the range of valid values.");
			}
		}

		private void EnsureCapacity(int min)
		{
			int newCapacity = ((m_array.Length == 0) ? DEFAULT_CAPACITY : m_array.Length * 2);
			if (newCapacity < min)
			{
				newCapacity = min;
			}

			this.Capacity = newCapacity;
		}

		#endregion
		
		#region Implementation (ICollection)

		void ICollection.CopyTo(Array array, int start)
		{
			Array.Copy(m_array, 0, array, start, m_count);
		}

		#endregion

		#region Implementation (IList)

		object IList.this[int i]
		{
			get { return (object)this[i]; }
			set { this[i] = (Level)value; }
		}

		int IList.Add(object x)
		{
			return this.Add((Level)x);
		}

		bool IList.Contains(object x)
		{
			return this.Contains((Level)x);
		}

		int IList.IndexOf(object x)
		{
			return this.IndexOf((Level)x);
		}

		void IList.Insert(int pos, object x)
		{
			this.Insert(pos, (Level)x);
		}

		void IList.Remove(object x)
		{
			this.Remove((Level)x);
		}

		void IList.RemoveAt(int pos)
		{
			this.RemoveAt(pos);
		}

		#endregion

		#region Implementation (IEnumerable)

		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)(this.GetEnumerator());
		}

		#endregion

		#region Nested enumerator class

		/// <summary>
		/// Supports simple iteration over a <see cref="LevelCollection"/>.
		/// </summary>
		private sealed class Enumerator : IEnumerator, ILevelCollectionEnumerator
		{
			#region Implementation (data)
			
			private readonly LevelCollection m_collection;
			private int m_index;
			private int m_version;
			
			#endregion
		
			#region Construction
			
			/// <summary>
			/// Initializes a new instance of the <c>Enumerator</c> class.
			/// </summary>
			/// <param name="tc"></param>
			internal Enumerator(LevelCollection tc)
			{
				m_collection = tc;
				m_index = -1;
				m_version = tc.m_version;
			}
			
			#endregion
	
			#region Operations (type-safe IEnumerator)
			
			/// <summary>
			/// Gets the current element in the collection.
			/// </summary>
			public Level Current
			{
				get { return m_collection[m_index]; }
			}

			/// <summary>
			/// Advances the enumerator to the next element in the collection.
			/// </summary>
			/// <returns>
			/// <c>true</c> if the enumerator was successfully advanced to the next element; 
			/// <c>false</c> if the enumerator has passed the end of the collection.
			/// </returns>
			/// <exception cref="InvalidOperationException">
			/// The collection was modified after the enumerator was created.
			/// </exception>
			public bool MoveNext()
			{
				if (m_version != m_collection.m_version)
				{
					throw new System.InvalidOperationException("Collection was modified; enumeration operation may not execute.");
				}

				++m_index;
				return (m_index < m_collection.Count);
			}

			/// <summary>
			/// Sets the enumerator to its initial position, before the first element in the collection.
			/// </summary>
			public void Reset()
			{
				m_index = -1;
			}

			#endregion
	
			#region Implementation (IEnumerator)
			
			object IEnumerator.Current
			{
				get { return this.Current; }
			}
			
			#endregion
		}

		#endregion

		#region Nested Read Only Wrapper class

		private sealed class ReadOnlyLevelCollection : LevelCollection
		{
			#region Implementation (data)

			private readonly LevelCollection m_collection;

			#endregion

			#region Construction

			internal ReadOnlyLevelCollection(LevelCollection list) : base(Tag.Default)
			{
				m_collection = list;
			}

			#endregion

			#region Type-safe ICollection

			public override void CopyTo(Level[] array)
			{
				m_collection.CopyTo(array);
			}

			public override void CopyTo(Level[] array, int start)
			{
				m_collection.CopyTo(array,start);
			}
			public override int Count
			{
				get { return m_collection.Count; }
			}

			public override bool IsSynchronized
			{
				get { return m_collection.IsSynchronized; }
			}

			public override object SyncRoot
			{
				get { return this.m_collection.SyncRoot; }
			}

			#endregion

			#region Type-safe IList

			public override Level this[int i]
			{
				get { return m_collection[i]; }
				set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
			}

			public override int Add(Level x)
			{
				throw new NotSupportedException("This is a Read Only Collection and can not be modified");
			}

			public override void Clear()
			{
				throw new NotSupportedException("This is a Read Only Collection and can not be modified");
			}

			public override bool Contains(Level x)
			{
				return m_collection.Contains(x);
			}

			public override int IndexOf(Level x)
			{
				return m_collection.IndexOf(x);
			}

			public override void Insert(int pos, Level x)
			{
				throw new NotSupportedException("This is a Read Only Collection and can not be modified");
			}

			public override void Remove(Level x)
			{           
				throw new NotSupportedException("This is a Read Only Collection and can not be modified");
			}

			public override void RemoveAt(int pos)
			{
				throw new NotSupportedException("This is a Read Only Collection and can not be modified");
			}

			public override bool IsFixedSize
			{
				get { return true; }
			}

			public override bool IsReadOnly
			{
				get { return true; }
			}

			#endregion

			#region Type-safe IEnumerable

			public override ILevelCollectionEnumerator GetEnumerator()
			{
				return m_collection.GetEnumerator();
			}

			#endregion

			#region Public Helpers

			// (just to mimic some nice features of ArrayList)
			public override int Capacity
			{
				get { return m_collection.Capacity; }
				set { throw new NotSupportedException("This is a Read Only Collection and can not be modified"); }
			}

			public override int AddRange(LevelCollection x)
			{
				throw new NotSupportedException("This is a Read Only Collection and can not be modified");
			}

			public override int AddRange(Level[] x)
			{
				throw new NotSupportedException("This is a Read Only Collection and can not be modified");
			}

			#endregion
		}

		#endregion
	}

}
