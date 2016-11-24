using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// A wrapper around a List which features swap'n'pop functionality and increases the List capacity by a fixed amount
/// when the maximum capacity is reached. The inclusion of swap'n'pop features mean that, as long as object order isn't
/// important, objects can be removed with O(1) complexity.
/// </summary>
public partial class PopList<T> : IList<T>
{
    #region Internal data

    /// <summary>
    /// The internal managed List.
    /// </summary>
    List<T> m_data = null;

    /// <summary>
    /// The logical "end" of the list, this is where dead objects exist.
    /// </summary>
    int m_end = 0;

    /// <summary>
    /// How much the capacity should be incremented by once we've reached our limit.
    /// </summary>
    readonly int m_capacityIncrement = 32;

    #endregion

    #region Constructors

    /// <summary>
    /// Constructs a PopList with a capacity of zero.
    /// </summary>
    public PopList()
    {
        m_data = new List<T>();
        m_end = m_data.Count;
    }

    /// <summary>
    /// Constructs a PopList from the given data.
    /// </summary>
    /// <param name="data">The desired data to be contained by the PopList.</param>
    /// <param name="capacityIncrement">How much the capacity should increment when an allocation is required.</param>
    public PopList (IEnumerable<T> data, int capacityIncrement = 32)
    {
        if (capacityIncrement < 0)
        {
            throw new ArgumentOutOfRangeException ("capacityIncrement", "Can't be less than zero.");
        }

        m_data = new List<T> (data);
        m_end = m_data.Count;
        m_capacityIncrement = capacityIncrement;
    }

