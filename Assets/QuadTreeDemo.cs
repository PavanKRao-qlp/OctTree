using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeDemo : MonoBehaviour
{
    [SerializeField] Rect _Bounds;
    [SerializeField] uint _Capacity;
    [Min(0)] [SerializeField] int _MaxDepth;
    [SerializeField] TestItem _SpherePrefab;

    QuadTree<TestItem> mQuadTree;
    List<TestItem> mFoundItems;
    Rect mSearchRect;

    void Start()
    {
        mQuadTree = new QuadTree<TestItem>(_Bounds, _Capacity, _MaxDepth);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            StartCoroutine("QuadTreeInsert");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine("QuadTreeRemove");
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mSearchRect = new Rect(mousePos.x - 10, mousePos.y - 10, 20 , 20);
            mFoundItems = mQuadTree.RangeSearch(mSearchRect);
        }
    }

    IEnumerator QuadTreeInsert()
    {
        for (int i = 0; i < 75; i++)
        {
            Vector3 position = new Vector3(Random.Range(_Bounds.xMin, _Bounds.xMax), Random.Range(_Bounds.yMin, _Bounds.yMax));
            TestItem sphere = GameObject.Instantiate(_SpherePrefab, position, Quaternion.identity);
            mQuadTree.Insert((sphere, sphere.Position));
            yield return new WaitForSeconds(0.1f);
        }       
    }

    IEnumerator QuadTreeRemove()
    {
        foreach (var item in mFoundItems)
        {
            mQuadTree.Remove((item,item.Position));
            GameObject.Destroy(item.gameObject);
            yield return new WaitForSeconds(1);
        }
        mFoundItems.Clear();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(_Bounds.center, _Bounds.size*1.01f);
        Gizmos.color = Color.magenta;
        if(mQuadTree != null) DrawQuads(mQuadTree);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(mSearchRect.center, mSearchRect.size);
        if (mFoundItems != null)
        {
            foreach (var item in mFoundItems)
            {
                if (item == null)
                    continue;
                Gizmos.DrawWireSphere(item.Position, 1f);
            }
        }
    }

    void DrawQuads(QuadTree<TestItem> tree)
    {
        Gizmos.DrawWireCube(tree.Bounds.center, tree.Bounds.size * 1f);
        foreach (var quad in tree.SubQuads)
            DrawQuads(quad);        
    }
}
