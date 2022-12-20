using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctTree<T>
{
    public Bounds Bounds { get; private set; }
    public uint Capacity { get; private set; }
    public int Depth { get; private set; }
    public int Count => GetItemCount();
    public List<OctTree<T>> SubOctant;
    private List<(T item, Vector3 position)> mStoredItems;
    private int mMaxDepth;
    private OctTree<T> mParentQuad;

    public OctTree(Bounds bounds, uint capacity, int maxDepth, int depth = 0)
    {
        Bounds = bounds;
        Capacity = capacity;
        Depth = depth;
        mMaxDepth = maxDepth;
        SubOctant = new List<OctTree<T>>();
        mStoredItems = new List<(T item, Vector3 position)>();
    }
    public void Insert((T item, Vector3 position) itemData)
    {
        if (Bounds.Contains(itemData.position))
        {
            if (SubOctant.Count > 0 && Depth < mMaxDepth)
                InsertToSubOctant(itemData);
            else
            {
                mStoredItems.Add(itemData);
                if (mStoredItems.Count > Capacity)
                {
                    Subdivide();
                }
            }
        }
        else
        {
            throw new System.ArgumentOutOfRangeException("item.Position", "Attempting to insert item outside Octant bounds!!");
        }
    }
    public void Remove((T item, Vector3 position) itemData)
    {
        if (Bounds.Contains(itemData.position))
        {
            OctTree<T> smallestQuad = FindSmallestOctantAt(itemData.position);
            if (smallestQuad.mStoredItems.Contains(itemData))
            {
                smallestQuad.mStoredItems.Remove(itemData);
                if(smallestQuad.mParentQuad.Count <= smallestQuad.mParentQuad.Capacity)
                {
                    smallestQuad.mParentQuad.Reconstruct();
                }
            }
        }
        else
        {
            throw new System.ArgumentOutOfRangeException("item.Position", "Attempting to remove item outside Octant bounds!!");
        }
    }
    private void InsertToSubOctant((T item, Vector3 position) itemData)
    {
        foreach (var Octant in SubOctant)
        {
            if (Octant.Bounds.Contains(itemData.position))
            {
                Octant.Insert(itemData);
                break;
            }
        }
    }
    private void Subdivide()
    {
        Bounds topLeftForwardBounds = new Bounds(Bounds.center + (new Vector3(-Bounds.size.x , Bounds.size.y , Bounds.size.z) * 0.25f), Bounds.size * 0.5f);
        OctTree<T> OctantTopLeftForward = new OctTree<T>(topLeftForwardBounds, Capacity, mMaxDepth,  Depth+1);
        Bounds topRightForwardBounds = new Bounds(Bounds.center + (new Vector3(Bounds.size.x, Bounds.size.y, Bounds.size.z) * 0.25f), Bounds.size * 0.5f);
        OctTree<T> OctantTopRightForward = new OctTree<T>(topRightForwardBounds, Capacity, mMaxDepth, Depth + 1);
        Bounds bottomLeftForwardBounds = new Bounds(Bounds.center + (new Vector3(-Bounds.size.x, -Bounds.size.y, Bounds.size.z) * 0.25f), Bounds.size * 0.5f);
        OctTree<T> OctantBottomLeftForward = new OctTree<T>(bottomLeftForwardBounds, Capacity, mMaxDepth, Depth + 1);
        Bounds bottomRightForwardBounds = new Bounds(Bounds.center + (new Vector3(Bounds.size.x, -Bounds.size.y, Bounds.size.z) * 0.25f), Bounds.size * 0.5f);
        OctTree<T> OctantBottomRightForward = new OctTree<T>(bottomRightForwardBounds, Capacity, mMaxDepth, Depth + 1);
        Bounds topLeftBackwardBounds = new Bounds(Bounds.center + (new Vector3(-Bounds.size.x, Bounds.size.y, -Bounds.size.z) * 0.25f), Bounds.size * 0.5f);
        OctTree<T> OctantTopLeftBackward = new OctTree<T>(topLeftBackwardBounds, Capacity, mMaxDepth, Depth + 1);
        Bounds topRightBackwardBounds = new Bounds(Bounds.center + (new Vector3(Bounds.size.x, Bounds.size.y, -Bounds.size.z) * 0.25f), Bounds.size * 0.5f);
        OctTree<T> OctantTopRightBackward = new OctTree<T>(topRightBackwardBounds, Capacity, mMaxDepth, Depth + 1);
        Bounds bottomLeftBackwardBounds = new Bounds(Bounds.center + (new Vector3(-Bounds.size.x, -Bounds.size.y, -Bounds.size.z) * 0.25f), Bounds.size * 0.5f);
        OctTree<T> OctantBottomLeftBackward = new OctTree<T>(bottomLeftBackwardBounds, Capacity, mMaxDepth, Depth + 1);
        Bounds bottomRightBackwardBounds = new Bounds(Bounds.center + (new Vector3(Bounds.size.x, -Bounds.size.y, -Bounds.size.z) * 0.25f), Bounds.size * 0.5f);
        OctTree<T> OctantBottomRightBackward = new OctTree<T>(bottomRightBackwardBounds, Capacity, mMaxDepth, Depth + 1);
        OctantTopLeftForward.mParentQuad = OctantTopRightForward.mParentQuad = OctantBottomLeftForward.mParentQuad = OctantBottomRightForward.mParentQuad = OctantTopLeftBackward.mParentQuad = OctantTopRightBackward.mParentQuad = OctantBottomLeftBackward.mParentQuad = OctantBottomRightBackward.mParentQuad = this;
        SubOctant.Add(OctantTopLeftForward);
        SubOctant.Add(OctantTopRightForward);
        SubOctant.Add(OctantBottomLeftForward);
        SubOctant.Add(OctantBottomRightForward);
        SubOctant.Add(OctantTopLeftBackward);
        SubOctant.Add(OctantTopRightBackward);
        SubOctant.Add(OctantBottomLeftBackward);
        SubOctant.Add(OctantBottomRightBackward);
        foreach (var itemData in mStoredItems)
        {
            InsertToSubOctant(itemData);
        }
        mStoredItems.Clear();
    }

    private void Reconstruct()
    {
        foreach (var Octant in SubOctant)
        {
            mStoredItems.AddRange(Octant.mStoredItems);
        }
        SubOctant.Clear();
    }

    public int GetItemCount()
    {
        int count = 0;
        if(SubOctant.Count > 0)
        {
            foreach (var Octant in SubOctant)
            {
                count += Octant.GetItemCount();
            }
        }
        else
        {
            count += mStoredItems.Count;
        }
        return count;
    }

    public bool Find(Vector3 position, out T foundItem)
    {
        foundItem = default(T);
        foreach (var itemData in FindSmallestOctantAt(position).mStoredItems)
        {
            if (itemData.position == position)
            {
                foundItem = itemData.item;
                return true;
            }
        } 
        return false;
    }

    private OctTree<T> FindSmallestOctantAt(Vector3 position)
    {
        OctTree<T> foundQuad = null;
        if (Bounds.Contains(position))
        {
            if (SubOctant.Count > 0)
            {
                foreach (var Octant in SubOctant)
                {
                    if (Octant.Bounds.Contains(position))
                    {
                        return Octant.FindSmallestOctantAt(position);
                    }
                }
            }
            else
            {
                return foundQuad = this;
            }
        }
        else
        {
            throw new System.ArgumentOutOfRangeException("item.Position", "Attempting to find item outside Octant bounds!!");
        }
        return null;
    }


    public List<T> RangeSearch(Bounds searchBound)
    {
        List<T> foundItems = new List<T>();
        if(CheckInterection(Bounds, searchBound))
        {
            if(SubOctant.Count > 0)
            {
                foreach (var Octant in SubOctant)
                {
                    foundItems.AddRange(Octant.RangeSearch(searchBound));
                }
            }
            else
            {
                foreach (var itemData in mStoredItems)
                {
                    if (searchBound.Contains(itemData.position)){
                        foundItems.Add(itemData.item);
                    }
                }
            }
        }
        return foundItems;
    }

    private bool CheckInterection(Bounds rectA, Bounds rectB)
    {
        return rectA.min.x <= rectB.max.x &&
               rectA.max.x >= rectB.min.x &&
               rectA.min.y <= rectB.max.y &&
               rectA.max.y >= rectB.min.y &&
               rectA.min.z <= rectB.max.z &&
               rectA.max.z >= rectB.min.z;
    }
}
