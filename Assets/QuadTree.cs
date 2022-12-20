using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree<T>
{
    public Rect Bounds { get; private set; }
    public uint Capacity { get; private set; }
    public int Depth { get; private set; }
    public int Count => GetItemCount();
    public List<QuadTree<T>> SubQuads;
    private List<(T item, Vector2 position)> mStoredItems;
    private int mMaxDepth;
    private QuadTree<T> mParentQuad;

    public QuadTree(Rect bounds, uint capacity, int maxDepth, int depth = 0)
    {
        Bounds = bounds;
        Capacity = capacity;
        Depth = depth;
        mMaxDepth = maxDepth;
        SubQuads = new List<QuadTree<T>>();
        mStoredItems = new List<(T item, Vector2 position)>();
    }

    public void Insert((T item, Vector2 position) itemData)
    {
        if (Bounds.Contains(itemData.position))
        {
            if (SubQuads.Count > 0 && Depth < mMaxDepth)
                InsertToSubquad(itemData);
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
            throw new System.ArgumentOutOfRangeException("item.Position", "Attempting to insert item outside quad bounds!!");
        }
    }
    public void Remove((T item, Vector2 position) itemData)
    {
        if (Bounds.Contains(itemData.position))
        {
            QuadTree<T> smallestQuad = FindSmallestQuadAt(itemData.position);
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
            throw new System.ArgumentOutOfRangeException("item.Position", "Attempting to remove item outside quad bounds!!");
        }
    }

    private void InsertToSubquad((T item, Vector2 position) itemData)
    {
        foreach (var quad in SubQuads)
        {
            if (quad.Bounds.Contains(itemData.position))
            {
                quad.Insert(itemData);
                break;
            }
        }
    }


    private void Subdivide()
    {
        Rect topLeftBounds = new Rect(new Vector2(Bounds.min.x, Bounds.min.y), Bounds.size * 0.5f);
        QuadTree<T> quadTopLeft = new QuadTree<T>(topLeftBounds, Capacity, mMaxDepth,  Depth+1);
        Rect topRightBounds = new Rect(new Vector2(Bounds.center.x, Bounds.min.y), Bounds.size * 0.5f);
        QuadTree<T> quadTopRight = new QuadTree<T>(topRightBounds, Capacity, mMaxDepth, Depth + 1);
        Rect bottomLeftBounds = new Rect(new Vector2(Bounds.min.x, Bounds.center.y), Bounds.size * 0.5f);
        QuadTree<T> quadBottomLeft = new QuadTree<T>(bottomLeftBounds, Capacity, mMaxDepth, Depth + 1);
        Rect bottomRightBounds = new Rect(new Vector2(Bounds.center.x, Bounds.center.y), Bounds.size * 0.5f);
        QuadTree<T> quadBottomRight = new QuadTree<T>(bottomRightBounds, Capacity, mMaxDepth, Depth + 1);
        quadTopLeft.mParentQuad = quadTopRight.mParentQuad = quadBottomLeft.mParentQuad = quadBottomRight.mParentQuad = this;
        SubQuads.Add(quadTopLeft);
        SubQuads.Add(quadTopRight);
        SubQuads.Add(quadBottomLeft);
        SubQuads.Add(quadBottomRight);
        foreach (var itemData in mStoredItems)
        {
            InsertToSubquad(itemData);
        }
        mStoredItems.Clear();
    }

    private void Reconstruct()
    {
        foreach (var quad in SubQuads)
        {
            mStoredItems.AddRange(quad.mStoredItems);
        }
        SubQuads.Clear();
    }

    public int GetItemCount()
    {
        int count = 0;
        if(SubQuads.Count > 0)
        {
            foreach (var quad in SubQuads)
            {
                count += quad.GetItemCount();
            }
        }
        else
        {
            count += mStoredItems.Count;
        }
        return count;
    }

    public bool Find(Vector2 position, out T foundItem)
    {
        foundItem = default(T);
        foreach (var itemData in FindSmallestQuadAt(position).mStoredItems)
        {
            if (itemData.position == position)
            {
                foundItem = itemData.item;
                return true;
            }
        } 
        return false;
    }

    private QuadTree<T> FindSmallestQuadAt(Vector2 position)
    {
        QuadTree<T> foundQuad = null;
        if (Bounds.Contains(position))
        {
            if (SubQuads.Count > 0)
            {
                foreach (var quad in SubQuads)
                {
                    if (quad.Bounds.Contains(position))
                    {
                        return quad.FindSmallestQuadAt(position);
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
            throw new System.ArgumentOutOfRangeException("item.Position", "Attempting to find item outside quad bounds!!");
        }
        return null;
    }


    public List<T> RangeSearch(Rect searchBound)
    {
        List<T> foundItems = new List<T>();
        if(CheckInterection(Bounds, searchBound))
        {
            if(SubQuads.Count > 0)
            {
                foreach (var quad in SubQuads)
                {
                    foundItems.AddRange(quad.RangeSearch(searchBound));
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

    private bool CheckInterection(Rect rectA, Rect rectB)
    {
        return rectA.xMin <= rectB.xMax &&
               rectA.xMax >= rectB.xMin &&
               rectA.yMin <= rectB.yMax &&
               rectA.yMax >= rectB.yMin;
    }
}