    /// <summary>
    /// Constructs a PopList with the given capacity.
    /// </summary>
    /// <param name="capacity">How many objects should be reserved by the PopList.</param>
    /// <param name="capacityIncrement">How much the capacity should increment when an allocation is required.</param>
    public PopList (Int32 capacity, int capacityIncrement = 32)
    {
        if (capacityIncrement < 0)
        {
            throw new ArgumentOutOfRangeException ("capacityIncrement", "Can't be less than zero.");
        }

        m_data = new List<T> (capacity);
        m_end = m_data.Count;
        m_capacityIncrement = capacityIncrement;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the capacity of the PopList.
    /// </summary>
    /// <value>The desired capacity.</value>
    public int Capacity
    {
        get { return m_data.Capacity; }
        set
        {
            // Replicate List behaviour.
            if (value < m_end || value >= m_data.Count)
            {
                m_data.Capacity = value;
            }

            // Trim the count to allow for the capacity to be reduced correctly.
            else
            {
                m_data.RemoveRange (value, m_data.Count - value);
                m_data.Capacity = value;
            }
        }
    }

    /// <summary>
    /// Gets the number of elements contained in the PopList.
    /// </summary>
    public int Count { get { return m_end; } }

    /// <summary>
    /// Gets a value indicating whether this instance is read only.
    /// </summary>
    /// <value><c>true</c> if this instance is read only; otherwise, <c>false</c>.</value>
    bool ICollection<T>.IsReadOnly { get { return false; } } 

    /// <summary>
    /// Gets or sets the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    public T this[int index]
    {
        get 
        { 
            if (index < 0 || index >= m_end)
            {
                throw new ArgumentOutOfRangeException ("index");
            }

            return m_data[index]; 
        }
        set 
        { 
            if (index < 0 || index >= m_end)
            {
                throw new ArgumentOutOfRangeException ("index");
            }

            m_data[index] = value; 
        }
    }

    #endregion

    #region IList<T> methods

    /// <summary>
    /// Add the specified item to the end of the PopList.
    /// </summary>
    /// <param name="item">The item to be added.</param>
    public virtual void Add (T item)
    {
        // Replace one of the stale objects.
        if (m_end < m_data.Count)
        {
            m_data[m_end++] = item;
        }

        else
        {
            // Increment the capacity by the desired amount.
            if (m_data.Count == m_data.Capacity)
            {
                m_data.Capacity += m_capacityIncrement;
            }

            m_data.Add (item);
            m_end = m_data.Count;
        }
    }

    /// <summary>
    /// Clears the stored data, discarding any references to objects.
    /// </summary>
    public virtual void Clear()
    {
        m_data.Clear();
        m_end = m_data.Count;
    }

    /// <summary>
    /// Iterates through the list looking for the specified item. Uses EqualityComparer<T>.Default.
    /// </summary>
    /// <param name="item">The item to look for.</param>
    public virtual bool Contains (T item)
    {
        // Use the types default comparison.
        var comparer = EqualityComparer<T>.Default;
        foreach (var storedItem in m_data)
        {
            if (comparer.Equals (storedItem, item))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Copies every item from the PopList to the given array at the specified index.
    /// </summary>
    /// <param name="array">The array to be filled with data.</param>
    /// <param name="arrayIndex">The index inside array to start copying to.</param>
    public void CopyTo (T[] array, int arrayIndex)
    {
        if (array == null)
        {
            throw new ArgumentNullException ("array");
        }

        if (arrayIndex < 0)
        {
            throw new ArgumentOutOfRangeException ("arrayIndex");
        }

        if (array.Length - arrayIndex < m_end)
        {
            throw new ArgumentException ("arrayIndex");
        }

        for (var i = 0; i < m_end; ++i)
        {
            array[arrayIndex++] = m_data[i];
        }
    }

    /// <summary>
    /// Gets an IEnumerator<T> to the data stored within.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator (this);
    }

    /// <summary>
    /// Gets an IEnumerator to the data stored within.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator (this);
    }

    /// <summary>
    /// Determines the index of the given item in the PopList. Uses EqualityComparer<T>.Default.
    /// </summary>
    /// <returns>The index of the item, -1 if it doesn't exist.</returns>
    /// <param name="item">The item to search for.</param>
    public virtual int IndexOf (T item)
    {
        var comparer = EqualityComparer<T>.Default;
        for (var i = 0; i < m_end; ++i)
        {
            if (comparer.Equals (m_data[i], item))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Inserts the given item at the specified index.
    /// </summary>
    /// <param name="index">The index to insert the item at.</param>
    /// <param name="item">The item to be inserted.</param>
    public virtual void Insert (int index, T item)
    {
        if (index < 0 || index >= m_end)
        {
            throw new ArgumentOutOfRangeException ("index");
        }

        m_data.Insert (index, item);
        ++m_end;
    }

    /// <summary>
    /// Removes the first occurrence of the given object from the PopList. Uses EqualityComparer<T>.Default.
    /// </summary>
    /// <param name="item">The item to be removed.</param>
    public virtual bool Remove (T item)
    {
        var comparer = EqualityComparer<T>.Default;
        for (var i = 0; i < m_end; ++i)
        {
            if (comparer.Equals (m_data[i], item))
            {
                m_data.RemoveAt (i);
                --m_end;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes the item at the given index. Removing the last item will just call PopBack().
    /// </summary>
    /// <param name="index">The index of the item to be removed.</param>
    public virtual void RemoveAt (int index)
    {
        // Use the pop functionality for speed.
        if (index == m_end - 1)
        {
            PopBack();
        }

        else if (index < 0 || index >= m_end)
        {
            throw new ArgumentOutOfRangeException ("index");
        }

        else
        {
            m_data.RemoveAt (index);
            --m_end;
        }
    }

    #endregion

    #region Pop functionality

    /// <summary>
    /// Removes the last item without reallocating data..
    /// </summary>
    public void PopBack()
    {
        if (m_end == 0)
        {
            throw new InvalidOperationException ("Attempting to remove last item when no items exists.");
        }

        InternalPopBack();
    }

    /// <summary>
    /// Swaps the item at the given index with the last item and pops it from the back.
    /// </summary>
    /// <param name="index">The item to be removed.</param>
    public void SwapAndPop (int index)
    {
        if (index < 0 || index >= m_end)
        {
            throw new ArgumentOutOfRangeException ("index", "Must be a valid index.");
        }

        InternalSwapAndPop (index);
    }

    /// <summary>
    /// Sets the last item to default (T) and reduces m_end.
    /// </summary>
    protected virtual void InternalPopBack()
    {
        m_data[--m_end] = default (T);
    }

    /// <summary>
    /// Swaps the item at the given index with the last item and pops it from the back.
    /// </summary>
    /// <param name="index">The item to be removed.<</param>
    protected virtual void InternalSwapAndPop (int index)
    {
        m_data[index] = m_data[m_end - 1];
        PopBack();
    }

    #endregion
}

public partial class PopList<T> : IList<T>
{
    /// <summary>
    /// An IEnumerator for the PopList class, allows for use in foreach loops.
    /// </summary>
    public sealed class Enumerator : IEnumerator<T>
    {
        #region Internal data

        /// <summary>
        /// The list being iterated through.
        /// </summary>
        PopList<T> m_list = null;

        /// <summary>
        /// The index of the current object.
        /// </summary>
        int m_currentIndex = -1;

        /// <summary>
        /// The initial PopList count when the Enumerator was created.
        /// </summary>
        int m_startingCount = -1;

        /// <summary>
        /// The current item being accessed.
        /// </summary>
        T m_currentItem = default (T);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PopList`1+Enumerator"/> class.
        /// </summary>
        /// <param name="list">List.</param>
        public Enumerator (PopList<T> list)
        {
            m_list = list;
            m_startingCount = m_list.Count;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current item as an object.
        /// </summary>
        public T Current
        {
            get { return m_currentItem; }
        }

        /// <summary>
        /// Gets the current item as an object.
        /// </summary>
        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion

        #region IEnumerator methods

        /// <summary>
        /// Does nothing.
        /// </summary>
        void IDisposable.Dispose() { }

        /// <summary>
        /// Moves the next.
        /// </summary>
        /// <returns>Whether we've reached the end of the PopList.</returns>
        public bool MoveNext()
        {
            if (m_list.Count != m_startingCount)
            {
                throw new InvalidOperationException ("The collection was modified after the enumerator was created.");
            }

            if (++m_currentIndex < m_list.Count)
            {
                m_currentItem = m_list.m_data[m_currentIndex];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reset the Enumerator to its initial state, ready for iterating the collection again.
        /// </summary>
        public void Reset()
        {
            if (m_list.Count != m_startingCount)
            {
                throw new InvalidOperationException ("The collection was modified after the enumerator was created.");
            }

            m_currentIndex = -1;
            m_currentItem = default (T);
        }

        #endregion
    }
}