using UnityEngine.Assertions;
using System.Collections.Generic;


/// <summary>
/// A wrapper around a Dictionary and a List. MappedList effectively stores both objects and their index in the List.
/// Mapping the indices of objects allows for arbitary reordering of objects within the List, offering fast removal of
/// objects, whilst still providing the benefits of using a standalone List. In exchange for memory, MappedList
/// provides fast iteration, fast removal, fast insertion and predictable search access times. MappedList is primarily
/// designed to be used with reference types.
/// </summary>
public sealed class MappedList<T>
{
    #region Members

    /// <summary>
    /// Maps the object reference to the List index.
    /// </summary>
    Dictionary<T, int> indices = new Dictionary<T, int> (100);

    /// <summary>
    /// A List containing all mapped objects.
    /// </summary>
    List<T> objects = new List<T> (100);

    #endregion

    #region Storage functionality

    /// <summary>
    /// Adds the given item. Will assert if the item is already stored.
    /// </summary>
    public void Add (T toAdd)
    {
        // Ensure we haven't already added the object.
        Assert.IsFalse (indices.ContainsKey (toAdd), 
            typeof (MappedList<T>).FullName + ": Attempt at adding the same object twice.");

        indices.Add (toAdd, objects.Count);
        objects.Add (toAdd);
    }

    void Remove (int index)
    {
        //var backIndex = indices.Count - 1;
    }

    #endregion
}
