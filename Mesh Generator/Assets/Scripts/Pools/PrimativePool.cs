using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimativePool : MonoBehaviour
{
    public static PrimativePool Instance;
    private List<GameObject> objectPool;
    public PrimitiveType primitiveType;
    public int poolSize;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }


    // Start is called before the first frame update
    void Start()
    {
        objectPool = new List<GameObject>();

        for (int i = 0; i < poolSize; ++i)
        {
            var obj = GameObject.CreatePrimitive(primitiveType);
            obj.SetActive(false);
            
            Billboard bb = obj.AddComponent(typeof(Billboard)) as Billboard;
            bb.target = Camera.main.transform;
            
            objectPool.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < poolSize; ++i)
        {
            if (!objectPool[i].activeInHierarchy)
            {
                var obj = objectPool[i];
                obj.SetActive(true);
                return obj;
            }
        }

        return null;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (!objectPool.Contains(obj)) return;
        obj.SetActive(false);
    }
}
