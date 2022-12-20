using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctTreeDemo : MonoBehaviour
{
    [SerializeField] Bounds _Bounds;
    [SerializeField] uint _Capacity;
    [Min(0)] [SerializeField] int _MaxDepth;
    [SerializeField] TestItem _SpherePrefab;

    OctTree<TestItem> mOctTree;
    List<TestItem> mFoundItems;
    Bounds mSearchRect;

    void Start()
    {
        mOctTree = new OctTree<TestItem>(_Bounds, _Capacity, _MaxDepth);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            StartCoroutine("OctTreeInsert");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine("OctTreeRemove");
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 position = new Vector3(Random.Range(_Bounds.min.x, _Bounds.max.x), Random.Range(_Bounds.min.y, _Bounds.max.y), Random.Range(_Bounds.min.z, _Bounds.max.z));
            mSearchRect = new Bounds(position, Vector3.one * 10);
            mFoundItems = mOctTree.RangeSearch(mSearchRect);
        }
    }

    IEnumerator OctTreeInsert()
    {
        for (int i = 0; i < 75; i++)
        {
            Vector3 position = new Vector3(Random.Range(_Bounds.min.x, _Bounds.max.x), Random.Range(_Bounds.min.y, _Bounds.max.y), Random.Range(_Bounds.min.z, _Bounds.max.z));
            TestItem sphere = GameObject.Instantiate(_SpherePrefab, position, Quaternion.identity);
            mOctTree.Insert((sphere, sphere.Position));
            yield return new WaitForSeconds(0.1f);
        }       
    }

    IEnumerator OctTreeRemove()
    {
        foreach (var item in mFoundItems)
        {
            mOctTree.Remove((item,item.Position));
            GameObject.Destroy(item.gameObject);
            yield return new WaitForSeconds(1);
        }
        mFoundItems.Clear();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(_Bounds.center, _Bounds.size*1.01f);
        Gizmos.color = Color.magenta;
        if(mOctTree != null) DrawOctant(mOctTree);
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

    void DrawOctant(OctTree<TestItem> tree)
    {
        Gizmos.DrawWireCube(tree.Bounds.center, tree.Bounds.size * 1f);
        foreach (var octant in tree.SubOctant)
            DrawOctant(octant);        
    }
}
