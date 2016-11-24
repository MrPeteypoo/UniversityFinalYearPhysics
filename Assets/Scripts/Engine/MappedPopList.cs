using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// A PopList which maps the stored objects to their index value. In exchange for higher memory usage, mapping object
/// indices means objects can be moved around freely by the PopList without sacrificing external access to objects.
/// Utilising a MappedPopList allows for fast iteration, fast external access and fast external adding/removal.
/// </summary>
public class MappedPopList<T> : PopList<T>
{
    #region Members

    /// <summary>
    /// Maps stored objects to their PopList index.
    /// </summary>
    Dictionary<T, int> m_indices = null;

    #endregion

    #region Construction

    /// <summary>
    /// Constructs a MappedPopList with a capacity of zero.
    /// </summary>
    public MappedPopList() : base() 
    {
        m_indices = new Dictionary<T, int>();
    }

    /// <summary>
    /// Constructs a MappedPopList with the given data. Duplicates will be removed and objects may be reorded.
    /// </summary>
    /// <param name="data">The data to initialise with.</param>
    /// <param name="capacityIncrement">Determines how much the capacity is expanded when reallocation occurs.</param>
    public MappedPopList (IEnumerable<T> data, int capacityIncrement = 32) : base (0, capacityIncrement)
    {
        // Retrieve the actual data.
        var fillWith = data.ToArray();

        // Allocate enough memory for filling the list and dictionary.
        Capacity = fillWith.Length;
        m_indices = new Dictionary<T, int> (Capacity);

        // Add each item.
        foreach (var item in fillWith)
        {
            Add (item);
        }
    }

    /// <summary>
    /// Constructs a MappedPopList and reserves the desired capacity.
    /// </summary>
    /// <param name="capacity">Allocates enough memory for this number of objects.</param>
    /// <param name="capacityIncrement">Determines how much the capacity is expanded when reallocation occurs.</param>
    public MappedPopList (int capacity, int capacityIncrement = 32) : base (capacity, capacityIncrement)
    {
        m_indices = new Dictionary<T, int> (capacity);
    }

    #endregion

    #region Overrides

    /// <summary>
    /// Adds the given item. This will be ignored if it already exists to avoid duplicates. This is an O(1) operation
    /// unless the capacity is exceeded, in which case a reallocation will occur.
    /// </summary>
    /// <param name="item">The item to be added.</param>
    public override void Add (T item)
    {
        if (Contains (item))
        {
            // The item will be added to the end.
            m_indices.Add (item, Count);
            base.Add (item);
        }
    }

    /// <summary>
    /// Checks if the given item has a mapped index. This is an O(1) operation.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    public override bool Contains (T item)
    {
        return m_indices.ContainsKey (item);
    }

    /// <summary>
    /// Clears the stored objects and mapped indices.
    /// </summary>
    public override void Clear()
    {
        base.Clear();
        m_indices.Clear();
    }

    /// <summary>
    /// Retrives the index of the given item in the MappedPopList. This is an O(1) operation.
    /// </summary>
    /// <returns>The index of the item, -1 if it doesn't exist.</returns>
    /// <param name="item">The item to look for.</param>
    public override int IndexOf (T item)
    {
        if (Contains (item))
        {
            return m_indices[item];
        }

        return -1;
    }

    /// <summary>
    /// Inserts the given item at the specified index. Order will be retained so this will be an incredibly slow
    /// operation, especially if memory reallocation is required. Attempts to insert a duplicate will result in an 
    /// exception.
    /// </summary>
    /// <param name="index">The desired index of the item.</param>
    /// <param name="item">The item to be inserted.</param>
    public override void Insert (int index, T item)
    {
        if (Contains (item))
        {
            throw new ArgumentException ("item", "Attempt to add a duplicate item.");
        }

        // Insert the object.
        base.Insert (index, item);
        SyncIndices (index);
    }

    /// <summary>
    /// Removes the specified item. Finding the item is an O(1) operation but the removal is O(n*2) where n is the number
    /// of items from the removed item to the total count, this is due to order being maintained.
    /// </summary>
    /// <param name="item">The item to be removed.</param>
    /// <returns> Whether the item existed and as such, was removed. </returns>
    public override bool Remove (T item)
    {
        var index = IndexOf (item);

        if (index == -1)
        {
            return false;
        }

        // Remove the object.
        base.RemoveAt (index);
        m_indices.Remove (item);
        SyncIndices (index);

        return true;
    }

    /// <summary>
    /// Removes the item at the given index. This is an O(n*2) operation where n == Count - index.
    /// </summary>
    /// <param name="index">The index of the item to be removed.</param>
    public override void RemoveAt (int index)
    {
        // Remove the item from the indices first.
        var item = this[index];
        m_indices.Remove (item);
        base.RemoveAt (index);
        SyncIndices (index);
    }

    /// <summary>
    /// Removes both the item at the given index and all references to it.
    /// </summary>
    /// <param name="index">The index of the item to be removed..</param>
    protected override void InternalSwapAndPop (int index)
    {
        // Remove references to the object being replaced.
        m_indices.Remove (this[index]);

        // Remove the chosen item.
        base.InternalSwapAndPop (index);

        // Update the reference to the swapped object if we didn't remove the last object.
        if (Count > 0)
        {
            m_indices[this[index]] = index;
        }
    }

    #endregion

    #region Extra functionality

    /// <summary>
    /// Swaps the given item with the last stored item and pops it from the back.
    /// </summary>
    /// <param name="item">The item to be removed.</param>
    public void SwapAndPop (T item)
    {
        // Check if the item exists.
        var index = IndexOf (item);

        if (index != -1)
        {
            InternalSwapAndPop (index);
        }
    }

    #endregion

    #region Internal workings

    private void SyncIndices (int start)
    {
        for (var i = start; i < Count; ++i)
        {
            m_indices[this[i]] = i;
        }
    }

    #endregion
}
